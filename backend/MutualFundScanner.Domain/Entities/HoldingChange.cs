using MutualFundScanner.Domain.Enums;

namespace MutualFundScanner.Domain.Entities;

/// <summary>
/// Tracks changes in holdings between monthly reports
/// </summary>
public class HoldingChange
{
    public long Id { get; set; }
    public int MutualFundId { get; set; }
    public string? ISIN { get; set; }
    public string? Ticker { get; set; }
    public string? SecurityName { get; set; }
    public HoldingChangeType ChangeType { get; set; }
    public decimal? PreviousQuantity { get; set; }
    public decimal? NewQuantity { get; set; }
    public decimal? PreviousWeight { get; set; }
    public decimal? NewWeight { get; set; }
    public decimal? PreviousValue { get; set; }
    public decimal? NewValue { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    public virtual MutualFund MutualFund { get; set; } = null!;
}
