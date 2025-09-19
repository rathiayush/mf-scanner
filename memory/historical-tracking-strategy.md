# Historical Portfolio Tracking Strategy

## Data Model for Historical Analysis

### Key Requirements
- Each mutual fund can have multiple monthly portfolio files
- Track portfolio evolution over time (holdings added/removed/changed)
- Maintain complete historical record for analysis
- Support portfolio comparison between different periods

### Enhanced Database Schema

#### Core Entities
1. **MutualFund**: Base fund information and folder monitoring
2. **PortfolioFile**: Individual Excel files with ReportDate for monthly tracking
3. **PortfolioSnapshot**: Monthly aggregated portfolio view
4. **PortfolioHolding**: Individual security positions with historical context
5. **HoldingChange**: Tracks changes between monthly reports

#### Historical Tracking Features

##### Monthly Portfolio Snapshots
```csharp
public class PortfolioSnapshot
{
    public DateTime SnapshotDate { get; set; } // Month-end date
    public decimal TotalValue { get; set; }
    public int TotalHoldings { get; set; }
    // Linked to holdings for that specific month
}
```

##### Holding Change Tracking
```csharp
public enum HoldingChangeType
{
    Added,      // New holding added
    Removed,    // Holding completely removed
    Increased,  // Position size increased
    Decreased,  // Position size decreased
    Unchanged   // No change in position
}
```

##### Enhanced Holdings with History
- **PreviousWeight**: Weight in previous month for comparison
- **Sector/Industry**: Additional classification for analysis
- **PortfolioSnapshotId**: Links to monthly snapshot

### Processing Workflow for Historical Data

#### File Processing Steps
1. **File Detection**: Monitor folders for new monthly files
2. **Date Extraction**: Extract report date from filename or content
3. **Duplicate Prevention**: Check if month already processed for fund
4. **Historical Comparison**: Compare with previous month's data
5. **Change Detection**: Identify added/removed/modified holdings
6. **Snapshot Creation**: Create monthly portfolio snapshot
7. **Change Logging**: Record all holding changes

#### Change Detection Algorithm
```csharp
// Compare current month holdings with previous month
var changes = new List<HoldingChange>();
var previousHoldings = GetPreviousMonthHoldings(fundId, reportDate);
var currentHoldings = GetCurrentMonthHoldings(fundId, reportDate);

// Detect additions, removals, and modifications
foreach (var current in currentHoldings)
{
    var previous = previousHoldings.FirstOrDefault(p => p.ISIN == current.ISIN);
    if (previous == null)
        changes.Add(CreateChange(HoldingChangeType.Added, current));
    else if (HasSignificantChange(previous, current))
        changes.Add(CreateChange(GetChangeType(previous, current), current, previous));
}

// Detect removals
foreach (var previous in previousHoldings)
{
    if (!currentHoldings.Any(c => c.ISIN == previous.ISIN))
        changes.Add(CreateChange(HoldingChangeType.Removed, previous));
}
```

### Analysis Capabilities

#### Portfolio Evolution Analysis
- Month-over-month holding changes
- Sector allocation trends
- Top additions/removals by value
- Portfolio turnover metrics
- Concentration analysis over time

#### Reporting Features
- Historical portfolio composition charts
- Holding change timeline
- Performance attribution analysis
- Risk metrics evolution
- Benchmark comparison over time
