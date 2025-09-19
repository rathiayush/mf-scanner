using MutualFundScanner.Domain.Enums;

namespace MutualFundScanner.Domain.Entities;

public class PortfolioFile
{
    public int Id { get; set; }
    public int MutualFundId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileHash { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public ProcessingStatus Status { get; set; } = ProcessingStatus.Pending;
    public string? ErrorMessage { get; set; }
    public int RecordCount { get; set; } = 0;
    public DateTime ReportDate { get; set; } // The month/date this portfolio represents
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    public virtual MutualFund MutualFund { get; set; } = null!;
    public virtual ICollection<PortfolioHolding> Holdings { get; set; } = new List<PortfolioHolding>();
    public virtual ICollection<ProcessingLog> Logs { get; set; } = new List<ProcessingLog>();
}
