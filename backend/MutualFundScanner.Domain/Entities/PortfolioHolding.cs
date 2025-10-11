using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MutualFundScanner.Domain.Entities;

public class PortfolioHolding
{
    public long Id { get; set; }
    public int PortfolioFileId { get; set; }
    public int MutualFundId { get; set; }
    public long PortfolioSnapshotId { get; set; }
    public string? ISIN { get; set; }
    public string? Ticker { get; set; }
    public string? SecurityName { get; set; }
    public string? Sector { get; set; }
    public string? Industry { get; set; }
    [Column(TypeName = "decimal(18, 4)")]
    public decimal Quantity { get; set; }
    [Column(TypeName = "decimal(18, 4)")]
    public decimal MarketValue { get; set; }
    [Column(TypeName = "decimal(18, 4)")]
    public decimal Weight { get; set; }
    [Column(TypeName = "decimal(18, 4)")]
    public decimal? PreviousWeight { get; set; } // Weight in previous month for comparison
    public DateTime AsOfDate { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    public virtual PortfolioFile PortfolioFile { get; set; } = null!;
    public virtual MutualFund MutualFund { get; set; } = null!;
    public virtual PortfolioSnapshot PortfolioSnapshot { get; set; } = null!;
}
