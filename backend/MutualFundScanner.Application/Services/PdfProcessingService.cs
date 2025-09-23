using MutualFundScanner.Application.Interfaces;
using MutualFundScanner.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Text.RegularExpressions;

namespace MutualFundScanner.Application.Services;

public class PdfProcessingService : IPdfProcessingService
{
    private readonly ILogger<PdfProcessingService> _logger;

    public PdfProcessingService(ILogger<PdfProcessingService> logger)
    {
        _logger = logger;
    }

    public async Task<PdfProcessingResponseDto> ProcessPdfFileAsync(PdfProcessingRequestDto request)
    {
        var result = new PdfProcessingResponseDto();

        try
        {
            // Validate PDF
            if (!await ValidatePdfFileAsync(request.File))
            {
                result.Success = false;
                result.Message = "Invalid PDF file.";
                result.Errors.Add("File validation failed.");
                return result;
            }

            // Extract text from PDF
            var pages = await ExtractTextFromPdfAsync(request.File);
            if (!pages.Any())
            {
                result.Success = false;
                result.Message = "No text extracted from PDF.";
                result.Errors.Add("PDF text extraction failed.");
                return result;
            }

            // Detect all funds per page
            var detectedFunds = new List<string>();
            var targetPages = new List<PdfPageData>();

            foreach (var page in pages)
            {
                var fundsOnPage = await DetectFundNamesOnPageAsync(page, PdfProcessingRequestDto.KnownFundNames);
                if (fundsOnPage.Any() && page.ContainsTargetTable)
                {
                    targetPages.Add(page);
                    foreach (var f in fundsOnPage)
                    {
                        if (!detectedFunds.Contains(f.FundName))
                            detectedFunds.Add(f.FundName);
                    }
                    _logger.LogInformation($"Page {page.PageNumber} contains fund(s): {string.Join(", ", fundsOnPage.Select(f => f.FundName))}");
                }
            }

            if (!targetPages.Any())
            {
                result.Success = false;
                result.Message = "No pages found with fund name and target table.";
                result.Errors.Add("No target pages detected in PDF.");
                return result;
            }

            result.DetectedFundName = detectedFunds.FirstOrDefault();

            // Extract tables
            var tables = new List<PdfExtractedTableData>();
            foreach (var page in targetPages)
            {
                var pageTables = await ExtractIssuerInstrumentTableAsync(page);
                tables.AddRange(pageTables);
            }

            result.ExtractedTables = tables;

            // Generate Excel files
            if (!string.IsNullOrEmpty(result.DetectedFundName))
            {
                var outputFolder = request.OutputFolder ?? Path.Combine(Directory.GetCurrentDirectory(), "PdfExports");
                result.CreatedExcelFiles = await GenerateExcelReportsAsync(detectedFunds, tables, outputFolder);
            }

            result.Success = true;
            result.Message = $"PDF processed successfully. Found {targetPages.Count} target page(s) with {tables.Count} table(s).";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PDF file.");
            result.Success = false;
            result.Message = "Error processing PDF file.";
            result.Errors.Add(ex.Message);
        }

        return result;
    }

