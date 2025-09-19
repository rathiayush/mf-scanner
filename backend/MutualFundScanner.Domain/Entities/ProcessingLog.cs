using MutualFundScanner.Domain.Enums;

namespace MutualFundScanner.Domain.Entities;

public class ProcessingLog
{
    public long Id { get; set; }
    public int PortfolioFileId { get; set; }
    public LogLevel LogLevel { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public virtual PortfolioFile PortfolioFile { get; set; } = null!;
}
