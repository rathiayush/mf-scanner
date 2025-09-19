using MutualFundScanner.Domain.Enums;

namespace MutualFundScanner.Domain.Entities;

public class MutualFund
{
    public int Id { get; set; }
    public string FundCode { get; set; } = string.Empty;
    public string FundName { get; set; } = string.Empty;
    public string AMC { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string SubCategory { get; set; } = string.Empty;
    public DateTime LaunchDate { get; set; }
    public decimal NAV { get; set; }
    public decimal AUM { get; set; }
    public decimal ExpenseRatio { get; set; }
    public string Status { get; set; } = "Active";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastUpdated { get; set; }
    public string FolderPath { get; set; } = string.Empty;
    public DateTime? LastScanDate { get; set; }
    public virtual ICollection<PortfolioFile> Files { get; set; } = new List<PortfolioFile>();
    public virtual ICollection<PortfolioSnapshot> Snapshots { get; set; } = new List<PortfolioSnapshot>();
    public virtual ICollection<HoldingChange> HoldingChanges { get; set; } = new List<HoldingChange>();
}
