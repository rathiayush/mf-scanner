namespace MutualFundScanner.Application.DTOs;

public class MutualFundDto
{
    public int Id { get; set; }
    public string FundCode { get; set; } = string.Empty;
    public string FundName { get; set; } = string.Empty;
    public string AMC { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string SubCategory { get; set; } = string.Empty;
    public DateTime? LaunchDate { get; set; }
    public decimal? NAV { get; set; }
    public decimal? AUM { get; set; }
    public decimal? ExpenseRatio { get; set; }
    public string Status { get; set; } = "Active";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; }
    public DateTime? LastUpdated { get; set; }
    public int PortfolioCount { get; set; }
    public DateTime? LatestPortfolioDate { get; set; }
}

public class CreateMutualFundDto
{
    public string FundName { get; set; } = string.Empty;
    public string? FundCode { get; set; }
    public string? AMC { get; set; }
    public string? Category { get; set; }
    public string? SubCategory { get; set; }
    public DateTime? LaunchDate { get; set; }
    public decimal? NAV { get; set; }
    public decimal? AUM { get; set; }
    public decimal? ExpenseRatio { get; set; }
}

public class UpdateMutualFundDto
{
    public int Id { get; set; }
    public string FundName { get; set; } = string.Empty;
    public string? FundCode { get; set; }
    public string? AMC { get; set; }
    public string? Category { get; set; }
    public string? SubCategory { get; set; }
    public DateTime? LaunchDate { get; set; }
    public decimal? NAV { get; set; }
    public decimal? AUM { get; set; }
    public decimal? ExpenseRatio { get; set; }
    public string Status { get; set; } = "Active";
    public bool IsActive { get; set; } = true;
}

public class MutualFundSearchDto
{
    public string? SearchTerm { get; set; }
    public string? Category { get; set; }
    public string? AMC { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class PagedResult<T>
{
    public List<T> Data { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
