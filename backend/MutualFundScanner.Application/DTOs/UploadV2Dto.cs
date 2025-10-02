using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MutualFundScanner.Application.DTOs;

public class UploadV2RequestDto
{
    [Required(ErrorMessage = "Report date is required")]
    public DateTime ReportDate { get; set; }

    [Required(ErrorMessage = "At least one file is required")]
    public List<IFormFile> Files { get; set; } = new();

    /// <summary>
    /// Controls whether date validation should be performed on uploaded files
    /// When true, validates that filename dates match the reported date
    /// When false, skips date validation and processes files regardless of filename dates
    /// Default: true
    /// </summary>
    public bool PerformDateCheck { get; set; } = true;
}

public class MutualFundDetectionResult
{
    public string FileName { get; set; } = string.Empty;
    public string DetectedFundName { get; set; } = string.Empty;
    public int? MutualFundId { get; set; }
    public bool WasCreated { get; set; }
    public double MatchConfidence { get; set; }
    public string MatchedText { get; set; } = string.Empty;
    public int RowFound { get; set; }
    public int ColumnFound { get; set; }
}

public class UploadV2ResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<MutualFundDetectionResult> DetectionResults { get; set; } = new();
    public List<int> ProcessedFileIds { get; set; } = new();
    public int CreatedMutualFunds { get; set; }
    public int ProcessedFiles { get; set; }
    public List<FileError> FileErrors { get; set; } = new();
}

public class FileError
{
    public string FileName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string ErrorType { get; set; } = string.Empty; // "Validation", "Processing", "Detection"
}
