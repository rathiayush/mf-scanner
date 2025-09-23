namespace MutualFundScanner.Application.DTOs;

public class AnalyticsRequestDto
{
    public List<int> MutualFundIds { get; set; } = new();
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? ChangeTypeFilter { get; set; } // Added, Removed, Increased, Decreased, Mixed, or null for all
}

public class AnalyticsResponseDto
{
    public List<int> MutualFundIds { get; set; } = new();
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public PagedResult<HoldingChangeDto> AddedRemovedHoldings { get; set; } = new();
    public List<TopShareDto> TopShares { get; set; } = new();
    public AnalyticsSummaryDto Summary { get; set; } = new();
}

public class HoldingChangeDto
{
    public string ISIN { get; set; } = string.Empty;
    public string SecurityName { get; set; } = string.Empty;
    public string Sector { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public decimal? PreviousWeight { get; set; }
    public decimal CurrentWeight { get; set; }
    public decimal WeightChange { get; set; }
    public string ChangeType { get; set; } = string.Empty; // Added, Removed, Increased, Decreased, Mixed
    public List<string> MutualFundNames { get; set; } = new();
    public List<FundChangeDetailDto> FundChangeDetails { get; set; } = new(); // Details for each fund's contribution
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}

public class FundChangeDetailDto
{
    public int MutualFundId { get; set; }
    public string MutualFundName { get; set; } = string.Empty;
    public decimal? PreviousWeight { get; set; }
    public decimal CurrentWeight { get; set; }
    public decimal WeightChange { get; set; }
    public string ChangeType { get; set; } = string.Empty; // Added, Removed, Increased, Decreased
}

public class TopShareDto
{
    public string ISIN { get; set; } = string.Empty;
    public string SecurityName { get; set; } = string.Empty;
    public string Sector { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public decimal TotalWeight { get; set; }
    public int HoldingFundsCount { get; set; }
    public List<FundHoldingDto> FundHoldings { get; set; } = new();
}

public class FundHoldingDto
{
    public int MutualFundId { get; set; }
    public string MutualFundName { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public decimal? PreviousWeight { get; set; }
    public decimal WeightChange { get; set; }
}

public class AnalyticsSummaryDto
{
    public int TotalHoldingsAnalyzed { get; set; }
    public int AddedHoldings { get; set; }
    public int RemovedHoldings { get; set; }
    public int IncreasedHoldings { get; set; }
    public int DecreasedHoldings { get; set; }
    public int UnchangedHoldings { get; set; }
    public decimal TotalWeightChange { get; set; }
}

public class DateRangeDto
{
    public DateTime Date { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public bool HasData { get; set; }
}

public class AvailableDatesResponseDto
{
    public List<DateRangeDto> AvailableDates { get; set; } = new();
    public DateTime? MinDate { get; set; }
    public DateTime? MaxDate { get; set; }
    public List<MutualFundDateAvailability> FundDateAvailabilities { get; set; } = new();
}

public class MutualFundDateAvailability
{
    public int MutualFundId { get; set; }
    public string MutualFundName { get; set; } = string.Empty;
    public DateTime? MinDate { get; set; }
    public DateTime? MaxDate { get; set; }
    public List<DateTime> AvailableDates { get; set; } = new();
}