    public async Task<List<PdfPageData>> ExtractTextFromPdfAsync(IFormFile file)
    {
        var pages = new List<PdfPageData>();

        try
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var pdfReader = new PdfReader(stream);
            using var pdfDocument = new PdfDocument(pdfReader);

            for (int pageNum = 1; pageNum <= pdfDocument.GetNumberOfPages(); pageNum++)
            {
                var page = pdfDocument.GetPage(pageNum);
                var strategy = new LocationTextExtractionStrategy();
                var text = PdfTextExtractor.GetTextFromPage(page, strategy);

                var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(l => l.Trim())
                                .Where(l => !string.IsNullOrWhiteSpace(l))
                                .ToList();

                pages.Add(new PdfPageData
                {
                    PageNumber = pageNum,
                    Text = text,
                    Lines = lines,
                    ContainsTargetTable = lines.Any(line =>
                        line.Contains("Issuer", StringComparison.OrdinalIgnoreCase) &&
                        line.Contains("Instrument", StringComparison.OrdinalIgnoreCase) &&
                        line.Contains("Net Assets", StringComparison.OrdinalIgnoreCase))
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from PDF.");
            throw;
        }

        return pages;
    }

    // Detect multiple funds per page
    public Task<List<FundDetectionResult>> DetectFundNamesOnPageAsync(PdfPageData page, List<string> knownFundNames)
    {
        var results = new List<FundDetectionResult>();
        var pageText = page.Text;

        foreach (var fundName in knownFundNames)
        {
            var patterns = new List<string>
            {
                fundName,
                Regex.Escape(fundName),
                fundName.Replace(" ", @"\s*"),
                fundName.Replace(" ", "-"),
                fundName.Replace(" ", "_")
            };

            foreach (var pattern in patterns)
            {
                var regex = new Regex(pattern, RegexOptions.IgnoreCase);
                var matches = regex.Matches(pageText);

                if (matches.Count > 0)
                {
                    results.Add(new FundDetectionResult
                    {
                        FundName = fundName,
                        ConfidenceScore = matches.Count * 10 + fundName.Length,
                        MatchedText = matches[0].Value,
                        PageNumber = page.PageNumber
                    });
                    break; // move to next fund once a match is found
                }
            }
        }

        return Task.FromResult(results);
    }

    public async Task<List<PdfExtractedTableData>> ExtractIssuerInstrumentTableAsync(PdfPageData page)
    {
        var tables = new List<PdfExtractedTableData>();

        try
        {
            var tableData = new PdfExtractedTableData
            {
                PageNumber = page.PageNumber,
                TableName = "Issuer/Instrument % to Net Assets",
                Rows = new List<PdfTableRow>()
            };

            foreach (var line in page.Lines)
            {
                var cleanLine = line.Trim();
                if (string.IsNullOrWhiteSpace(cleanLine)) continue;
                
                // Skip headers, footers, and non-security data
                if (IsNonSecurityLine(cleanLine)) continue;
                
                // Extract multiple securities from same line if present
                var rows = ExtractMultipleSecurities(cleanLine);
                tableData.Rows.AddRange(rows);
            }

            if (tableData.Rows.Any())
            {
                tables.Add(tableData);
                _logger.LogInformation($"Extracted {tableData.Rows.Count} clean securities from page {page.PageNumber}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error extracting table from page {page.PageNumber}");
        }

        return tables;
    }

    private bool IsNonSecurityLine(string line)
    {
        var skipPatterns = new[]
        {
            @"^(Issuer|Instrument|Net Assets|Total|Subtotal|Grand Total)",
            @"^(Equity & Equity related|Banks|IT - Software|Automobiles|Petroleum Products)",
            @"^(Pharmaceuticals|Power|Cement|Transport|Fertilizers|Beverages|Chemicals)",
            @"^(Diversified FMCG|Auto Components|Consumer Durables|Aerospace|Non - Ferrous)",
            @"^(Gas|Healthcare|Insurance|Construction|Telecom|Retailing|Finance)",
            @"Allotment Date:",
            @"Folio Count:",
            @"Growth `",
            @"IDCW `",
            @"as on (August|January)",
            @"Sharpe##",
            @"Standard Deviation",
            @"P/E|P/BV",
            @"Direct Plan:",
            @"Exit Load:",
            @"Scheme Returns",
            @"purchased or switched",
            @"date of allotment:",
            @"Data as on",
            @"Product Label",
            @"^\d+\.\d+%?\s*$", // Just percentages
            @"^[\d\s,.-]+$", // Just numbers and punctuation
            @"Total amount invested",
            @"redemption / switch"
        };

        return skipPatterns.Any(pattern => Regex.IsMatch(line, pattern, RegexOptions.IgnoreCase));
    }

    private List<PdfTableRow> ExtractMultipleSecurities(string line)
    {
        var securities = new List<PdfTableRow>();
        
        try
        {
            // Pattern to find company names followed by percentages
            // Look for patterns like "COMPANY NAME LTD. 5.20" or "COMPANY NAME LIMITED 2.30"
            var pattern = @"([A-Z][A-Za-z\s&.-]+(?:LTD\.?|LIMITED|CORPORATION|CORP\.?|INC\.?|INDUSTRIES|BANK|FINANCE))\s+(\d+\.\d+)";
            var matches = Regex.Matches(line, pattern);
            
            foreach (Match match in matches)
            {
                var companyName = match.Groups[1].Value.Trim();
                var percentage = match.Groups[2].Value;
                
                // Clean up company name
                companyName = CleanCompanyName(companyName);
                
                if (!string.IsNullOrWhiteSpace(companyName) && companyName.Length > 3)
                {
                    securities.Add(new PdfTableRow
                    {
                        Issuer = companyName,
                        Percentage = percentage,
                        RawText = line
                    });
                }
            }
            
            // If no matches with the strict pattern, try a simpler approach
            if (!securities.Any())
            {
                var simpleMatch = Regex.Match(line, @"^([A-Z][A-Za-z\s&.-]+?)\s+(\d+\.\d+)\s*$");
                if (simpleMatch.Success)
                {
                    var companyName = CleanCompanyName(simpleMatch.Groups[1].Value.Trim());
                    var percentage = simpleMatch.Groups[2].Value;
                    
                    if (!string.IsNullOrWhiteSpace(companyName) && companyName.Length > 3)
                    {
                        securities.Add(new PdfTableRow
                        {
                            Issuer = companyName,
                            Percentage = percentage,
                            RawText = line
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug($"Failed to parse securities from line: {line}. Error: {ex.Message}");
        }
        
        return securities;
    }
    
    private string CleanCompanyName(string name)
    {
        // Remove extra spaces and clean up
        name = Regex.Replace(name, @"\s+", " ").Trim();
        
        // Remove trailing punctuation except for LTD. etc
        name = Regex.Replace(name, @"[,;:-]+$", "").Trim();
        
        return name;
    }

    public async Task<List<CreatedExcelFile>> GenerateExcelReportsAsync(List<string> fundNames, List<PdfExtractedTableData> tableData, string outputFolder)
    {
        var createdFiles = new List<CreatedExcelFile>();

        try
        {
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            // Group tables by page
            var tablesByPage = tableData.GroupBy(t => t.PageNumber);

            foreach (var pageGroup in tablesByPage)
            {
                var pageNumber = pageGroup.Key;
                var pageTables = pageGroup.ToList();

                foreach (var fundName in fundNames)
                {
                    var fileName = $"{fundName.Replace(" ", "_")}_Page_{pageNumber}_Holdings_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    var filePath = Path.Combine(outputFolder, fileName);

                    using var workbook = new XSSFWorkbook();
                    var sheet = workbook.CreateSheet($"Page_{pageNumber}");

                    // Styles
                    var headerStyle = workbook.CreateCellStyle();
                    var font = workbook.CreateFont();
                    font.IsBold = true;
                    font.FontHeightInPoints = 12;
                    headerStyle.SetFont(font);

                    var titleStyle = workbook.CreateCellStyle();
                    var titleFont = workbook.CreateFont();
                    titleFont.IsBold = true;
                    titleFont.FontHeightInPoints = 14;
                    titleStyle.SetFont(titleFont);

                    // Title
                    var titleRow = sheet.CreateRow(0);
                    var titleCell = titleRow.CreateCell(0);
                    titleCell.SetCellValue($"{fundName} - Page {pageNumber}");
                    titleCell.CellStyle = titleStyle;
                    sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 0, 0, 2));

                    // Headers
                    var headerRow = sheet.CreateRow(2);
                    headerRow.CreateCell(0).SetCellValue("Issuer/Instrument");
                    headerRow.CreateCell(1).SetCellValue("% to Net Assets");
                    headerRow.CreateCell(2).SetCellValue("Source Page");

                    for (int i = 0; i < 3; i++)
                        headerRow.GetCell(i).CellStyle = headerStyle;

                    // Data
                    var rowNum = 3;
                    int totalRecords = 0;
                    foreach (var table in pageTables)
                    {
                        foreach (var row in table.Rows)
                        {
                            var dataRow = sheet.CreateRow(rowNum++);
                            dataRow.CreateCell(0).SetCellValue(row.Issuer);
                            dataRow.CreateCell(1).SetCellValue(row.Percentage);
                            dataRow.CreateCell(2).SetCellValue(table.PageNumber);
                            totalRecords++;
                        }
                    }

                    for (int i = 0; i < 3; i++)
                        sheet.AutoSizeColumn(i);

                    using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                    workbook.Write(fileStream);

                    createdFiles.Add(new CreatedExcelFile
                    {
                        FileName = fileName,
                        FilePath = filePath,
                        PageNumber = pageNumber,
                        RecordCount = totalRecords,
                        CreatedAt = DateTime.Now
                    });

                    _logger.LogInformation($"Created Excel file: {fileName} with {totalRecords} records.");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Excel reports.");
            throw;
        }

        return createdFiles;
    }

    public async Task<bool> ValidatePdfFileAsync(IFormFile file)
    {
        if (file == null || file.Length == 0) return false;
        if (Path.GetExtension(file.FileName).ToLower() != ".pdf") return false;

        try
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var pdfReader = new PdfReader(stream);
            using var pdfDocument = new PdfDocument(pdfReader);

            return pdfDocument.GetNumberOfPages() > 0;
        }
        catch
        {
            return false;
        }
    }
}
