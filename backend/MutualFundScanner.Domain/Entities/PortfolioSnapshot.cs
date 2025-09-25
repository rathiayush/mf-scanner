using System.ComponentModel.DataAnnotations.Schema;
using MutualFundScanner.Domain.Enums;

namespace MutualFundScanner.Domain.Entities;

/// <summary>
/// Represents a monthly portfolio snapshot for historical tracking
/// </summary>
public class PortfolioSnapshot
{
    public int Id { get; set; }
    public int MutualFundId { get; set; }
    public DateTime SnapshotDate { get; set; } // Month-end date
    [Column(TypeName = "decimal(18, 4)")]
    public decimal TotalValue { get; set; }
    public int TotalHoldings { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    public virtual MutualFund MutualFund { get; set; } = null!;
    public virtual ICollection<PortfolioHolding> Holdings { get; set; } = new List<PortfolioHolding>();
}
