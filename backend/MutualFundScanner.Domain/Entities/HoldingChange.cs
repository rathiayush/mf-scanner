using System.ComponentModel.DataAnnotations.Schema;
using MutualFundScanner.Domain.Enums;

namespace MutualFundScanner.Domain.Entities;

/// <summary>
/// Tracks changes in holdings between monthly reports
/// </summary>
public class HoldingChange
{
    public long Id { get; set; }
    public int MutualFundId { get; set; }
    public int PortfolioSnapshotId { get; set; }
    public string? ISIN { get; set; }
    public string? Ticker { get; set; }
    public string? SecurityName { get; set; }
    public HoldingChangeType ChangeType { get; set; }
    [Column(TypeName = "decimal(18, 4)")]
    public decimal? PreviousQuantity { get; set; }
    [Column(TypeName = "decimal(18, 4)")]
    public decimal? NewQuantity { get; set; }
    [Column(TypeName = "decimal(18, 4)")]
    public decimal? PreviousWeight { get; set; }
    [Column(TypeName = "decimal(18, 4)")]
    public decimal? NewWeight { get; set; }
    [Column(TypeName = "decimal(18, 4)")]
    public decimal? PreviousValue { get; set; }
    [Column(TypeName = "decimal(18, 4)")]
    public decimal? NewValue { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    public virtual MutualFund MutualFund { get; set; } = null!;
    public virtual PortfolioSnapshot PortfolioSnapshot { get; set; } = null!;
}
