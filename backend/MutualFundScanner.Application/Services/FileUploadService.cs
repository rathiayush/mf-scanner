using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MutualFundScanner.Application.DTOs;
using MutualFundScanner.Application.Helpers;
using MutualFundScanner.Application.Interfaces;
using MutualFundScanner.Infrastructure.Data;
using MutualFundScanner.Domain.Entities;

namespace MutualFundScanner.Application.Services;

public class FileUploadService : IFileUploadService
{
    private readonly MutualFundContext _context;
    private readonly IFileProcessingService _fileProcessingService;
    private readonly ILogger<FileUploadService> _logger;
    private readonly string[] _allowedExtensions = { ".xlsx", ".xls", ".csv" };
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public FileUploadService(
        MutualFundContext context, 
        IFileProcessingService fileProcessingService,
        ILogger<FileUploadService> logger)
    {
        _context = context;
        _fileProcessingService = fileProcessingService;
        _logger = logger;
    }


    public async Task<FileUploadResponseDto> UploadFilesAsync(FileUploadRequestDto request)
    {
        var response = new FileUploadResponseDto { Success = true };

        // Validate mutual fund exists
        var mutualFund = await _context.MutualFunds.FindAsync(request.MutualFundId);
        if (mutualFund == null)
        {
            response.Success = false;
            response.Message = $"Mutual fund with ID {request.MutualFundId} not found";
            return response;
        }

        // Parse report date if provided
        DateTime? reportDate = null;
        if (!string.IsNullOrEmpty(request.ReportDate) && DateTime.TryParse(request.ReportDate, out var parsedDate))
        {
            reportDate = parsedDate;
        }

        foreach (var file in request.Files)
        {
            try
            {
                // Validate file
                if (!await ValidateFileAsync(file))
                {
                    response.Errors.Add($"File {file.FileName} failed validation");
                    continue;
                }

                // Validate date consistency between reported date and filename
                var dateValidationResult = DateValidationHelper.ValidateDateConsistency(file.FileName, reportDate);
                if (!dateValidationResult.IsValid)
                {
                    response.Errors.Add($"File {file.FileName}: {dateValidationResult.ErrorMessage}");
                    response.ProcessedFiles.Add(new ProcessedFileDto
                    {
                        FileName = file.FileName,
                        Status = "Error",
                        ErrorMessage = dateValidationResult.ErrorMessage
                    });
                    continue;
                }

                // Process file
                var processedFile = await ProcessSingleFileAsync(file, request.MutualFundId, reportDate);
                response.ProcessedFiles.Add(processedFile);

                _logger.LogInformation("Successfully processed file {FileName} for fund {FundId}", 
                    file.FileName, request.MutualFundId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file {FileName} for fund {FundId}", 
                    file.FileName, request.MutualFundId);
                
                response.Errors.Add($"Error processing {file.FileName}: {ex.Message}");
                response.ProcessedFiles.Add(new ProcessedFileDto
                {
                    FileName = file.FileName,
                    Status = "Error",
                    ErrorMessage = ex.Message
                });
            }
        }

        if (response.Errors.Any())
        {
            response.Success = false;
            response.Message = $"Processed {response.ProcessedFiles.Count(f => f.Status == "Success")} files successfully, {response.Errors.Count} failed";
        }
        else
        {
            response.Message = $"Successfully processed {response.ProcessedFiles.Count} files";
        }

        return response;
    }

    public async Task<FileUploadResponseDto> UploadMultipleFilesAsync(MultipleFileUploadRequestDto request)
    {
        var response = new FileUploadResponseDto { Success = true };

        foreach (var fileUpload in request.FileUploads)
        {
            try
            {
                // Validate mutual fund exists
                var mutualFund = await _context.MutualFunds.FindAsync(fileUpload.MutualFundId);
                if (mutualFund == null)
                {
                    response.Errors.Add($"Mutual fund with ID {fileUpload.MutualFundId} not found for file {fileUpload.File.FileName}");
                    continue;
                }

                // Validate file
                if (!await ValidateFileAsync(fileUpload.File))
                {
                    response.Errors.Add($"File {fileUpload.File.FileName} failed validation");
                    continue;
                }

                // Parse report date if provided
                DateTime? reportDate = null;
                if (!string.IsNullOrEmpty(fileUpload.ReportDate) && DateTime.TryParse(fileUpload.ReportDate, out var parsedDate))
                {
                    reportDate = parsedDate;
                }

                // Process file
                var processedFile = await ProcessSingleFileAsync(fileUpload.File, fileUpload.MutualFundId, reportDate);
                response.ProcessedFiles.Add(processedFile);

                _logger.LogInformation("Successfully processed file {FileName} for fund {FundId}", 
                    fileUpload.File.FileName, fileUpload.MutualFundId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file {FileName} for fund {FundId}", 
                    fileUpload.File.FileName, fileUpload.MutualFundId);
                
                response.Errors.Add($"Error processing {fileUpload.File.FileName}: {ex.Message}");
                response.ProcessedFiles.Add(new ProcessedFileDto
                {
                    FileName = fileUpload.File.FileName,
                    Status = "Error",
                    ErrorMessage = ex.Message
                });
            }
        }

        if (response.Errors.Any())
        {
            response.Success = false;
            response.Message = $"Processed {response.ProcessedFiles.Count(f => f.Status == "Success")} files successfully, {response.Errors.Count} failed";
        }
        else
        {
            response.Message = $"Successfully processed {response.ProcessedFiles.Count} files";
        }

        return response;
    }

