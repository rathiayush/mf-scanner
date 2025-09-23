using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MutualFundScanner.Application.DTOs;
using MutualFundScanner.Domain.Entities;
using MutualFundScanner.Infrastructure.Data;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using Microsoft.AspNetCore.Http;

namespace MutualFundScanner.Application.Services;

public class MutualFundDetectionService
{
    private readonly MutualFundContext _context;
    private readonly ILogger<MutualFundDetectionService> _logger;

    // Static list of mutual fund names/patterns to match against
    // Static list of mutual fund names/patterns to match against
    private readonly List<string> _mutualFundPatterns = new()
{
    // HDFC Mutual Fund
    "HDFC NIFTY SMALLCAP 250 INDEX FUND","HDFC NIFTY MIDCAP 250 INDEX FUND",
    
    // ICICI Prudential
    "ICICI Prudential Balanced Advantage Fund", "ICICI Prudential Bluechip Fund", "ICICI Prudential Value Discovery Fund",
    "ICICI Prudential Technology Fund", "ICICI Prudential Banking and Financial Services Fund",
    "ICICI Prudential Multi-Asset Fund", "ICICI Prudential Flexi Cap Fund", "ICICI Prudential Large & Mid Cap Fund",
    
    
    
    // Axis Mutual Fund
    "Axis Bluechip Fund", "Axis Midcap Fund", "Axis Small Cap Fund", "Axis Flexi Cap Fund",
    "Axis Long Term Equity Fund", "Axis Focused 25 Fund", "Axis Banking & Financial Services Fund",
    "Axis Multi Cap Fund", "Axis Large Cap Fund", "Axis Growth Opportunities Fund",
    
    // Kotak Mutual Fund
    "Kotak Standard Multicap Fund", "Kotak Bluechip Fund", "Kotak Emerging Equity Fund", "Kotak Small Cap Fund",
    "Kotak Flexicap Fund", "Kotak Banking and Financial Services Fund", "Kotak Infrastructure & Economic Reform Fund",
    "Kotak Multi Cap Fund", "Kotak Large Cap Fund", "Kotak Mid Cap Fund",
    
    // Reliance Mutual Fund / Nippon India
    "Nippon India Large Cap Fund", "Nippon India Multi Cap Fund", "Nippon India Small Cap Fund",
    "Nippon India Banking & Financial Services Fund", "Nippon India Growth Fund", "Nippon India Power & Natural Resources Fund",
    "Reliance Large Cap Fund", "Reliance Multi Cap Fund", "Reliance Small Cap Fund",
    
    // Aditya Birla Sun Life
    "Aditya Birla Sun Life Frontline Equity Fund", "Aditya Birla Sun Life Equity Fund", "Aditya Birla Sun Life Small Cap Fund",
    "Aditya Birla Sun Life Mid Cap Fund", "Aditya Birla Sun Life Banking & Financial Services Fund",
    "Aditya Birla Sun Life Digital India Fund", "Aditya Birla Sun Life Flexi Cap Fund",

    // UTI Mutual Fund
    "UTI Nifty Fund", "UTI Mastershare Fund", "UTI Mid Cap Fund", "UTI Small Cap Fund",
    "UTI Flexi Cap Fund", "UTI Banking and Financial Services Fund", "UTI Transportation and Logistics Fund",
    
    // DSP Mutual Fund
    "DSP Equity Fund", "DSP Mid Cap Fund", "DSP Small Cap Fund", "DSP Flexi Cap Fund",
    "DSP Banking & Financial Services Fund", "DSP Healthcare Fund", "DSP Natural Resources and New Energy Fund",
    
    // Franklin Templeton
    "Franklin India Bluechip Fund", "Franklin India Equity Fund", "Franklin India Smaller Companies Fund",
    "Franklin India Mid Cap Fund", "Franklin India Banking & Financial Services Fund",
    
    // Mirae Asset
    "Mirae Asset Large Cap Fund", "Mirae Asset Emerging Bluechip Fund", "Mirae Asset Focused Fund",
    "Mirae Asset Tax Saver Fund", "Mirae Asset Banking and Financial Services Fund",
    
    // Tata Mutual Fund
    "Tata Large Cap Fund", "Tata Mid Cap Growth Fund", "Tata Small Cap Fund", "Tata Flexi Cap Fund",
    "Tata Banking and Financial Services Fund", "Tata Digital India Fund", "Tata Infrastructure Fund",
    
    // PGIM
    "PGIM INDIA RETIREMENT FUND", "PGIM INDIA SMALL CAP FUND",

    // Axis Equity Funds
    "Axis Large Cap Fund", "Axis Large & Mid Cap Fund", "Axis Midcap Fund", "Axis Momentum Fund",
    "Axis Multicap Fund", "Axis Flexi Cap Fund", "Axis Small Cap Fund", "Axis Quant Fund",
    "Axis Nifty 100 Index Fund", "Axis Nifty 500 Index Fund", "Axis Nifty Next 50 Index Fund",
    "Axis Nifty500 Momentum 50 Index Fund", "Axis Nifty500 Value 50 Index Fund",
    "Axis Nifty Midcap 50 Index Fund", "Axis Nifty Smallcap 50 Index Fund", "Axis NASDAQ 100 Fund of Fund",

    // Bajaj Finserv Mutual Fund
    "Bajaj Finserv Flexi Cap Fund", "Bajaj Finserv Balanced Advantage Fund",
    "Bajaj Finserv Arbitrage Fund", "Bajaj Finserv Large and Midcap Fund",
    "Bajaj Finserv Multi Asset Allocation Fund", "Bajaj Finserv Nifty 50 ETF", "Bajaj Finserv Nifty Bank ETF",

    //  Aditya Birla Sun Life (new equity/hybrid)
    "Aditya Birla Sun Life Business Cycle Fund", "Aditya Birla Sun Life Conglomerate Fund",
    "Aditya Birla Sun Life Multi Asset Allocation Fund", "Aditya Birla Sun Life Multi-Cap Fund",
    "Aditya Birla Sun Life Quant Fund", "Aditya Birla Sun Life Large & Mid Cap Fund",
    "Aditya Birla Sun Life Infrastructure Fund",

    // Bank of India Mutual Fund
    "Bank Of India Large & Mid Cap Equity Fund", "Bank of India Conservative Hybrid Fund",
    "Bank of India ELSS Tax Saver", "Bank Of India Manufacturing and Infrastructure Fund",
    "Bank Of India Mid & Small Cap Equity & Debt Fund", "Bank Of India Mid Cap Tax Fund Series 1",
    "Bank Of India Mid Cap Tax Fund Series 2", "Bank Of India Small Cap Fund",
    "Bank Of India Flexi Cap Fund", "Bank Of India Bluechip Fund", "Bank Of India Multi Cap Fund",
    "Bank Of India Arbitrage Fund",

    // Motilal Oswal Mutual Fund
    "Motilal Oswal Focused Fund", "Motilal Oswal Midcap Fund", "Motilal Oswal Flexi Cap Fund",
    "Motilal Oswal Balanced Advantage Fund", "Motilal Oswal ELSS Tax Saver Fund",
    "Motilal Oswal Large and Midcap Fund", "Motilal Oswal Multi Asset Fund",
    "Motilal Oswal Nifty 50 Index Fund", "Motilal Oswal Nifty 500 Index Fund",
    "Motilal Oswal Nifty Midcap 150 Index Fund", "Motilal Oswal Nifty Smallcap 250 Index Fund",
    "Motilal Oswal Nifty Bank Index Fund", "Motilal Oswal Nifty 50 ETF",
    "Motilal Oswal Nifty Midcap 100 ETF",
    // Nippon India (New Additions)
"Nippon India Nifty Auto ETF",
"Nippon India Nifty Alpha Low Volatility",
"Nippon India Innovation Fund",
"Nippon India Nifty 500 Momentum",
"Nippon India Nifty Realty",
"Nippon India Active Momentum Fund",
"Nippon India Nifty 500 Low",
"Nippon India Nifty 500 Quality 50 Index Fund",

// Parag Parikh
"Parag Parikh Flexi Cap Fund",
"Parag Parikh ELSS Tax Saver Fund",

// quant Mutual Fund
"quant Multi Cap Fund",
"quant Flexi Cap Fund",
"quant Infrastructure Fund",
"quant Small Cap Fund",
"quant Large Cap Fund",
"quant Large & Mid Cap Fund",
"quant Manufacturing Fund",
"quant ELSS Tax Saver Fund",

// SBI Mutual Fund (New Additions)
"SBI Large and Midcap Fund",
"SBI ELSS Tax Saver Fund",
"SBI Consumption Opportunities Fund",
"SBI Midcap Fund",
};


    public MutualFundDetectionService(MutualFundContext context, ILogger<MutualFundDetectionService> logger)
    {
        _context = context;
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
            var detectedText = ScanWorksheetForMutualFund(worksheet);
            
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
            var detectedText = ScanWorksheetForMutualFund(worksheet);
            
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

    private (string FundName, string MatchedText, int Row, int Column, double Confidence) ScanWorksheetForMutualFund(ISheet worksheet)
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
                var exactMatch = FindExactMatch(cellValue);
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

    private string FindExactMatch(string cellText)
    {
        if (string.IsNullOrEmpty(cellText)) return string.Empty;

        var normalizedCellText = cellText.ToLowerInvariant().Trim();

        foreach (var pattern in _mutualFundPatterns)
        {
            var normalizedPattern = pattern.ToLowerInvariant().Trim();
            
            // Check for exact match
            if (normalizedCellText == normalizedPattern)
            {
                return pattern;
            }
            
            // Check if cell text contains the exact pattern
            if (normalizedCellText.Contains(normalizedPattern))
            {
                return pattern;
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
