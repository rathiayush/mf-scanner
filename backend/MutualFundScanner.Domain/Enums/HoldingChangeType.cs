namespace MutualFundScanner.Domain.Enums;

public enum HoldingChangeType
{
    Added = 0,      // New holding added to portfolio
    Removed = 1,    // Holding completely removed from portfolio
    Increased = 2,  // Quantity/weight increased
    Decreased = 3,  // Quantity/weight decreased
    Unchanged = 4   // No change in position
}
