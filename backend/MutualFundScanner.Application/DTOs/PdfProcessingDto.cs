using Microsoft.AspNetCore.Http;

namespace MutualFundScanner.Application.DTOs;

public class PdfProcessingRequestDto
{
    public IFormFile File { get; set; } = null!;
    public static List<string> KnownFundNames { get; set; } = new List<string> { "KOTAK BLUECHIP FUND" };
    public string? OutputFolder { get; set; }
}

public class PdfProcessingResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? DetectedFundName { get; set; }
    public List<PdfExtractedTableData> ExtractedTables { get; set; } = new();
    public List<CreatedExcelFile> CreatedExcelFiles { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

public class CreatedExcelFile
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int PageNumber { get; set; }
    public int RecordCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PdfExtractedTableData
{
    public int PageNumber { get; set; }
    public string TableName { get; set; } = string.Empty;
    public List<PdfTableRow> Rows { get; set; } = new();
}

public class PdfTableRow
{
    public string Issuer { get; set; } = string.Empty;
    public string Percentage { get; set; }
    public string RawText { get; set; } = string.Empty;
}

public class PdfPageData
{
    public int PageNumber { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<string> Lines { get; set; } = new();
    public bool ContainsTargetTable { get; set; }
}

public class FundDetectionResult
{
    public string FundName { get; set; } = string.Empty;
    public int ConfidenceScore { get; set; }
    public int PageNumber { get; set; }
    public string MatchedText { get; set; } = string.Empty;
}
