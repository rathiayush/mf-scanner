using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MutualFundScanner.Application.DTOs;
using MutualFundScanner.Application.Interfaces;
using MutualFundScanner.Domain.Entities;
using MutualFundScanner.Infrastructure.Data;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using Microsoft.AspNetCore.Http;
using MutualFundScanner.Infrastructure.Repositories;

namespace MutualFundScanner.Application.Services;

public class MutualFundDetectionService
{
    private readonly MutualFundContext _context;
    private readonly IConcreteMutualFundRepository _concreteMutualFundRepository;
    private readonly ILogger<MutualFundDetectionService> _logger;
    private List<ConcreteMutualFund>? _cachedMutualFunds;
    private DateTime _lastCacheUpdate = DateTime.MinValue;
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(30);

    public MutualFundDetectionService(
        MutualFundContext context, 
        IConcreteMutualFundRepository concreteMutualFundRepository,
        ILogger<MutualFundDetectionService> logger)
    {
        _context = context;
        _concreteMutualFundRepository = concreteMutualFundRepository;
        _logger = logger;
    }

    public async Task<MutualFundDetectionResult> DetectMutualFundFromFile(IFormFile file)
    {
        var result = new MutualFundDetectionResult
        {
            FileName = file.FileName
        };

        try
        {
            using var stream = file.OpenReadStream();
            var workbook = CreateWorkbook(stream, file.FileName);
            var worksheet = workbook.GetSheetAt(0);

            // Scan first 50 rows across all columns
            var detectedText = await ScanWorksheetForMutualFundAsync(worksheet);
            
            if (!string.IsNullOrEmpty(detectedText.FundName))
            {
                result.DetectedFundName = detectedText.FundName;
                result.MatchedText = detectedText.MatchedText;
                result.RowFound = detectedText.Row;
                result.ColumnFound = detectedText.Column;
                result.MatchConfidence = detectedText.Confidence;

                // Check if mutual fund exists
                var existingFund = await FindExistingMutualFund(detectedText.FundName);
                
                if (existingFund != null)
                {
                    result.MutualFundId = existingFund.Id;
                    result.WasCreated = false;
                }
                else
                {
                    // Create new mutual fund
                    var newFund = await CreateMutualFund(detectedText.FundName);
                    result.MutualFundId = newFund.Id;
                    result.WasCreated = true;
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting mutual fund from file {FileName}", file.FileName);
            result.DetectedFundName = "Error processing file";
            return result;
        }
    }

    /// <summary>
    /// Detects mutual fund from a specific sheet in a workbook
    /// </summary>
    /// <param name="workbook">The Excel workbook</param>
    /// <param name="sheetIndex">The index of the sheet to process</param>
    /// <param name="fileName">The original file name for logging</param>
    /// <returns>Detection result</returns>
    public async Task<MutualFundDetectionResult> DetectMutualFundFromSheet(IWorkbook workbook, int sheetIndex, string fileName)
    {
        var result = new MutualFundDetectionResult
        {
            FileName = fileName
        };

        try
        {
            if (workbook == null || sheetIndex < 0 || sheetIndex >= workbook.NumberOfSheets)
            {
                result.DetectedFundName = "Invalid workbook or sheet index";
                return result;
            }

            var worksheet = workbook.GetSheetAt(sheetIndex);

            // Scan first 50 rows across all columns
            var detectedText = await ScanWorksheetForMutualFundAsync(worksheet);
            
            if (!string.IsNullOrEmpty(detectedText.FundName))
            {
                result.DetectedFundName = detectedText.FundName;
                result.MatchedText = detectedText.MatchedText;
                result.RowFound = detectedText.Row;
                result.ColumnFound = detectedText.Column;
                result.MatchConfidence = detectedText.Confidence;

                // Check if mutual fund already exists
                var existingFund = await FindExistingMutualFund(detectedText.FundName);
                if (existingFund != null)
                {
                    result.MutualFundId = existingFund.Id;
                    result.WasCreated = false;
                }
                else
                {
                    // Create new mutual fund
                    var newFund = await CreateMutualFund(detectedText.FundName);
                    result.MutualFundId = newFund.Id;
                    result.WasCreated = true;
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting mutual fund from sheet {SheetIndex} in file {FileName}", sheetIndex, fileName);
            result.DetectedFundName = "Error processing sheet";
            return result;
        }
    }

    private IWorkbook CreateWorkbook(Stream stream, string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".xlsx" => new XSSFWorkbook(stream),
            ".xls" => new HSSFWorkbook(stream),
            _ => throw new ArgumentException($"Unsupported file format: {extension}")
        };
    }

    private async Task<(string FundName, string MatchedText, int Row, int Column, double Confidence)> ScanWorksheetForMutualFundAsync(ISheet worksheet)
    {
        // First, check if this sheet has the required column headers for mutual fund data
        if (!HasRequiredMutualFundColumns(worksheet))
        {
            return (string.Empty, string.Empty, -1, -1, 0.0);
        }

        var maxRows = Math.Min(5, worksheet.LastRowNum + 1);
        
        for (int row = 0; row < maxRows; row++)
        {
            var dataRow = worksheet.GetRow(row);
            if (dataRow == null) continue;

            var maxCols = dataRow.LastCellNum;
            for (int col = 0; col < maxCols; col++)
            {
                var cell = dataRow.GetCell(col);
                if (cell == null) continue;

                var cellValue = GetCellValue(cell)?.Trim();
                if (string.IsNullOrEmpty(cellValue)) continue;

                // Check for exact match against mutual fund patterns
                var exactMatch = await FindExactMatchAsync(cellValue);
                if (!string.IsNullOrEmpty(exactMatch))
                {
                    return (exactMatch, cellValue, row, col, 1.0);
                }
            }
        }

        return (string.Empty, string.Empty, -1, -1, 0.0);
    }

    /// <summary>
    /// Checks if the worksheet contains any of the required column headers for mutual fund data
    /// Required columns: Name of the Instrument, ISIN, Industry, % to Net, NAV
    /// </summary>
    /// <param name="worksheet">The worksheet to check</param>
    /// <returns>True if any required column is found</returns>
    private bool HasRequiredMutualFundColumns(ISheet worksheet)
    {
        var requiredColumns = new[]
        {
            "name of the instrument",
            "isin",
            "industry",
            "% to net",
            "nav"
        };

        var foundColumns = new HashSet<string>();
        var maxRowsToCheck = Math.Min(10, worksheet.LastRowNum + 1); // Check first 10 rows for headers

        for (int row = 0; row < maxRowsToCheck; row++)
        {
            var dataRow = worksheet.GetRow(row);
            if (dataRow == null) continue;

            var maxCols = dataRow.LastCellNum;
            for (int col = 0; col < maxCols; col++)
            {
                var cell = dataRow.GetCell(col);
                if (cell == null) continue;

                var cellValue = GetCellValue(cell)?.Trim().ToLowerInvariant();
                if (string.IsNullOrEmpty(cellValue)) continue;

                // Check for exact matches and common variations
                foreach (var requiredColumn in requiredColumns)
                {
                    if (cellValue == requiredColumn)
                    {
                        _logger.LogDebug("Found exact match: '{CellValue}' matches required column '{RequiredColumn}'", 
                            cellValue, requiredColumn);
                        return true;
                    }
                    else if (cellValue.Contains(requiredColumn))
                    {
                        _logger.LogDebug("Found partial match: '{CellValue}' contains required column '{RequiredColumn}'", 
                            cellValue, requiredColumn);
                        return true;
                    }
                    else if (IsColumnVariation(cellValue, requiredColumn))
                    {
                        _logger.LogDebug("Found variation match: '{CellValue}' is a variation of required column '{RequiredColumn}'", 
                            cellValue, requiredColumn);
                        return true;
                    }
                }
            }
        }

        // Log that no qualifying columns were found
        _logger.LogDebug("No qualifying mutual fund columns found in worksheet");
        return false;
    }

    /// <summary>
    /// Checks for common variations of column names
    /// </summary>
    /// <param name="cellValue">The cell value to check</param>
    /// <param name="requiredColumn">The required column name</param>
    /// <returns>True if the cell value is a variation of the required column</returns>
    private bool IsColumnVariation(string cellValue, string requiredColumn)
    {
        return requiredColumn switch
        {
            "name of the instrument" => cellValue.Contains("instrument") || 
                                       cellValue.Contains("security") || 
                                       cellValue.Contains("company") ||
                                       cellValue == "name",
            "isin" => cellValue == "isin code" || 
                     cellValue == "isin number" ||
                     cellValue.Contains("isin"),
            "industry" => cellValue.Contains("sector") || 
                         cellValue.Contains("industry"),
            "% to net" => cellValue.Contains("weight") || 
                         cellValue.Contains("percentage") ||
                         cellValue.Contains("% to") ||
                         cellValue.Contains("net %") ||
                         cellValue == "weight %",
            "nav" => cellValue == "net asset value" || 
                    cellValue.Contains("nav") ||
                    cellValue == "price",
            _ => false
        };
    }

    private string? GetCellValue(ICell cell)
    {
        return cell.CellType switch
        {
            CellType.String => cell.StringCellValue,
            CellType.Numeric => cell.NumericCellValue.ToString(),
            CellType.Boolean => cell.BooleanCellValue.ToString(),
            CellType.Formula => cell.CachedFormulaResultType switch
            {
                CellType.String => cell.StringCellValue,
                CellType.Numeric => cell.NumericCellValue.ToString(),
                _ => null
            },
            _ => null
        };
    }

    private async Task<List<ConcreteMutualFund>> GetCachedMutualFundsAsync()
    {
        if (_cachedMutualFunds == null || DateTime.UtcNow - _lastCacheUpdate > _cacheExpiry)
        {
            _cachedMutualFunds = (await _concreteMutualFundRepository.GetActiveForDetectionAsync()).ToList();
            _lastCacheUpdate = DateTime.UtcNow;
            _logger.LogInformation("Refreshed mutual fund cache with {Count} active funds", _cachedMutualFunds.Count);
        }
        
        return _cachedMutualFunds;
    }

    private async Task<string> FindExactMatchAsync(string cellText)
    {
        if (string.IsNullOrEmpty(cellText)) return string.Empty;

        var normalizedCellText = cellText.ToLowerInvariant().Trim();
        var mutualFunds = await GetCachedMutualFundsAsync();

        foreach (var fund in mutualFunds)
        {
            var normalizedPattern = fund.FundName.ToLowerInvariant().Trim();
            
            // Check for exact match
            if (normalizedCellText == normalizedPattern)
            {
                return fund.FundName;
            }
            
            // Check if cell text contains the exact pattern
            if (normalizedCellText.Contains(normalizedPattern))
            {
                return fund.FundName;
            }
        }

        return string.Empty;
    }


    private async Task<MutualFund?> FindExistingMutualFund(string fundName)
    {
        // Try exact match first
        var exactMatch = await _context.MutualFunds
            .FirstOrDefaultAsync(f => f.FundName.ToLower() == fundName.ToLower());

        return exactMatch;
    }

    private async Task<MutualFund> CreateMutualFund(string fundName)
    {
        var newFund = new MutualFund
        {
            FundName = fundName,
            FundCode = GenerateFundCode(fundName),
           
            Category = "Equity", // Default category
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };

        _context.MutualFunds.Add(newFund);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created new mutual fund: {FundName} with ID {FundId}", 
            newFund.FundName, newFund.Id);

        return newFund;
    }

    private string GenerateFundCode(string fundName)
    {
        if (string.IsNullOrEmpty(fundName)) return "UNKNOWN";

        var words = fundName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var code = new System.Text.StringBuilder();

        foreach (var word in words)
        {
            if (string.IsNullOrEmpty(word)) continue;
            
            // Take first 3 characters from each word
            var part = word.Substring(0, Math.Min(3, word.Length));
            code.Append(part);
        }

        return code.Length > 0 ? code.ToString() : "UNKNOWN";
    }

    private string ExtractAMCName(string fundName)
    {
        var amcPatterns = new Dictionary<string, string>
        {
            { "HDFC", "HDFC Asset Management Company Limited" },
            { "ICICI", "ICICI Prudential Asset Management Company Limited" },
            { "SBI", "SBI Funds Management Private Limited" },
            { "Axis", "Axis Asset Management Company Ltd" },
            { "Kotak", "Kotak Mahindra Asset Management Company Limited" },
            { "Nippon", "Nippon Life India Asset Management Limited" },
            { "Reliance", "Nippon Life India Asset Management Limited" },
            { "Aditya Birla", "Aditya Birla Sun Life AMC Limited" },
            { "UTI", "UTI Asset Management Company Limited" },
            { "DSP", "DSP Investment Managers Private Limited" },
            { "Franklin", "Franklin Templeton Asset Management (India) Private Ltd" },
            { "Mirae", "Mirae Asset Investment Managers (India) Private Limited" },
            { "Tata", "Tata Asset Management Private Limited" }
        };

        foreach (var amc in amcPatterns)
        {
            if (fundName.Contains(amc.Key, StringComparison.OrdinalIgnoreCase))
            {
                return amc.Value;
            }
        }

        return "Unknown AMC";
    }
}
