using Microsoft.AspNetCore.Http;

namespace MutualFundScanner.Application.DTOs;

public class FileUploadRequestDto
{
    public int MutualFundId { get; set; }
    public List<IFormFile> Files { get; set; } = new();
    public string? ReportDate { get; set; }
}

public class FileUploadResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<ProcessedFileDto> ProcessedFiles { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

public class ProcessedFileDto
{
    public int FileId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
    public int HoldingsCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

public class MultipleFileUploadRequestDto
{
    public List<FileUploadItem> FileUploads { get; set; } = new();
}

public class FileUploadItem
{
    public int MutualFundId { get; set; }
    public IFormFile File { get; set; } = null!;
    public string? ReportDate { get; set; }
}
