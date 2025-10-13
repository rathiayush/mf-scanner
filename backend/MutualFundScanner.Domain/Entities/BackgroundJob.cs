using MutualFundScanner.Domain.Enums;

namespace MutualFundScanner.Domain.Entities;

public class BackgroundJob
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string JobType { get; set; } = string.Empty; // "FileProcessing"
    public BackgroundJobStatus Status { get; set; }
    public string? DirectoryPath { get; set; }
    public DateTime ReportDate { get; set; }
    public bool PerformDateCheck { get; set; }
    public string FilePatterns { get; set; } = string.Empty; // JSON serialized
    public bool GenerateExcelReport { get; set; }
    
    // Progress tracking
    public int TotalFiles { get; set; }
    public int ProcessedFiles { get; set; }
    public int SuccessfulFiles { get; set; }
    public int FailedFiles { get; set; }
    public string? CurrentFile { get; set; }
    
    // Timing
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    // Results
    public string? ResultJson { get; set; } // Serialized BackgroundProcessingResultDto
    public string? ExcelReportPath { get; set; }
    public string? ErrorMessage { get; set; }
    
    // Metadata
    public string CreatedBy { get; set; } = "System";
    public string? Notes { get; set; }
}
