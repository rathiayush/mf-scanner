using MutualFundScanner.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace MutualFundScanner.Application.Interfaces;

public interface IFileProcessingService
{
    Task<FileProcessingResult> ProcessExcelFileAsync(IFormFile file, int mutualFundId, DateTime? reportDate = null);
    Task<FileProcessingResult> ProcessExcelFileAsync(IFormFile file, int mutualFundId, DateTime? reportDate, int sheetIndex);
    Task<FileProcessingResult> ProcessFileAsync(PortfolioFile portfolioFile);
    Task<bool> ValidateFileAsync(IFormFile file);
    Task<string> CalculateFileHashAsync(IFormFile file);
    Task<bool> FileExistsAsync(string fileHash);
    bool IsExcelFile(string fileName);
}

public class FileProcessingResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int ProcessedRecords { get; set; }
    public List<PortfolioHolding> Holdings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public PortfolioFile? PortfolioFile { get; set; }
}
