namespace MutualFundScanner.Application.DTOs;

public class AnalyticsRequestDto
{
    public List<int> MutualFundIds { get; set; } = new();
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public List<string>? ChangeTypeFilters { get; set; } // One or more of: Added, Removed, Increased, Decreased, Mixed, or null/empty for all
    public string? ChangeTypeMatchMode { get; set; } = "any"; // 'any' or 'only'
    public List<string>? SectorFilters { get; set; } // Filter by specific sectors
    public string? SecurityNameSearch { get; set; } // Search by security name (partial match)
    public decimal? WeightChangeMin { get; set; } // Minimum weight change percentage
    public decimal? WeightChangeMax { get; set; } // Maximum weight change percentage
    public int? MinFundsCount { get; set; } // Minimum number of funds holding the security
    public string? SortBy { get; set; } // Column to sort by: SecurityName, Sector, Industry, PreviousWeight, CurrentWeight, WeightChange, ChangeType, MutualFundsCount
    public string? SortDirection { get; set; } = "desc"; // asc or desc
}

public class AnalyticsResponseDto
{
    public List<int> MutualFundIds { get; set; } = new();
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public PagedResult<HoldingChangeDto> AddedRemovedHoldings { get; set; } = new();
    public PagedResult<TopShareDto> TopShares { get; set; } = new();
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
    public decimal? PreviousQuantity { get; set; }
    public decimal CurrentQuantity { get; set; }
    public decimal QuantityChange { get; set; }
    public decimal? PreviousValue { get; set; }
    public decimal CurrentValue { get; set; }
    public decimal ValueChange { get; set; }
    public string ChangeType { get; set; } = string.Empty; // Added, Removed, Increased, Decreased, Mixed
    public List<string> MutualFundNames { get; set; } = new();
    public List<FundChangeDetailDto> FundChangeDetails { get; set; } = new(); // Details for each fund's contribution
    public List<string> ConstituentChangeTypes { get; set; } = new(); // For Mixed: list of constituent change types (Added, Removed, etc.)
    public Dictionary<string, int> ChangeTypeCounts { get; set; } = new(); // Count of each change type for UI display
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
    public decimal? PreviousQuantity { get; set; }
    public decimal CurrentQuantity { get; set; }
    public decimal QuantityChange { get; set; }
    public decimal? PreviousValue { get; set; }
    public decimal CurrentValue { get; set; }
    public decimal ValueChange { get; set; }
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

public class TopSharesRequestDto
{
    public List<int> MutualFundIds { get; set; } = new();
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 15;
    public string? SortBy { get; set; } // SecurityName, Sector, Industry, TotalWeight, HoldingFundsCount, ISIN
    public string? SortDirection { get; set; } = "desc"; // asc or desc
}

public class SecurityAnalyticsRequestDto
{
    public string? ISIN { get; set; }
    public string? SecurityName { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } // Date, ChangeType, MutualFundName, WeightChange
    public string? SortDirection { get; set; } = "desc"; // asc or desc
}

public class SecurityMonthlyChangeDto
{
    public DateTime MonthYear { get; set; }
    public List<MonthlyChangeTypeDto> ChangeTypes { get; set; } = new();
    public int TotalMutualFundsCount { get; set; }
    public decimal TotalAbsoluteWeightChange { get; set; }
    public List<SecurityFundChangeDto> AllFundChanges { get; set; } = new();
}

public class MonthlyChangeTypeDto
{
    public string ChangeType { get; set; } = string.Empty;
    public int MutualFundsCount { get; set; }
    public decimal TotalWeightChange { get; set; }
}

public class SecurityFundChangeDto
{
    public int MutualFundId { get; set; }
    public string MutualFundName { get; set; } = string.Empty;
    public decimal PreviousWeight { get; set; }
    public decimal CurrentWeight { get; set; }
    public decimal WeightChange { get; set; }
    public string ChangeType { get; set; } = string.Empty;
}

public class SecurityAnalyticsResponseDto
{
    public string ISIN { get; set; } = string.Empty;
    public string SecurityName { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<SecurityMonthlyChangeDto> MonthlyChanges { get; set; } = new();
    public int TotalMutualFunds { get; set; }
    public SecurityAnalyticsSummaryDto Summary { get; set; } = new();
}

public class SecurityAnalyticsSummaryDto
{
    public int TotalMonthsAnalyzed { get; set; }
    public int MonthsWithChanges { get; set; }
    public int TotalMutualFundsInvolved { get; set; }
    public decimal TotalAbsoluteWeightChange { get; set; }
    public Dictionary<string, int> ChangeTypeCounts { get; set; } = new();
}

public class SecurityDropdownDto
{
    public string ISIN { get; set; } = string.Empty;
    public string SecurityName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public class AvailableSecuritiesResponseDto
{
    public List<SecurityDropdownDto> Securities { get; set; } = new();
}
