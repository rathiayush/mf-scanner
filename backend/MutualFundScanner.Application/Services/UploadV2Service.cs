using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MutualFundScanner.Application.DTOs;
using MutualFundScanner.Application.Helpers;
using MutualFundScanner.Application.Interfaces;
using MutualFundScanner.Domain.Entities;
using MutualFundScanner.Domain.Enums;
using MutualFundScanner.Infrastructure.Data;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;

namespace MutualFundScanner.Application.Services;

public class UploadV2Service : IUploadV2Service
{
    private readonly MutualFundContext _context;
    private readonly IFileProcessingService _fileProcessingService;
    private readonly MutualFundDetectionService _detectionService;
    private readonly ILogger<UploadV2Service> _logger;
    private readonly IFileUploadService _fileUploadService;


    public UploadV2Service(
        MutualFundContext context,
        IFileProcessingService fileProcessingService,
        MutualFundDetectionService detectionService,
        IFileUploadService fileUploadService,
        ILogger<UploadV2Service> logger)
    {
        _context = context;
        _fileProcessingService = fileProcessingService;
        _detectionService = detectionService;
        _fileUploadService = fileUploadService;
        _logger = logger;
    }


    public async Task<UploadV2ResponseDto> ProcessUploadV2Async(UploadV2RequestDto request)
    {
        var response = new UploadV2ResponseDto();
        var detectionResults = new List<MutualFundDetectionResult>();

        try
        {
            _logger.LogInformation("Starting V2 upload process for {FileCount} files on date {ReportDate}", 
                request.Files.Count, request.ReportDate);

            // Use report date directly (already a DateTime type)
            DateTime? reportDate = request.ReportDate;

            // Step 1: Validate date consistency for all files
            var hasValidationErrors = false;
            foreach (var file in request.Files)
            {
                var dateValidationResult = DateValidationHelper.ValidateDateConsistency(file.FileName, reportDate);
                if (!dateValidationResult.IsValid)
                {
                    response.FileErrors.Add(new FileError
                    {
                        FileName = file.FileName,
                        ErrorMessage = dateValidationResult.ErrorMessage,
                        ErrorType = "Validation"
                    });
                    hasValidationErrors = true;
                    _logger.LogWarning("Date validation failed for file {FileName}: {Error}", file.FileName, dateValidationResult.ErrorMessage);
                }
            }

            // If all files failed validation, return error
            if (hasValidationErrors && response.FileErrors.Count == request.Files.Count)
            {
                response.Success = false;
                response.Message = "All files failed date validation";
                return response;
            }

            // Step 2: Detect mutual funds from each file (handling multi-sheet files)
            var filesToProcess = request.Files.Where(f => !response.FileErrors.Any(e => e.FileName == f.FileName)).ToList();
            
            foreach (var file in filesToProcess)
            {
                try
                {
                    if (_fileProcessingService.IsExcelFile(file.FileName))
                    {
                        var sheetCount = GetSheetCount(file);
                        _logger.LogInformation("File {FileName} has {SheetCount} sheets", file.FileName, sheetCount);

                        if (sheetCount > 1)
                        {
                            // Multi-sheet file - detect mutual fund from each sheet
                            await DetectMutualFundsFromMultiSheetFile(file, sheetCount, detectionResults);
                        }
                        else
                        {
                            // Single sheet file - use existing logic
                            var detectionResult = await _detectionService.DetectMutualFundFromFile(file);
                            detectionResults.Add(detectionResult);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("File {FileName} is not an Excel file, skipping", file.FileName);
                        response.FileErrors.Add(new FileError
                        {
                            FileName = file.FileName,
                            ErrorMessage = "File is not an Excel file",
                            ErrorType = "Validation"
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error detecting mutual fund from file {FileName}", file.FileName);
                    response.FileErrors.Add(new FileError
                    {
                        FileName = file.FileName,
                        ErrorMessage = $"Detection failed: {ex.Message}",
                        ErrorType = "Detection"
                    });
                }
            }

            // Step 2: Process files/sheets with detected/created mutual funds
            var processedFileIds = new List<int>();
            var createdFundsCount = 0;
            var processedSheetsCount = 0;

            // Group detections by original file to handle multi-sheet processing
            var fileDetectionMap = new Dictionary<string, List<MutualFundDetectionResult>>();
            
            foreach (var detection in detectionResults)
            {
                var originalFileName = ExtractOriginalFileName(detection.FileName);
                if (!fileDetectionMap.ContainsKey(originalFileName))
                {
                    fileDetectionMap[originalFileName] = new List<MutualFundDetectionResult>();
                }
                fileDetectionMap[originalFileName].Add(detection);
            }

            foreach (var file in filesToProcess)
            {
                if (fileDetectionMap.TryGetValue(file.FileName, out var detections))
                {
                    foreach (var detection in detections)
                    {
                        try
                        {
                            if (detection.MutualFundId.HasValue)
                            {
                                int? fileId = null;
                                
                                // Check if this is a sheet-based detection
                                if (IsSheetBasedDetection(detection.FileName))
                                {
                                    var sheetIndex = ExtractSheetIndex(detection.FileName);
                                    if (sheetIndex.HasValue)
                                    {
                                        fileId = await ProcessSingleSheet(file, sheetIndex.Value, detection.MutualFundId.Value, request.ReportDate);
                                        processedSheetsCount++;
                                    }
                                }
                                else
                                {
                                    // Regular single-file processing
                                    fileId = await ProcessSingleFile(file, detection.MutualFundId.Value, request.ReportDate);
                                }

                                if (fileId.HasValue)
                                {
                                    processedFileIds.Add(fileId.Value);
                                }

                                if (detection.WasCreated)
                                {
                                    createdFundsCount++;
                                }
                            }
                            else
                            {
                                _logger.LogWarning("Could not detect mutual fund for {FileName}", detection.FileName);
                                response.FileErrors.Add(new FileError
                                {
                                    FileName = detection.FileName,
                                    ErrorMessage = "Could not detect mutual fund",
                                    ErrorType = "Detection"
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing detection for {FileName}", detection.FileName);
                            response.FileErrors.Add(new FileError
                            {
                                FileName = detection.FileName,
                                ErrorMessage = $"Processing failed: {ex.Message}",
                                ErrorType = "Processing"
                            });
                        }
                    }
                }
            }

            var totalProcessed = processedFileIds.Count;
            var totalFiles = request.Files.Count;
            var failedFiles = response.FileErrors.Count;
            
            // Determine success status and message
            if (totalProcessed > 0)
            {
                // Partial or complete success
                response.Success = true;
                
                if (failedFiles > 0)
                {
                    response.Message = processedSheetsCount > 0 
                        ? $"Partially completed: {totalProcessed} items processed ({processedSheetsCount} sheets from multi-sheet files), {failedFiles} files failed. Created {createdFundsCount} new mutual funds."
                        : $"Partially completed: {totalProcessed} files processed, {failedFiles} files failed. Created {createdFundsCount} new mutual funds.";
                }
                else
                {
                    response.Message = processedSheetsCount > 0 
                        ? $"Successfully processed {totalProcessed} items ({processedSheetsCount} sheets from multi-sheet files). Created {createdFundsCount} new mutual funds."
                        : $"Successfully processed {totalProcessed} files. Created {createdFundsCount} new mutual funds.";
                }
            }
            else
            {
                // Complete failure
                response.Success = false;
                response.Message = failedFiles > 0 
                    ? $"All {failedFiles} files failed to process."
                    : "No files were processed.";
            }

            response.DetectionResults = detectionResults;
            response.ProcessedFileIds = processedFileIds;
            response.ProcessedFiles = totalProcessed;
            response.CreatedMutualFunds = createdFundsCount;

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in V2 upload process");
            response.Success = false;
            response.Message = $"Upload failed: {ex.Message}";
            response.DetectionResults = detectionResults;
            return response;
        }
    }

    private async Task<int?> ProcessSingleFile(IFormFile file, int mutualFundId, DateTime reportDate)
    {
        try
        {
            // Validate mutual fund exists
            var mutualFund = await _context.MutualFunds.FindAsync(mutualFundId);
            if (mutualFund == null)
            {
                _logger.LogError("Mutual fund with ID {MutualFundId} not found for file {FileName}", mutualFundId, file.FileName);
                return null;
            }

            // Validate file using existing FileProcessingService
            if (!await _fileProcessingService.ValidateFileAsync(file))
            {
                _logger.LogError("File {FileName} failed validation", file.FileName);
                return null;
            }

            // Check for duplicate file using existing FileProcessingService
            var fileHash = await _fileProcessingService.CalculateFileHashAsync(file);
            var existingFile = await _context.PortfolioFiles
                .FirstOrDefaultAsync(f => f.FileHash == fileHash && f.MutualFundId == mutualFundId);

            if (existingFile != null)
            {
                _logger.LogWarning("File {FileName} already exists for mutual fund {MutualFundId}", file.FileName, mutualFundId);
                return existingFile.Id;
            }

            // Process the file using existing file processing service
            var processingResult = await _fileProcessingService.ProcessExcelFileAsync(file, mutualFundId, reportDate);
            
            if (!processingResult.Success || processingResult.PortfolioFile == null)
            {
                _logger.LogError("Failed to process file {FileName}: {Message}", file.FileName, processingResult.Message);
                return null;
            }

            _logger.LogInformation("Successfully processed file {FileName} for fund {FundId}", 
                file.FileName, mutualFundId);

            return processingResult.PortfolioFile.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing file {FileName} for mutual fund {MutualFundId}", 
                file.FileName, mutualFundId);
            return null;
        }
    }

    private int GetSheetCount(IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            var workbook = CreateWorkbook(stream, file.FileName);
            return workbook.NumberOfSheets;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sheet count from file {FileName}", file.FileName);
            return 1; // Default to single sheet
        }
    }

    private async Task DetectMutualFundsFromMultiSheetFile(IFormFile file, int sheetCount, List<MutualFundDetectionResult> detectionResults)
    {
        _logger.LogInformation("Processing multi-sheet file {FileName} with {SheetCount} sheets", file.FileName, sheetCount);
        
        using var stream = file.OpenReadStream();
        var workbook = CreateWorkbook(stream, file.FileName);
        
        for (int sheetIndex = 0; sheetIndex < sheetCount; sheetIndex++)
        {
            try
            {
                var detectionResult = await DetectMutualFundFromSheet(workbook, sheetIndex, file.FileName);
                detectionResults.Add(detectionResult);
                
                _logger.LogInformation("Processed sheet {SheetIndex} from file {FileName}. Detected fund: {FundName}", 
                    sheetIndex + 1, file.FileName, detectionResult.DetectedFundName ?? "None");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing sheet {SheetIndex} from file {FileName}", sheetIndex + 1, file.FileName);
                
                // Add a failed detection result
                detectionResults.Add(new MutualFundDetectionResult
                {
                    FileName = $"{file.FileName} - Sheet {sheetIndex + 1}",
                    DetectedFundName = "Error processing sheet"
                });
            }
        }
    }

    private async Task<MutualFundDetectionResult> DetectMutualFundFromSheet(IWorkbook workbook, int sheetIndex, string fileName)
    {
        var result = new MutualFundDetectionResult
        {
            FileName = $"{fileName} - Sheet {sheetIndex + 1}"
        };

        try
        {
            var worksheet = workbook.GetSheetAt(sheetIndex);
            if (worksheet == null)
            {
                _logger.LogWarning("Sheet {SheetIndex} not found in file {FileName}", sheetIndex, fileName);
                return result;
            }

            // Use the direct sheet detection method to avoid SheetFormFile wrapper
            var tempFileName = $"{fileName}_Sheet{sheetIndex + 1}";
            var detectionResult = await _detectionService.DetectMutualFundFromSheet(workbook, sheetIndex, tempFileName);
            
            // Update the filename to indicate it's from a specific sheet
            detectionResult.FileName = result.FileName;
            
            return detectionResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting mutual fund from sheet {SheetIndex} in file {FileName}", sheetIndex, fileName);
            result.DetectedFundName = "Error processing sheet";
            return result;
        }
    }

    private string ExtractOriginalFileName(string fileName)
    {
        // If filename contains " - Sheet ", extract the original part
        var sheetIndex = fileName.IndexOf(" - Sheet ", StringComparison.OrdinalIgnoreCase);
        return sheetIndex > 0 ? fileName.Substring(0, sheetIndex) : fileName;
    }

    private bool IsSheetBasedDetection(string fileName)
    {
        return fileName.Contains(" - Sheet ", StringComparison.OrdinalIgnoreCase);
    }

    private int? ExtractSheetIndex(string fileName)
    {
        try
        {
            // Extract sheet number from filename like "OriginalFile - Sheet 2"
            var match = System.Text.RegularExpressions.Regex.Match(fileName, @"Sheet (\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int sheetNumber))
            {
                return sheetNumber - 1; // Convert to 0-based index
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting sheet index from filename {FileName}", fileName);
        }
        
        return null;
    }

    private async Task<int?> ProcessSingleSheet(IFormFile file, int sheetIndex, int mutualFundId, DateTime reportDate)
    {
        try
        {
            // Use FileUploadService logic for processing with sheet-specific handling
            var processedFile = await _fileUploadService.ProcessSingleFileAsync(file, mutualFundId, reportDate, sheetIndex);
            
            if (processedFile.Status == "Success")
            {
                _logger.LogInformation("Successfully processed sheet {SheetIndex} from file {FileName} for fund {FundId}", 
                    sheetIndex + 1, file.FileName, mutualFundId);
                return processedFile.FileId;
            }
            else
            {
                _logger.LogError("Failed to process sheet {SheetIndex} from file {FileName}: {Message}", 
                    sheetIndex + 1, file.FileName, processedFile.ErrorMessage);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing sheet {SheetIndex} from file {FileName} for mutual fund {MutualFundId}", 
                sheetIndex + 1, file.FileName, mutualFundId);
            return null;
        }
    }


    private IWorkbook CreateWorkbook(Stream stream, string fileName)
    {
        try
        {
            // Use WorkbookFactory.Create which automatically detects the format
            return WorkbookFactory.Create(stream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create workbook from file {FileName}. Trying fallback method.", fileName);
            
            // Fallback to manual detection
            stream.Position = 0;
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".xlsx" => new XSSFWorkbook(stream),
                ".xls" => new HSSFWorkbook(stream),
                _ => throw new ArgumentException($"Unsupported file format: {extension}")
            };
        }
    }

}
