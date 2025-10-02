using MutualFundScanner.Application.Interfaces;
using MutualFundScanner.Domain.Entities;
using MutualFundScanner.Domain.Enums;
using MutualFundScanner.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using System.Text;
using NPOI.OpenXmlFormats.Dml;

namespace MutualFundScanner.Application.Services;

public class FileProcessingService : IFileProcessingService
{
    private readonly MutualFundContext _context;
    private readonly ILogger<FileProcessingService> _logger;
    private readonly IMutualFundService _mutualFundService;

    public FileProcessingService(
        MutualFundContext context,
        ILogger<FileProcessingService> logger,
        IMutualFundService mutualFundService)
    {
        _context = context;
        _logger = logger;
        _mutualFundService = mutualFundService;
    }

    public async Task<FileProcessingResult> ProcessExcelFileAsync(IFormFile file, int mutualFundId, DateTime? reportDate = null)
    {
        var result = new FileProcessingResult();

        try
        {
            // Validate file
            if (!await ValidateFileAsync(file))
            {
                result.Success = false;
                result.Message = "File validation failed";
                return result;
            }

            // Get mutual fund
            var fund = await _mutualFundService.GetByIdAsync(mutualFundId);
            if (fund == null)
            {
                result.Success = false;
                result.Message = $"Mutual fund with ID {mutualFundId} not found";
                return result;
            }

            // Calculate file hash
            var fileHash = await CalculateFileHashAsync(file);

            // Check for duplicates and get existing portfolio file if it exists
            var existingPortfolioFile = await GetExistingPortfolioFileAsync(fileHash);
            PortfolioFile portfolioFile;

            if (existingPortfolioFile != null)
            {
                // File exists, use existing portfolio file and reprocess content
                portfolioFile = existingPortfolioFile;
                _logger.LogInformation("File already exists with hash {FileHash}. Reprocessing content for file {FileName}", 
                    fileHash, file.FileName);
                
                // Update status to processing
                portfolioFile.Status = ProcessingStatus.Processing;
                portfolioFile.ProcessedDate = DateTime.UtcNow;
                
                // Clear existing holdings for reprocessing
                var existingHoldings = _context.PortfolioHoldings
                    .Where(h => h.PortfolioFileId == portfolioFile.Id);
                _context.PortfolioHoldings.RemoveRange(existingHoldings);
                
                await _context.SaveChangesAsync();
            }
            else
            {
                // Create new portfolio file record
                portfolioFile = new PortfolioFile
                {
                    FileName = file.FileName,
                    FilePath = $"uploads/{file.FileName}",
                    FileHash = fileHash,
                    MutualFundId = fund.Id,
                    ProcessedDate = DateTime.UtcNow,
                    Status = ProcessingStatus.Processing,
                    ReportDate = reportDate ?? ExtractReportDateFromFileName(file.FileName)
                };

                _context.PortfolioFiles.Add(portfolioFile);
                await _context.SaveChangesAsync();
            }

            // Process Excel file
            var holdings = await ProcessExcelContent(file, portfolioFile.Id);
            
            portfolioFile.Status = ProcessingStatus.Completed;
            result.Holdings = holdings;
            result.ProcessedRecords = holdings.Count;
            result.Success = true;
            result.Message = $"Successfully processed {holdings.Count} holdings";
            result.PortfolioFile = portfolioFile;

            await _context.SaveChangesAsync();

            // Create portfolio snapshot and track holding changes
            await CreatePortfolioSnapshotAndTrackChangesAsync(portfolioFile, holdings);

            _logger.LogInformation("Successfully processed file {FileName} with {Count} holdings", 
                file.FileName, holdings.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing file {FileName}", file.FileName);
            result.Success = false;
            result.Message = $"Processing failed: {ex.Message}";
            result.Errors.Add(ex.Message);
        }

        return result;
    }

    public async Task<FileProcessingResult> ProcessExcelFileAsync(IFormFile file, int mutualFundId, DateTime? reportDate, int sheetIndex)
    {
        var result = new FileProcessingResult();

        try
        {
            // Validate file
            if (!await ValidateFileAsync(file))
            {
                result.Success = false;
                result.Message = "File validation failed";
                return result;
            }

            // Get mutual fund
            var fund = await _mutualFundService.GetByIdAsync(mutualFundId);
            if (fund == null)
            {
                result.Success = false;
                result.Message = $"Mutual fund with ID {mutualFundId} not found";
                return result;
            }

            // Calculate file hash (for the entire file, not just the sheet)
            var fileHash = await CalculateFileHashAsync(file);
            
            // Create a unique hash for this specific sheet by combining file hash and sheet index
            var sheetSpecificHash = $"{fileHash}_Sheet{sheetIndex}";

            // Check for duplicates using sheet-specific hash
            var existingPortfolioFile = await _context.PortfolioFiles
                .FirstOrDefaultAsync(f => f.FileHash == sheetSpecificHash && f.MutualFundId == mutualFundId);
            
            PortfolioFile portfolioFile;

            if (existingPortfolioFile != null)
            {
                // Sheet already processed, use existing portfolio file and reprocess content
                portfolioFile = existingPortfolioFile;
                _logger.LogInformation("Sheet {SheetIndex} from file {FileName} already exists with hash {FileHash}. Reprocessing content", 
                    sheetIndex, file.FileName, sheetSpecificHash);
                
                // Update status to processing
                portfolioFile.Status = ProcessingStatus.Processing;
                portfolioFile.ProcessedDate = DateTime.UtcNow;
                
                // Clear existing holdings for reprocessing
                var existingHoldings = _context.PortfolioHoldings
                    .Where(h => h.PortfolioFileId == portfolioFile.Id);
                _context.PortfolioHoldings.RemoveRange(existingHoldings);
                
                await _context.SaveChangesAsync();
            }
            else
            {
                // Create new portfolio file record for this sheet
                portfolioFile = new PortfolioFile
                {
                    FileName = $"{file.FileName} - Sheet {sheetIndex + 1}",
                    FilePath = $"uploads/{file.FileName}",
                    FileHash = sheetSpecificHash,
                    MutualFundId = fund.Id,
                    ProcessedDate = DateTime.UtcNow,
                    Status = ProcessingStatus.Processing,
                    ReportDate = reportDate ?? ExtractReportDateFromFileName(file.FileName)
                };

                _context.PortfolioFiles.Add(portfolioFile);
                await _context.SaveChangesAsync();
            }

            // Process specific sheet from Excel file
            var holdings = await ProcessExcelContentFromSheet(file, portfolioFile.Id, sheetIndex);
            
            portfolioFile.Status = ProcessingStatus.Completed;
            result.Holdings = holdings;
            result.ProcessedRecords = holdings.Count;
            result.Success = true;
            result.Message = $"Successfully processed {holdings.Count} holdings from sheet {sheetIndex + 1}";
            result.PortfolioFile = portfolioFile;

            await _context.SaveChangesAsync();

            // Create portfolio snapshot and track holding changes
            await CreatePortfolioSnapshotAndTrackChangesAsync(portfolioFile, holdings);

            _logger.LogInformation("Successfully processed sheet {SheetIndex} from file {FileName} with {Count} holdings", 
                sheetIndex + 1, file.FileName, holdings.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing sheet {SheetIndex} from file {FileName}", sheetIndex + 1, file.FileName);
            result.Success = false;
            result.Message = $"Processing failed: {ex.Message}";
            result.Errors.Add(ex.Message);
        }

        return result;
    }

    public async Task<bool> ValidateFileAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return false;

        if (!IsExcelFile(file.FileName))
            return false;

        // Additional validation can be added here
        return await Task.FromResult(true);
    }

    public async Task<string> CalculateFileHashAsync(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        using var sha256 = SHA256.Create();
        var hashBytes = await Task.Run(() => sha256.ComputeHash(stream));
        return Convert.ToBase64String(hashBytes);
    }

    public bool IsExcelFile(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension == ".xlsx" || extension == ".xls";
    }

    public async Task<bool> FileExistsAsync(string fileHash)
    {
        return await Task.FromResult(_context.PortfolioFiles.Any(f => f.FileHash == fileHash));
    }

    public async Task<FileProcessingResult> ProcessFileAsync(PortfolioFile portfolioFile)
    {
        var result = new FileProcessingResult();

        try
        {
            // Update status to processing
            portfolioFile.Status = ProcessingStatus.Processing;
            portfolioFile.ProcessedDate = DateTime.UtcNow;
            
            // Clear existing holdings for reprocessing
            var existingHoldings = _context.PortfolioHoldings
                .Where(h => h.PortfolioFileId == portfolioFile.Id);
            _context.PortfolioHoldings.RemoveRange(existingHoldings);
            
            await _context.SaveChangesAsync();

            // Process file from disk (assuming file path is stored)
            if (!File.Exists(portfolioFile.FilePath))
            {
                result.Success = false;
                result.Message = $"File not found at path: {portfolioFile.FilePath}";
                portfolioFile.Status = ProcessingStatus.Failed;
                portfolioFile.ErrorMessage = result.Message;
                await _context.SaveChangesAsync();
                return result;
            }

            // Process the file content from disk
            var holdings = await ProcessFileContentFromDisk(portfolioFile.FilePath, portfolioFile.Id);
            
            portfolioFile.Status = ProcessingStatus.Completed;
            portfolioFile.RecordCount = holdings.Count();
            result.Holdings = holdings;
            result.ProcessedRecords = holdings.Count();
            result.Success = true;
            result.Message = $"Successfully processed {holdings.Count()} holdings";
            result.PortfolioFile = portfolioFile;

            await _context.SaveChangesAsync();

            // Create portfolio snapshot and track holding changes
            await CreatePortfolioSnapshotAndTrackChangesAsync(portfolioFile, holdings);

            _logger.LogInformation("Successfully processed file {FileName} with {Count} holdings", 
                portfolioFile.FileName, holdings.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing file {FileName}", portfolioFile.FileName);
            portfolioFile.Status = ProcessingStatus.Failed;
            portfolioFile.ErrorMessage = ex.Message;
            await _context.SaveChangesAsync();
            
            result.Success = false;
            result.Message = $"Processing failed: {ex.Message}";
            result.Errors.Add(ex.Message);
        }

        return result;
    }

    public async Task<PortfolioFile?> GetExistingPortfolioFileAsync(string fileHash)
    {
        return await Task.FromResult(_context.PortfolioFiles
            .Include(f => f.MutualFund)
            .Include(f => f.Holdings)
            .FirstOrDefault(f => f.FileHash == fileHash));
    }

    private async Task<List<PortfolioHolding>> ProcessFileContentFromDisk(string filePath, int portfolioFileId)
    {
        var holdings = new List<PortfolioHolding>();

        // Read file into memory stream to avoid file locking issues
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        
        // Determine file type and create appropriate workbook using WorkbookFactory for better detection
        IWorkbook workbook;
        try
        {
            // Use WorkbookFactory.Create which automatically detects the format
            workbook = WorkbookFactory.Create(memoryStream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create workbook from file {FilePath}. Trying fallback method.", filePath);
            
            // Fallback to manual detection
            memoryStream.Position = 0;
            if (filePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                workbook = new XSSFWorkbook(memoryStream);
            }
            else if (filePath.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
            {
                workbook = new HSSFWorkbook(memoryStream);
            }
            else
            {
                _logger.LogError("Unsupported file format: {FilePath}", filePath);
                return holdings;
            }
        }

        using (workbook)
        {
            var worksheet = workbook.GetSheetAt(0);
            if (worksheet == null)
            {
                _logger.LogWarning("No worksheet found in file");
                return holdings;
            }

            var rowCount = worksheet.LastRowNum + 1;
            var colCount = GetColumnCount(worksheet);

            if (rowCount == 0 || colCount == 0)
            {
                _logger.LogWarning("Empty worksheet detected");
                return holdings;
            }

            // Detect column mappings using intelligent extraction
            var columnMapping = await DetectColumnMappings(worksheet, rowCount, colCount);
            
            if (columnMapping.CompanyNameColumn == -1)
            {
                _logger.LogWarning("Could not detect company name column");
                return holdings;
            }

            // Start processing from the row where first company was detected
            for (int row = columnMapping.DataStartRow; row < rowCount; row++)
            {
                try
                {
                    var dataRow = worksheet.GetRow(row);
                    if (dataRow == null) continue;

                    var companyName = GetCellValue(dataRow, columnMapping.CompanyNameColumn)?.Trim();
                    if (string.IsNullOrEmpty(companyName) || !IsCompanyName(companyName))
                        continue;

                    var isin = ExtractISINFromRow(worksheet, row, colCount);
                    var quantity = ExtractQuantityFromRow(worksheet, row, columnMapping);
                    var weight = ExtractWeightFromRow(worksheet, row, columnMapping);

                    // Only create portfolio holding if all key validations pass
                    if (string.IsNullOrEmpty(isin) || !IsValidISIN(isin))
                        continue;
                    
                    if (weight <= 0m || weight > 100m)
                        continue;

                    var industry = ExtractSectorFromRow(worksheet, row, columnMapping);
                    
                    // Skip holdings where industry contains credit ratings
                    if (IsCreditRating(industry))
                        continue;
                        
                    // Skip securities with parentheses in their names
                    if (ContainsParentheses(companyName))
                    {
                        _logger.LogDebug("Skipping security with parentheses: {Company}", companyName);
                        continue;
                    }

                    var holding = new PortfolioHolding
                    {
                        PortfolioFileId = portfolioFileId,
                        SecurityName = companyName,
                        ISIN = isin,
                        Ticker = ExtractTickerFromCompanyName(companyName),
                        Quantity = quantity,
                        MarketValue = 0, // Can be calculated from quantity * price if needed
                        Weight = weight,
                        Sector = string.Empty,
                        Industry = industry, // Can be enhanced later
                        AsOfDate = DateTime.UtcNow.Date
                    };

                    holdings.Add(holding);
                    _logger.LogDebug("Processed holding: {Company} - ISIN: {ISIN}, Weight: {Weight}%", 
                        companyName, isin, weight);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Error processing row {Row}: {Error}", row, ex.Message);
                }
            }
        }

        if (holdings.Any())
        {
            _context.PortfolioHoldings.AddRange(holdings);
            _logger.LogInformation("Successfully extracted {Count} holdings using intelligent detection", holdings.Count);
        }

        return holdings;
    }

    private async Task<List<PortfolioHolding>> ProcessExcelContent(IFormFile file, int portfolioFileId)
    {
        var holdings = new List<PortfolioHolding>();

        // Create a memory stream to avoid stream disposal issues
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        
        // Determine file type and create appropriate workbook using WorkbookFactory for better detection
        IWorkbook workbook;
        try
        {
            // Use WorkbookFactory.Create which automatically detects the format
            workbook = WorkbookFactory.Create(memoryStream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create workbook from file {FileName}. Trying fallback method.", file.FileName);
            
            // Fallback to manual detection
            memoryStream.Position = 0;
            if (file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                workbook = new XSSFWorkbook(memoryStream);
            }
            else if (file.FileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
            {
                workbook = new HSSFWorkbook(memoryStream);
            }
            else
            {
                _logger.LogError("Unsupported file format: {FileName}", file.FileName);
                return holdings;
            }
        }

        using (workbook)
        {
            var worksheet = workbook.GetSheetAt(0);
            if (worksheet == null)
            {
                _logger.LogWarning("No worksheet found in file");
                return holdings;
            }

            var rowCount = worksheet.LastRowNum + 1;
            var colCount = GetColumnCount(worksheet);

            if (rowCount == 0 || colCount == 0)
            {
                _logger.LogWarning("Empty worksheet detected");
                return holdings;
            }

            // Detect column mappings using intelligent extraction
            var columnMapping = await DetectColumnMappings(worksheet, rowCount, colCount);
            
            if (columnMapping.CompanyNameColumn == -1)
            {
                _logger.LogWarning("Could not detect company name column");
                return holdings;
            }

            // Start processing from the row where first company was detected
            for (int row = columnMapping.DataStartRow; row < rowCount; row++)
            {
                try
                {
                    var dataRow = worksheet.GetRow(row);
                    if (dataRow == null) continue;

                    var companyName = GetCellValue(dataRow, columnMapping.CompanyNameColumn)?.Trim();
                    if (string.IsNullOrEmpty(companyName) || !IsCompanyName(companyName))
                        continue;

                    var isin = ExtractISINFromRow(worksheet, row, colCount);
                    var quantity = ExtractQuantityFromRow(worksheet, row, columnMapping);
                    var weight = ExtractWeightFromRow(worksheet, row, columnMapping);

                    // Only create portfolio holding if all key validations pass
                    if (string.IsNullOrEmpty(isin) || !IsValidISIN(isin))
                        continue;
                    
                    if (weight <= 0m || weight > 100m)
                        continue;

                    var sector = ExtractSectorFromRow(worksheet, row, columnMapping);
                    
                    // Skip holdings where sector contains credit ratings
                    if (IsCreditRating(sector))
                        continue;
                        
                    // Skip securities with parentheses in their names
                    if (ContainsParentheses(companyName))
                    {
                        _logger.LogDebug("Skipping security with parentheses: {Company}", companyName);
                        continue;
                    }

                    var holding = new PortfolioHolding
                    {
                        PortfolioFileId = portfolioFileId,
                        SecurityName = companyName,
                        ISIN = isin,
                        Ticker = ExtractTickerFromCompanyName(companyName),
                        Quantity = quantity,
                        MarketValue = 0, // Can be calculated from quantity * price if needed
                        Weight = weight,
                        Sector = sector,
                        Industry = string.Empty, // Can be enhanced later
                        AsOfDate = DateTime.UtcNow.Date
                    };

                    holdings.Add(holding);
                    _logger.LogDebug("Processed holding: {Company} - ISIN: {ISIN}, Weight: {Weight}%", 
                        companyName, isin, weight);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Error processing row {Row}: {Error}", row, ex.Message);
                }
            }
        }

        if (holdings.Any())
        {
            _context.PortfolioHoldings.AddRange(holdings);
            _logger.LogInformation("Successfully extracted {Count} holdings using intelligent detection", holdings.Count);
        }

        return holdings;
    }

    private async Task<List<PortfolioHolding>> ProcessExcelContentFromSheet(IFormFile file, int portfolioFileId, int sheetIndex)
    {
        var holdings = new List<PortfolioHolding>();

        // Create a memory stream to avoid stream disposal issues
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        
        // Determine file type and create appropriate workbook using WorkbookFactory for better detection
        IWorkbook workbook;
        try
        {
            // Use WorkbookFactory.Create which automatically detects the format
            workbook = WorkbookFactory.Create(memoryStream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create workbook from file {FileName}. Trying fallback method.", file.FileName);
            
            // Fallback to manual detection
            memoryStream.Position = 0;
            if (file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                workbook = new XSSFWorkbook(memoryStream);
            }
            else if (file.FileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
            {
                workbook = new HSSFWorkbook(memoryStream);
            }
            else
            {
                _logger.LogError("Unsupported file format: {FileName}", file.FileName);
                return holdings;
            }
        }

        using (workbook)
        {
            // Get the specific sheet by index
            if (sheetIndex >= workbook.NumberOfSheets)
            {
                _logger.LogWarning("Sheet index {SheetIndex} is out of range. File has {SheetCount} sheets", 
                    sheetIndex, workbook.NumberOfSheets);
                return holdings;
            }

            var worksheet = workbook.GetSheetAt(sheetIndex);
            if (worksheet == null)
            {
                _logger.LogWarning("No worksheet found at index {SheetIndex}", sheetIndex);
                return holdings;
            }

            var rowCount = worksheet.LastRowNum + 1;
            var colCount = GetColumnCount(worksheet);

            if (rowCount == 0 || colCount == 0)
            {
                _logger.LogWarning("Empty worksheet detected at sheet index {SheetIndex}", sheetIndex);
                return holdings;
            }

            // Detect column mappings using intelligent extraction
            var columnMapping = await DetectColumnMappings(worksheet, rowCount, colCount);
            
            if (columnMapping.CompanyNameColumn == -1)
            {
                _logger.LogWarning("Could not detect company name column in sheet {SheetIndex}", sheetIndex);
                return holdings;
            }

            // Start processing from the row where first company was detected
            for (int row = columnMapping.DataStartRow; row < rowCount; row++)
            {
                try
                {
                    var dataRow = worksheet.GetRow(row);
                    if (dataRow == null) continue;

                    var companyName = GetCellValue(dataRow, columnMapping.CompanyNameColumn)?.Trim();
                    if (string.IsNullOrEmpty(companyName) || !IsCompanyName(companyName))
                        continue;

                    var isin = ExtractISINFromRow(worksheet, row, colCount);
                    var quantity = ExtractQuantityFromRow(worksheet, row, columnMapping);
                    var weight = ExtractWeightFromRow(worksheet, row, columnMapping);

                    // Only create portfolio holding if all key validations pass
                    if (string.IsNullOrEmpty(isin) || !IsValidISIN(isin))
                        continue;
                    
                    if (weight <= 0m || weight > 100m)
                        continue;

                    var sector = ExtractSectorFromRow(worksheet, row, columnMapping);
                    
                    // Skip holdings where sector contains credit ratings
                    if (IsCreditRating(sector))
                        continue;
                        
                    // Skip securities with parentheses in their names
                    if (ContainsParentheses(companyName))
                    {
                        _logger.LogDebug("Skipping security with parentheses: {Company}", companyName);
                        continue;
                    }
                        
                    var holding = new PortfolioHolding
                    {
                        PortfolioFileId = portfolioFileId,
                        SecurityName = companyName,
                        ISIN = isin,
                        Ticker = ExtractTickerFromCompanyName(companyName),
                        Quantity = quantity,
                        MarketValue = 0, // Can be calculated from quantity * price if needed
                        Weight = weight,
                        Sector = sector,
                        Industry = string.Empty, // Can be enhanced later
                        AsOfDate = DateTime.UtcNow.Date
                    };

                    holdings.Add(holding);
                    _logger.LogDebug("Processed holding from sheet {SheetIndex}: {Company} - ISIN: {ISIN}, Weight: {Weight}%", 
                        sheetIndex, companyName, isin, weight);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Error processing row {Row} in sheet {SheetIndex}: {Error}", row, sheetIndex, ex.Message);
                }
            }
        }

        if (holdings.Any())
        {
            _context.PortfolioHoldings.AddRange(holdings);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {

                throw;
            }
            
            _logger.LogInformation("Successfully extracted {Count} holdings from sheet {SheetIndex} using intelligent detection", 
                holdings.Count, sheetIndex);
        }

        return holdings;
    }

    private async Task<ColumnMapping> DetectColumnMappings(ISheet worksheet, int rowCount, int colCount)
    {
        var mapping = new ColumnMapping();
        
        // Scan through rows to detect company name column
        for (int row = 0; row < Math.Min(rowCount, 20); row++) // Check first 20 rows
        {
            var dataRow = worksheet.GetRow(row);
            if (dataRow == null) continue;

            for (int col = 0; col < colCount; col++)
            {
                var cellValue = GetCellValue(dataRow, col)?.Trim();
                if (string.IsNullOrEmpty(cellValue)) continue;

                if (IsCompanyName(cellValue))
                {
                    // Verify this column has company names by checking next few cells
                    if (await VerifyCompanyNameColumn(worksheet, row, col, rowCount))
                    {
                        mapping.CompanyNameColumn = col;
                        mapping.DataStartRow = row;
                        break;
                    }
                }
            }
            
            if (mapping.CompanyNameColumn != -1) break;
        }

        // Detect numeric columns for quantity and weight, and text-based columns for sector
        if (mapping.CompanyNameColumn != -1)
        {
            DetectNumericColumns(worksheet, mapping, rowCount, colCount);
            DetectTextBasedColumns(worksheet, mapping, rowCount, colCount);
        }

        return mapping;
    }

    private Task<bool> VerifyCompanyNameColumn(ISheet worksheet, int startRow, int col, int rowCount)
    {
        int companyNameCount = 0;
        int checkRows = Math.Min(5, rowCount - startRow + 1); // Check next 5 rows or remaining rows

        for (int row = startRow; row < startRow + checkRows; row++)
        {
            var dataRow = worksheet.GetRow(row);
            if (dataRow == null) continue;

            var cellValue = GetCellValue(dataRow, col)?.Trim();
            if (!string.IsNullOrEmpty(cellValue) && IsCompanyName(cellValue))
            {
                companyNameCount++;
            }
        }

        return Task.FromResult(companyNameCount >= 2); // At least 3 company names found
    }

    private void DetectNumericColumns(ISheet worksheet, ColumnMapping mapping, int rowCount, int colCount)
    {
        bool weightColumnFound = false;
        bool quantityColumnFound = false;
        
        for (int col = 0; col < colCount; col++)
        {
            if (col == mapping.CompanyNameColumn) continue;

            // First check for header text patterns for quantity
            if (!quantityColumnFound && IsQuantityColumn(worksheet, col, mapping.DataStartRow))
            {
                mapping.QuantityColumn = col;
                quantityColumnFound = true;
                continue;
            }

            var columnStats = AnalyzeNumericColumn(worksheet, mapping.DataStartRow, col, rowCount);
            
            // Weight column: decimal values typically < 100 (percentages)
            if (columnStats.HasDecimals && columnStats.MaxValue < 100 && columnStats.MinValue >= 0)
            {
                if (weightColumnFound) continue; // Only one weight column expected
                mapping.WeightColumn = col;
                weightColumnFound = true;
            }
            // Quantity column: integer values, typically larger numbers (fallback if header detection failed)
            else if (!quantityColumnFound && !columnStats.HasDecimals && columnStats.MaxValue > 100)
            {
                mapping.QuantityColumn = col;
                quantityColumnFound = true;
            }
        }
    }

    private void DetectTextBasedColumns(ISheet worksheet, ColumnMapping mapping, int rowCount, int colCount)
    {
        for (int col = 0; col < colCount; col++)
        {
            if (col == mapping.CompanyNameColumn || col == mapping.WeightColumn || col == mapping.QuantityColumn) 
                continue;

            // Check for sector/industry column based on header text
            if (IsSectorColumn(worksheet, col, mapping.DataStartRow))
            {
                mapping.SectorColumn = col;
                break; // Only one sector column expected
            }
        }
    }

    private bool IsQuantityColumn(ISheet worksheet, int col, int dataStartRow)
    {
        // Check header rows (before data starts) for quantity-related text
        for (int row = 0; row < dataStartRow; row++)
        {
            var headerRow = worksheet.GetRow(row);
            if (headerRow == null) continue;

            var cellValue = GetCellValue(headerRow, col)?.Trim();
            if (string.IsNullOrEmpty(cellValue)) continue;

            // Look for "quantity" text in header
            if (cellValue.Contains("quantity", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsSectorColumn(ISheet worksheet, int col, int dataStartRow)
    {
        // Check header rows (before data starts) for sector/industry-related text
        for (int row = 0; row < dataStartRow; row++)
        {
            var headerRow = worksheet.GetRow(row);
            if (headerRow == null) continue;

            var cellValue = GetCellValue(headerRow, col)?.Trim();
            if (string.IsNullOrEmpty(cellValue)) continue;

            // Look for "Industry", "Rating", or "Sector" text in header
            if (cellValue.Contains("industry", StringComparison.OrdinalIgnoreCase) ||
                cellValue.Contains("Industry / Rating", StringComparison.OrdinalIgnoreCase) ||
                cellValue.Contains("Industry+", StringComparison.OrdinalIgnoreCase) ||
                cellValue.Contains("sector", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    private NumericColumnStats AnalyzeNumericColumn(ISheet worksheet, int startRow, int col, int rowCount)
    {
        var stats = new NumericColumnStats();
        var values = new List<decimal>();

        for (int row = startRow; row < Math.Min(startRow + 10, rowCount); row++) // Analyze first 10 data rows
        {
            var dataRow = worksheet.GetRow(row);
            if (dataRow == null) continue;

            var cellValue = GetCellValue(dataRow, col)?.Trim();
            if (decimal.TryParse(cellValue, out var numValue))
            {
                values.Add(numValue);
                stats.HasDecimals = stats.HasDecimals || (numValue % 1 != 0);
            }
        }

        if (values.Any())
        {
            stats.MinValue = values.Min();
            stats.MaxValue = values.Max();
            stats.NumericCount = values.Count;
        }

        return stats;
    }

    private bool IsCompanyName(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;

        var companyKeywords = new[] { "Ltd", "Limited", "LLP", "Pvt", "PLC", "Inc", "Corp", "Corporation" };
        return companyKeywords.Any(keyword => text.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private string ExtractISINFromRow(ISheet worksheet, int row, int colCount)
    {
        var dataRow = worksheet.GetRow(row);
        if (dataRow == null) return string.Empty;

        for (int col = 0; col < colCount; col++)
        {
            var cellValue = GetCellValue(dataRow, col)?.Trim();
            if (IsValidISIN(cellValue))
            {
                return cellValue!;
            }
        }
        return string.Empty;
    }

    private bool IsValidISIN(string isin)
    {
        if (string.IsNullOrWhiteSpace(isin) || isin.Length != 12)
            return false;

        // Basic ISIN format check: 2 letters + 10 alphanumeric
        return char.IsLetter(isin[0]) && char.IsLetter(isin[1]) && 
               isin.Skip(2).All(c => char.IsLetterOrDigit(c));
    }

    private bool IsCreditRating(string sector)
    {
        if (string.IsNullOrWhiteSpace(sector))
            return false;

        var creditRatingPatterns = new[]
        {
            "IND A1+", "CRISIL A1+", "CARE A1+", "ICRA A1+",
            "IND A1", "CRISIL A1", "CARE A1", "ICRA A1",
            "A1+", "A1", "AA+", "AA", "AAA"
        };

        return creditRatingPatterns.Any(pattern => 
            sector.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    private decimal ExtractQuantityFromRow(ISheet worksheet, int row, ColumnMapping mapping)
    {
        if (mapping.QuantityColumn == -1) return 0;
        
        var dataRow = worksheet.GetRow(row);
        if (dataRow == null) return 0;

        var cell = dataRow.GetCell(mapping.QuantityColumn);
        if (cell == null) return 0;

        // Handle numeric cells directly to avoid string conversion issues
        if (cell.CellType == CellType.Numeric)
        {
            // Convert to int first since quantity should be whole numbers, then cast to decimal for entity compatibility
            return (int)cell.NumericCellValue;
        }
        
        // Handle other cell types using the existing GetCellValue method
        var cellValue = GetCellValue(dataRow, mapping.QuantityColumn)?.Trim();
        if (int.TryParse(cellValue, out var intQuantity))
        {
            return intQuantity;
        }
        return decimal.TryParse(cellValue, out var quantity) ? quantity : 0;
    }

    private decimal ExtractWeightFromRow(ISheet worksheet, int row, ColumnMapping mapping)
    {
        if (mapping.WeightColumn == -1) return 0;
        
        var dataRow = worksheet.GetRow(row);
        if (dataRow == null) return 0;

        var cell = dataRow.GetCell(mapping.WeightColumn);
        if (cell == null) return 0;

        // Handle numeric cells directly to avoid string conversion issues
        if (cell.CellType == CellType.Numeric)
        {
            return Math.Round(((decimal)cell.NumericCellValue) * 100, 2);
        }
        
        // Handle other cell types using the existing GetCellValue method
        var cellValue = GetCellValue(dataRow, mapping.WeightColumn)?.Trim();
        return decimal.TryParse(cellValue, out var weight) ? weight : 0;
    }

    private string ExtractSectorFromRow(ISheet worksheet, int row, ColumnMapping mapping)
    {
        if (mapping.SectorColumn == -1) return string.Empty;
        
        var dataRow = worksheet.GetRow(row);
        if (dataRow == null) return string.Empty;

        var cellValue = GetCellValue(dataRow, mapping.SectorColumn)?.Trim();
        return string.IsNullOrEmpty(cellValue) ? string.Empty : cellValue;
    }

    private string ExtractTickerFromCompanyName(string companyName)
    {
        // Simple ticker extraction - take first word before "Ltd", "Limited", etc.
        var parts = companyName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[0].ToUpperInvariant() : string.Empty;
    }

    private DateTime ExtractReportDateFromFileName(string fileName)
    {
        try
        {
            _logger.LogInformation("Extracting report date from filename: {FileName}", fileName);
            
            // Common date patterns in mutual fund portfolio files
            var datePatterns = new[]
            {
                // Pattern: FundName_31-Mar-2024.xlsx or FundName_31_03_2024.xlsx
                @"(\d{1,2})[-_](Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)[-_](\d{4})",
                @"(\d{1,2})[-_](\d{1,2})[-_](\d{4})",
                
                // Pattern: FundName_20240331.xlsx or FundName_2024-03-31.xlsx
                @"(\d{4})[-_](\d{1,2})[-_](\d{1,2})",
                @"(\d{4})(\d{2})(\d{2})",
                
                // Pattern: 31Mar2024, 31-03-2024, 31/03/2024
                @"(\d{1,2})[-/](Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)[-/](\d{4})",
                @"(\d{1,2})[-/](\d{1,2})[-/](\d{4})",
                
                // Pattern: Mar-31-2024, 03-31-2024
                @"(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)[-_](\d{1,2})[-_](\d{4})",
                @"(\d{1,2})[-_](\d{1,2})[-_](\d{4})"
            };

            foreach (var pattern in datePatterns)
            {
                var match = System.Text.RegularExpressions.Regex.Match(fileName, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    try
                    {
                        DateTime extractedDate;
                        
                        if (match.Groups.Count == 4)
                        {
                            // Pattern with 3 groups (day, month, year or month, day, year)
                            var group1 = match.Groups[1].Value;
                            var group2 = match.Groups[2].Value;
                            var group3 = match.Groups[3].Value;
                            
                            // Check if group2 is a month name (Jan, Feb, etc.)
                            if (new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" }
                                .Contains(group2, StringComparer.OrdinalIgnoreCase))
                            {
                                // Format: DD-MMM-YYYY
                                extractedDate = DateTime.ParseExact($"{group1}-{group2}-{group3}", "d-MMM-yyyy", null);
                            }
                            else if (int.TryParse(group1, out int year) && year > 1000)
                            {
                                // Format: YYYY-MM-DD
                                extractedDate = DateTime.ParseExact($"{group1}-{group2}-{group3}", "yyyy-MM-dd", null);
                            }
                            else
                            {
                                // Format: DD-MM-YYYY
                                extractedDate = DateTime.ParseExact($"{group1}-{group2}-{group3}", "dd-MM-yyyy", null);
                            }
                        }
                        else if (match.Groups.Count == 3)
                        {
                            // Pattern with 2 groups (like YYYYMMDD)
                            var year = match.Groups[1].Value;
                            var month = match.Groups[2].Value;
                            var day = match.Groups[2].Value.Length > 2 ? match.Groups[2].Value.Substring(0, 2) : "01";
                            
                            if (year.Length == 4 && month.Length == 2 && day.Length == 2)
                            {
                                extractedDate = DateTime.ParseExact($"{year}-{month}-{day}", "yyyy-MM-dd", null);
                            }
                            else
                            {
                                continue; // Try next pattern
                            }
                        }
                        else
                        {
                            continue; // Try next pattern
                        }

                        _logger.LogInformation("Successfully extracted date {Date} from filename {FileName}", extractedDate.ToString("yyyy-MM-dd"), fileName);
                        return extractedDate.Date;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Failed to parse date from pattern {Pattern} in filename {FileName}: {Error}", pattern, fileName, ex.Message);
                        continue; // Try next pattern
                    }
                }
            }

            // If no date pattern found, try to get date from file creation/modification time
            _logger.LogWarning("No date pattern found in filename {FileName}. Using current date.", fileName);
            return DateTime.UtcNow.Date;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting date from filename {FileName}", fileName);
            return DateTime.UtcNow.Date;
        }
    }


    private int GetColumnCount(ISheet worksheet)
    {
        int maxColumns = 0;
        
        for (int i = 0; i <= worksheet.LastRowNum; i++)
        {
            var row = worksheet.GetRow(i);
            if (row != null && row.LastCellNum > maxColumns)
            {
                maxColumns = row.LastCellNum;
            }
        }
        return maxColumns;
    }

    /// <summary>
    /// Creates a dictionary from holdings using ISIN as key, handling duplicates by taking the first occurrence
    /// </summary>
    private Dictionary<string, PortfolioHolding> CreateHoldingDictionaryByISIN(IEnumerable<PortfolioHolding> holdings)
    {
        var result = new Dictionary<string, PortfolioHolding>();
        var duplicateIsins = new List<string>();
        
        foreach (var holding in holdings)
        {
            var isin = holding.ISIN ?? string.Empty;
            
            if (string.IsNullOrEmpty(isin))
            {
                _logger.LogWarning("Found holding with empty ISIN for security {SecurityName}", holding.SecurityName ?? "Unknown");
                continue;
            }
            
            if (!result.ContainsKey(isin))
            {
                result[isin] = holding;
            }
            else
            {
                if (!duplicateIsins.Contains(isin))
                {
                    duplicateIsins.Add(isin);
                    _logger.LogWarning("Duplicate ISIN {ISIN} found. Using first occurrence for security {SecurityName}, skipping {SecurityName2}", 
                        isin, result[isin].SecurityName ?? "Unknown", holding.SecurityName ?? "Unknown");
                }
            }
        }
        
        if (duplicateIsins.Any())
        {
            _logger.LogInformation("Processed {TotalCount} holdings with {DuplicateCount} duplicate ISINs. Final dictionary contains {UniqueCount} unique holdings.", 
                holdings.Count(), duplicateIsins.Count, result.Count);
        }
        
        return result;
    }

    private string? GetCellValue(IRow row, int columnIndex)
    {
        var cell = row.GetCell(columnIndex);
        if (cell == null) return null;

        return cell.CellType switch
        {
            CellType.String => cell.StringCellValue,
            CellType.Numeric => cell.NumericCellValue.ToString(),
            CellType.Boolean => cell.BooleanCellValue.ToString(),
            CellType.Formula => GetFormulaCellValue(cell),
            _ => cell.ToString()
        };
    }

    private string GetFormulaCellValue(ICell cell)
    {
        try
        {
            return cell.CachedFormulaResultType switch
            {
                CellType.String => cell.StringCellValue,
                CellType.Numeric => cell.NumericCellValue.ToString(),
                CellType.Boolean => cell.BooleanCellValue.ToString(),
                _ => cell.ToString() ?? string.Empty
            };
        }
        catch
        {
            return cell.ToString() ?? string.Empty;
        }
    }

    private class ColumnMapping
    {
        public int CompanyNameColumn { get; set; } = -1;
        public int QuantityColumn { get; set; } = -1;
        public int WeightColumn { get; set; } = -1;
        public int SectorColumn { get; set; } = -1;
        public int DataStartRow { get; set; } = 0;
    }

    private class NumericColumnStats
    {
        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }
        public bool HasDecimals { get; set; }
        public int NumericCount { get; set; }
    }

    /// <summary>
    /// Creates a portfolio snapshot and tracks holding changes
    /// </summary>
    private async Task CreatePortfolioSnapshotAndTrackChangesAsync(PortfolioFile portfolioFile, ICollection<PortfolioHolding> holdings)
    {
        try
        {
            PortfolioSnapshot snapshot = null;
            var existingsapshot = await _context.PortfolioSnapshots.FirstOrDefaultAsync(x => x.MutualFund == portfolioFile.MutualFund && x.SnapshotDate == portfolioFile.ReportDate);
            if (existingsapshot == null)
            {
                // Create portfolio snapshot
                 snapshot = new PortfolioSnapshot
                {
                    MutualFundId = portfolioFile.MutualFundId,
                    SnapshotDate = portfolioFile.ReportDate,
                    TotalValue = holdings.Sum(h => h.MarketValue),
                    TotalHoldings = holdings.Count,
                    CreatedDate = DateTime.UtcNow
                };
                _context.PortfolioSnapshots.Add(snapshot);
                await _context.SaveChangesAsync();
            }
            else {
                // Create portfolio snapshot
                snapshot = existingsapshot;
                existingsapshot.Holdings = new List<PortfolioHolding>();
                
                await _context.SaveChangesAsync();
                existingsapshot.Holdings = holdings;
            }


            // Update PortfolioHolding entities with the PortfolioSnapshotId
            foreach (var holding in holdings)
            {
                holding.PortfolioSnapshotId = snapshot.Id;
            }
            await _context.SaveChangesAsync();

            // Track holding changes by comparing with previous snapshot
            await TrackHoldingChangesAsync(portfolioFile.MutualFundId, snapshot.SnapshotDate, holdings);

            _logger.LogInformation("Created portfolio snapshot for fund {FundId} on {Date} with {Holdings} holdings",
                portfolioFile.MutualFundId, portfolioFile.ReportDate, holdings.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating portfolio snapshot and tracking changes for file {FileName}",
                portfolioFile.FileName);
        }
    }

    /// <summary>
    /// Tracks holding changes by comparing current holdings with previous snapshot
    /// and recalculates subsequent snapshots if needed
    /// </summary>
    private async Task TrackHoldingChangesAsync(int mutualFundId, DateTime currentDate, ICollection<PortfolioHolding> currentHoldings)
    {
        try
        {
            // Get the most recent snapshot before the current date
            var previousSnapshot = await _context.PortfolioSnapshots
                .Where(s => s.MutualFundId == mutualFundId && s.SnapshotDate < currentDate)
                .OrderByDescending(s => s.SnapshotDate)
                .FirstOrDefaultAsync();

            if (previousSnapshot == null)
            {
                // No previous snapshot, all holdings are new
                foreach (var holding in currentHoldings)
                {
                    var change = new HoldingChange
                    {
                        MutualFundId = mutualFundId,
                        ISIN = holding.ISIN,
                        Ticker = holding.Ticker,
                        SecurityName = holding.SecurityName,
                        ChangeType = HoldingChangeType.Added,
                        PreviousQuantity = null,
                        NewQuantity = holding.Quantity,
                        PreviousWeight = null,
                        NewWeight = holding.Weight,
                        PreviousValue = null,
                        NewValue = holding.MarketValue,
                        FromDate = currentDate.AddDays(-30), // Default to 30 days ago
                        ToDate = currentDate,
                        CreatedDate = DateTime.UtcNow
                    };

                    _context.HoldingChanges.Add(change);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Created {Count} new holding changes for fund {FundId} (no previous snapshot)",
                    currentHoldings.Count, mutualFundId);
                return;
            }

            // Get holdings from the previous snapshot
            var previousHoldings = await _context.PortfolioHoldings
                .Where(h => h.PortfolioFile.MutualFundId == mutualFundId && 
                           h.PortfolioFile.ReportDate == previousSnapshot.SnapshotDate)
                .ToListAsync();

            // Create dictionaries with proper duplicate handling
            var fromHoldings = CreateHoldingDictionaryByISIN(previousHoldings);
            var toHoldings = CreateHoldingDictionaryByISIN(currentHoldings);

            var changes = new List<HoldingChange>();
            var processedIsins = new HashSet<string>();

            // Check for changes and new holdings using the dictionaries
            foreach (var currentHolding in currentHoldings)
            {
                var isin = currentHolding.ISIN ?? string.Empty;
                if (string.IsNullOrEmpty(isin)) continue;
                
                processedIsins.Add(isin);
                
                var previousHoldingExists = fromHoldings.TryGetValue(isin, out var previousHolding);
                
                if (!previousHoldingExists)
                {
                    // New holding added
                    changes.Add(new HoldingChange
                    {
                        MutualFundId = mutualFundId,
                        ISIN = currentHolding.ISIN,
                        Ticker = currentHolding.Ticker,
                        SecurityName = currentHolding.SecurityName,
                        ChangeType = HoldingChangeType.Added,
                        PreviousQuantity = null,
                        NewQuantity = currentHolding.Quantity,
                        PreviousWeight = null,
                        NewWeight = currentHolding.Weight,
                        PreviousValue = null,
                        NewValue = currentHolding.MarketValue,
                        FromDate = previousSnapshot.SnapshotDate,
                        ToDate = currentDate,
                        CreatedDate = DateTime.UtcNow
                    });
                }
                else
                {
                    // Check for changes
                    var quantityChanged = previousHolding!.Quantity != currentHolding.Quantity;
                    var weightChanged = previousHolding!.Weight != currentHolding.Weight;
                    var valueChanged = previousHolding!.MarketValue != currentHolding.MarketValue;

                    if (quantityChanged || weightChanged || valueChanged)
                    {
                        var changeType = HoldingChangeType.Unchanged;
                        if (quantityChanged)
                        {
                            changeType = currentHolding.Quantity > previousHolding!.Quantity 
                                ? HoldingChangeType.Increased 
                                : HoldingChangeType.Decreased;
                        }
                        else if (weightChanged)
                        {
                            changeType = currentHolding.Weight > previousHolding!.Weight 
                                ? HoldingChangeType.Increased 
                                : HoldingChangeType.Decreased;
                        }

                        changes.Add(new HoldingChange
                        {
                            MutualFundId = mutualFundId,
                            ISIN = currentHolding.ISIN,
                            Ticker = currentHolding.Ticker,
                            SecurityName = currentHolding.SecurityName,
                            ChangeType = changeType,
                            PreviousQuantity = previousHolding!.Quantity,
                            NewQuantity = currentHolding.Quantity,
                            PreviousWeight = previousHolding!.Weight,
                            NewWeight = currentHolding.Weight,
                            PreviousValue = previousHolding!.MarketValue,
                            NewValue = currentHolding.MarketValue,
                            FromDate = previousSnapshot.SnapshotDate,
                            ToDate = currentDate,
                            CreatedDate = DateTime.UtcNow
                        });
                    }
                }
            }

            // Check for removed holdings using the dictionaries
            foreach (var kvp in fromHoldings)
            {
                var isin = kvp.Key;
                var previousHolding = kvp.Value;
                
                if (!processedIsins.Contains(isin))
                {
                    changes.Add(new HoldingChange
                    {
                        MutualFundId = mutualFundId,
                        ISIN = previousHolding.ISIN,
                        Ticker = previousHolding.Ticker,
                        SecurityName = previousHolding.SecurityName,
                        ChangeType = HoldingChangeType.Removed,
                        PreviousQuantity = previousHolding.Quantity,
                        NewQuantity = null,
                        PreviousWeight = previousHolding.Weight,
                        NewWeight = null,
                        PreviousValue = previousHolding.MarketValue,
                        NewValue = null,
                        FromDate = previousSnapshot.SnapshotDate,
                        ToDate = currentDate,
                        CreatedDate = DateTime.UtcNow
                    });
                }
            }

            if (changes.Any())
            {
                _context.HoldingChanges.AddRange(changes);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Created {Count} holding changes for fund {FundId} between {FromDate} and {ToDate}",
                    changes.Count, mutualFundId, previousSnapshot.SnapshotDate, currentDate);
            }
            else
            {
                _logger.LogInformation("No holding changes detected for fund {FundId} between {FromDate} and {ToDate}",
                    mutualFundId, previousSnapshot.SnapshotDate, currentDate);
            }

            // Check if this newly uploaded data fills a gap between existing snapshots
            // If so, recalculate holding changes for all subsequent snapshots
            await RecalculateSubsequentHoldingChangesAsync(mutualFundId, currentDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking holding changes for fund {FundId} on {Date}", mutualFundId, currentDate);
        }
    }

    /// <summary>
    /// Recalculates holding changes for the immediate next snapshot after the specified date
    /// This is called when missing data is filled (e.g., uploading July data between June and August)
    /// Only processes the very next snapshot, not all subsequent ones
    /// </summary>
    private async Task RecalculateSubsequentHoldingChangesAsync(int mutualFundId, DateTime filledDate)
    {
        try
        {
            // Find only the immediate next snapshot after the filled date
            var nextSnapshot = await _context.PortfolioSnapshots
                .Where(s => s.MutualFundId == mutualFundId && s.SnapshotDate > filledDate)
                .OrderBy(s => s.SnapshotDate)
                .FirstOrDefaultAsync();

            if (nextSnapshot == null)
            {
                _logger.LogInformation("No next snapshot found for fund {FundId} after {Date}", mutualFundId, filledDate);
                return;
            }

            _logger.LogInformation("Recalculating holding changes for the next snapshot {Date} for fund {FundId} after {Date}",
                nextSnapshot.SnapshotDate, mutualFundId, filledDate);

            // Process only the immediate next snapshot
            var subsequentSnapshot = nextSnapshot;
            
            // Get the most recent snapshot before this subsequent snapshot
            var previousSnapshot = await _context.PortfolioSnapshots
                .Where(s => s.MutualFundId == mutualFundId && s.SnapshotDate < subsequentSnapshot.SnapshotDate)
                .OrderByDescending(s => s.SnapshotDate)
                .FirstOrDefaultAsync();

            if (previousSnapshot == null)
            {
                _logger.LogWarning("No previous snapshot found for subsequent snapshot {Date} for fund {FundId}",
                    subsequentSnapshot.SnapshotDate, mutualFundId);
                return;
            }

            // Get holdings for both snapshots
            var previousHoldings = await _context.PortfolioHoldings
                .Where(h => h.PortfolioFile.MutualFundId == mutualFundId && 
                           h.PortfolioFile.ReportDate == previousSnapshot.SnapshotDate)
                .ToListAsync();

            var currentHoldings = await _context.PortfolioHoldings
                .Where(h => h.PortfolioFile.MutualFundId == mutualFundId && 
                           h.PortfolioFile.ReportDate == subsequentSnapshot.SnapshotDate)
                .ToListAsync();

            // Remove existing holding changes for this snapshot
            var existingChanges = await _context.HoldingChanges
                .Where(c => c.MutualFundId == mutualFundId && c.ToDate == subsequentSnapshot.SnapshotDate)
                .ToListAsync();

            if (existingChanges.Any())
            {
                _context.HoldingChanges.RemoveRange(existingChanges);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Removed {Count} existing holding changes for fund {FundId} on {Date}",
                    existingChanges.Count, mutualFundId, subsequentSnapshot.SnapshotDate);
            }

            // Recalculate holding changes
            await RecalculateHoldingChangesForSnapshotAsync(mutualFundId, subsequentSnapshot.SnapshotDate, 
                previousHoldings, currentHoldings, previousSnapshot.SnapshotDate);

            _logger.LogInformation("Completed recalculation of holding changes for fund {FundId} after {Date}",
                mutualFundId, filledDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recalculating subsequent holding changes for fund {FundId} after {Date}",
                mutualFundId, filledDate);
        }
    }

    /// <summary>
    /// Recalculates holding changes for a specific snapshot
    /// </summary>
    private async Task RecalculateHoldingChangesForSnapshotAsync(int mutualFundId, DateTime currentDate, 
        ICollection<PortfolioHolding> previousHoldings, ICollection<PortfolioHolding> currentHoldings, DateTime previousDate)
    {
        try
        {
            // Create dictionaries with proper duplicate handling
            var fromHoldings = CreateHoldingDictionaryByISIN(previousHoldings);
            var toHoldings = CreateHoldingDictionaryByISIN(currentHoldings);

            var changes = new List<HoldingChange>();
            var processedIsins = new HashSet<string>();

            // Check for changes and new holdings using the dictionaries
            foreach (var currentHolding in currentHoldings)
            {
                var isin = currentHolding.ISIN ?? string.Empty;
                if (string.IsNullOrEmpty(isin)) continue;
                
                processedIsins.Add(isin);
                
                var previousHoldingExists = fromHoldings.TryGetValue(isin, out var previousHolding);
                
                if (!previousHoldingExists)
                {
                    // New holding added
                    changes.Add(new HoldingChange
                    {
                        MutualFundId = mutualFundId,
                        ISIN = currentHolding.ISIN,
                        Ticker = currentHolding.Ticker,
                        SecurityName = currentHolding.SecurityName,
                        ChangeType = HoldingChangeType.Added,
                        PreviousQuantity = null,
                        NewQuantity = currentHolding.Quantity,
                        PreviousWeight = null,
                        NewWeight = currentHolding.Weight,
                        PreviousValue = null,
                        NewValue = currentHolding.MarketValue,
                        FromDate = previousDate,
                        ToDate = currentDate,
                        CreatedDate = DateTime.UtcNow
                    });
                }
                else
                {
                    // Check for changes
                    var quantityChanged = previousHolding!.Quantity != currentHolding.Quantity;
                    var weightChanged = previousHolding!.Weight != currentHolding.Weight;
                    var valueChanged = previousHolding!.MarketValue != currentHolding.MarketValue;

                    if (quantityChanged || weightChanged || valueChanged)
                    {
                        var changeType = HoldingChangeType.Unchanged;
                        if (quantityChanged)
                        {
                            changeType = currentHolding.Quantity > previousHolding!.Quantity 
                                ? HoldingChangeType.Increased 
                                : HoldingChangeType.Decreased;
                        }
                        else if (weightChanged)
                        {
                            changeType = currentHolding.Weight > previousHolding!.Weight 
                                ? HoldingChangeType.Increased 
                                : HoldingChangeType.Decreased;
                        }

                        changes.Add(new HoldingChange
                        {
                            MutualFundId = mutualFundId,
                            ISIN = currentHolding.ISIN,
                            Ticker = currentHolding.Ticker,
                            SecurityName = currentHolding.SecurityName,
                            ChangeType = changeType,
                            PreviousQuantity = previousHolding!.Quantity,
                            NewQuantity = currentHolding.Quantity,
                            PreviousWeight = previousHolding!.Weight,
                            NewWeight = currentHolding.Weight,
                            PreviousValue = previousHolding!.MarketValue,
                            NewValue = currentHolding.MarketValue,
                            FromDate = previousDate,
                            ToDate = currentDate,
                            CreatedDate = DateTime.UtcNow
                        });
                    }
                }
            }

            // Check for removed holdings using the dictionaries
            foreach (var kvp in fromHoldings)
            {
                var isin = kvp.Key;
                var previousHolding = kvp.Value;
                
                if (!processedIsins.Contains(isin))
                {
                    changes.Add(new HoldingChange
                    {
                        MutualFundId = mutualFundId,
                        ISIN = previousHolding.ISIN,
                        Ticker = previousHolding.Ticker,
                        SecurityName = previousHolding.SecurityName,
                        ChangeType = HoldingChangeType.Removed,
                        PreviousQuantity = previousHolding.Quantity,
                        NewQuantity = null,
                        PreviousWeight = previousHolding.Weight,
                        NewWeight = null,
                        PreviousValue = previousHolding.MarketValue,
                        NewValue = null,
                        FromDate = previousDate,
                        ToDate = currentDate,
                        CreatedDate = DateTime.UtcNow
                    });
                }
            }

            if (changes.Any())
            {
                _context.HoldingChanges.AddRange(changes);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Recalculated {Count} holding changes for fund {FundId} between {FromDate} and {ToDate}",
                    changes.Count, mutualFundId, previousDate, currentDate);
            }
            else
            {
                _logger.LogInformation("No holding changes found during recalculation for fund {FundId} between {FromDate} and {ToDate}",
                    mutualFundId, previousDate, currentDate);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recalculating holding changes for fund {FundId} on {Date}", mutualFundId, currentDate);
        }
    }

    /// <summary>
    /// Checks if a security or company name contains parentheses and should be ignored
    /// </summary>
    /// <param name="securityName">The security or company name to check</param>
    /// <returns>True if the name contains parentheses and should be ignored, false otherwise</returns>
    private bool ContainsParentheses(string securityName)
    {
        if (string.IsNullOrEmpty(securityName))
            return false;
            
        return securityName.Contains('(') || securityName.Contains(')');
    }
}
