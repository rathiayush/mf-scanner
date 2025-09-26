using System.ComponentModel.DataAnnotations;

namespace MutualFundScanner.Application.DTOs;

public class CurrentHoldingsRequestDto
{
    [Required(ErrorMessage = "Mutual fund ID is required")]
    public int MutualFundId { get; set; }

    [Required(ErrorMessage = "Target date is required")]
    public DateTime TargetDate { get; set; }

    public DateTime? DateRangeStart { get; set; }
    public DateTime? DateRangeEnd { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } // Symbol, CompanyName, Quantity, Price, Value, Weight, Sector, Industry
    public string? SortDirection { get; set; } = "desc"; // asc or desc
}

public class CurrentHoldingDto
{
    public int Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Value { get; set; }
    public decimal Weight { get; set; }
    public string? Sector { get; set; }
    public string? Industry { get; set; }
}

public class CurrentHoldingsResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int MutualFundId { get; set; }
    public string MutualFundName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal TotalValue { get; set; }
    public PagedResult<CurrentHoldingDto> Holdings { get; set; } = new();
    public int HoldingsCount { get; set; }
}