    public async Task<List<ProcessedFileDto>> GetProcessedFilesAsync(int mutualFundId)
    {
        var files = await _context.PortfolioFiles
            .Where(f => f.MutualFundId == mutualFundId)
            .OrderByDescending(f => f.ReportDate)
            .Select(f => new ProcessedFileDto
            {
                FileId = f.Id,
                FileName = f.FileName,
                ReportDate = f.ReportDate,
                HoldingsCount = f.Holdings.Count,
                //Status = f.ProcessingStatus
            })
            .ToListAsync();

        return files;
    }

    public async Task<bool> ValidateFileAsync(IFormFile file)
    {
        // Check file size
        if (file.Length > MaxFileSize)
        {
            _logger.LogWarning("File {FileName} exceeds maximum size limit", file.FileName);
            return false;
        }

        // Check file extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            _logger.LogWarning("File {FileName} has unsupported extension {Extension}", file.FileName, extension);
            return false;
        }

        // Check if file is empty
        if (file.Length == 0)
        {
            _logger.LogWarning("File {FileName} is empty", file.FileName);
            return false;
        }

        return true;
    }

    public async Task<ProcessedFileDto> ProcessSingleFileAsync(IFormFile file, int mutualFundId, DateTime? reportDate = null)
    {
        return await ProcessSingleFileAsync(file, mutualFundId, reportDate, null);
    }

    public async Task<ProcessedFileDto> ProcessSingleFileAsync(IFormFile file, int mutualFundId, DateTime? reportDate, int? sheetIndex)
    {
        // Create temporary file
        var tempPath = Path.GetTempFileName();
        var originalExtension = Path.GetExtension(file.FileName);
        var tempFileWithExtension = Path.ChangeExtension(tempPath, originalExtension);

        try
        {
            // Save uploaded file to temp location
            using (var stream = new FileStream(tempFileWithExtension, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Check for duplicate file
            var fileHash = await CalculateFileHashAsync(tempFileWithExtension);
            var existingFile = await _context.PortfolioFiles
                .FirstOrDefaultAsync(f => f.FileHash == fileHash && f.MutualFundId == mutualFundId);

            if (existingFile != null)
            {
                return new ProcessedFileDto
                {
                    FileId = existingFile.Id,
                    FileName = file.FileName,
                    ReportDate = existingFile.ReportDate,
                    HoldingsCount = existingFile.Holdings.Count,
                    Status = "Duplicate",
                    ErrorMessage = "File already exists"
                };
            }

            // Process the file using existing file processing service
            var processingResult = sheetIndex.HasValue 
                ? await _fileProcessingService.ProcessExcelFileAsync(file, mutualFundId, reportDate, sheetIndex.Value)
                : await _fileProcessingService.ProcessExcelFileAsync(file, mutualFundId, reportDate);
            
            if (!processingResult.Success || processingResult.PortfolioFile == null)
            {
                return new ProcessedFileDto
                {
                    FileName = file.FileName,
                    Status = "Failed",
                    ErrorMessage = processingResult.Message
                };
            }

            var portfolioFile = processingResult.PortfolioFile;
            return new ProcessedFileDto
            {
                FileId = portfolioFile.Id,
                FileName = file.FileName,
                ReportDate = portfolioFile.ReportDate,
                HoldingsCount = portfolioFile.Holdings.Count,
                Status = "Success"
            };
        }
        finally
        {
            // Clean up temp files
            if (File.Exists(tempPath))
                File.Delete(tempPath);
            if (File.Exists(tempFileWithExtension))
                File.Delete(tempFileWithExtension);
        }
    }

    private async Task<string> CalculateFileHashAsync(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = await Task.Run(() => sha256.ComputeHash(stream));
        return Convert.ToBase64String(hash);
    }
}
