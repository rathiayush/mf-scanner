using Microsoft.AspNetCore.Mvc;
using MutualFundScanner.Application.Interfaces;
using MutualFundScanner.Application.DTOs;

namespace MutualFundScanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(
        IAnalyticsService analyticsService,
        ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> GetAnalytics([FromBody] AnalyticsRequestDto request)
    {
        try
        {
            _logger.LogInformation("GetAnalytics called with fundIds: {FundIds}, fromDate: {FromDate}, toDate: {ToDate}, page: {Page}, pageSize: {PageSize}, sortBy: {SortBy}, sortDirection: {SortDirection}", 
                string.Join(",", request.MutualFundIds ?? new List<int>()), request.FromDate, request.ToDate, request.Page, request.PageSize, request.SortBy ?? "default", request.SortDirection ?? "desc");

            // Allow empty MutualFundIds for "All Funds" analysis
            // if (request.MutualFundIds == null || !request.MutualFundIds.Any())
            // {
            //     return BadRequest(new { Message = "At least one mutual fund must be selected" });
            // }

            if (request.FromDate >= request.ToDate)
            {
                return BadRequest(new { Message = "From date must be before to date" });
            }
            
            var result = await _analyticsService.GetAnalyticsAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting analytics for funds {FundIds}", string.Join(",", request.MutualFundIds));
            return StatusCode(500, new { Message = ex.Message });
        }
    }

    [HttpPost("holding-changes")]
    public async Task<IActionResult> GetHoldingChanges([FromBody] AnalyticsRequestDto request)
    {
        try
        {
            _logger.LogInformation("GetHoldingChanges called with fundIds: {FundIds}, fromDate: {FromDate}, toDate: {ToDate}, page: {Page}, pageSize: {PageSize}, sortBy: {SortBy}, sortDirection: {SortDirection}", 
                string.Join(",", request.MutualFundIds ?? new List<int>()), request.FromDate, request.ToDate, request.Page, request.PageSize, request.SortBy ?? "default", request.SortDirection ?? "desc");

            // Allow empty MutualFundIds for "All Funds" analysis
            // if (request.MutualFundIds == null || !request.MutualFundIds.Any())
            // {
            //     return BadRequest(new { Message = "At least one mutual fund must be selected" });
            // }

            var result = await _analyticsService.GetHoldingChangesAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting holding changes for funds {FundIds}", string.Join(",", request.MutualFundIds));
            return StatusCode(500, new { Message = ex.Message });
        }
    }

    [HttpPost("top-shares")]
    public async Task<IActionResult> GetTopShares([FromBody] TopSharesRequestDto request)
    {
        try
        {
            _logger.LogInformation("GetTopShares called with fundIds: {FundIds}, fromDate: {FromDate}, toDate: {ToDate}, page: {Page}, pageSize: {PageSize}, sortBy: {SortBy}, sortDirection: {SortDirection}", 
                string.Join(",", request.MutualFundIds ?? new List<int>()), request.FromDate, request.ToDate, request.Page, request.PageSize, request.SortBy ?? "default", request.SortDirection ?? "desc");
            
            // Allow empty MutualFundIds for "All Funds" analysis
            // if (request.MutualFundIds == null || !request.MutualFundIds.Any())
            // {
            //     return BadRequest(new { Message = "At least one mutual fund must be selected" });
            // }

            if (request.FromDate >= request.ToDate)
            {
                return BadRequest(new { Message = "From date must be before to date" });
            }

            var result = await _analyticsService.GetTopSharesAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top shares for funds {FundIds}", string.Join(",", request.MutualFundIds));
            return StatusCode(500, new { Message = ex.Message });
        }
    }

    [HttpGet("available-dates")]
    public async Task<IActionResult> GetAvailableDates([FromQuery] int[]? mutualFundIds = null)
    {
        try
        {
            var result = await _analyticsService.GetAvailableDatesAsync(mutualFundIds?.ToList());
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available dates");
            return StatusCode(500, new { Message = ex.Message });
        }
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetAnalyticsSummary(
        [FromQuery] int[] mutualFundIds,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        try
        {
            // Allow empty mutualFundIds for "All Funds" analysis
            // if (!mutualFundIds.Any())
            // {
            //     return BadRequest(new { Message = "At least one mutual fund must be selected" });
            // }

            if (fromDate >= toDate)
            {
                return BadRequest(new { Message = "From date must be before to date" });
            }

            var result = await _analyticsService.GetAnalyticsSummaryAsync(mutualFundIds.ToList(), fromDate, toDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting analytics summary for funds {FundIds}", string.Join(",", mutualFundIds));
            return StatusCode(500, new { Message = ex.Message });
        }
    }

    [HttpPost("current-holdings")]
    public async Task<IActionResult> GetCurrentHoldings([FromBody] CurrentHoldingsRequestDto request)
    {
        try
        {
            _logger.LogInformation("GetCurrentHoldings called with fundId: {FundId}, targetDate: {TargetDate}, page: {Page}, pageSize: {PageSize}, sortBy: {SortBy}, sortDirection: {SortDirection}", 
                request.MutualFundId, request.TargetDate, request.Page, request.PageSize, request.SortBy ?? "default", request.SortDirection ?? "desc");

            if (request.MutualFundId <= 0)
            {
                return BadRequest(new { Message = "Valid mutual fund ID is required" });
            }

            if (request.TargetDate == default)
            {
                return BadRequest(new { Message = "Valid target date is required" });
            }

            var result = await _analyticsService.GetCurrentHoldingsAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current holdings for fund {FundId} on date {Date}", 
                request.MutualFundId, request.TargetDate);
            return StatusCode(500, new { Message = ex.Message });
        }
    }

    [HttpGet("validate-dates")]
    public async Task<IActionResult> ValidateDatesForFunds(
        [FromQuery] string? mutualFundIds = null,
        [FromQuery] DateTime fromDate = default,
        [FromQuery] DateTime toDate = default)
    {
        try
        {
            _logger.LogInformation("ValidateDatesForFunds called with mutualFundIds: {FundIds}, fromDate: {FromDate}, toDate: {ToDate}", 
                mutualFundIds, fromDate, toDate);
            
            // Parse mutualFundIds from comma-separated string
            List<int> fundIds = new List<int>();
            if (!string.IsNullOrEmpty(mutualFundIds))
            {
                var ids = mutualFundIds.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var id in ids)
                {
                    if (int.TryParse(id.Trim(), out int fundId))
                    {
                        fundIds.Add(fundId);
                    }
                }
            }
            
            _logger.LogInformation("Parsed mutualFundIds: {FundIds}", string.Join(",", fundIds));
            
            // Allow empty fundIds for "All Funds" analysis
            // if (!fundIds.Any())
            // {
            //     _logger.LogWarning("ValidateDatesForFunds: No mutual fund IDs provided");
            //     return BadRequest(new { Message = "At least one mutual fund must be selected" });
            // }

            if (fromDate >= toDate)
            {
                _logger.LogWarning("ValidateDatesForFunds: From date {FromDate} is not before to date {ToDate}", fromDate, toDate);
                return BadRequest(new { Message = "From date must be before to date" });
            }

            var isValid = await _analyticsService.ValidateDatesForFundsAsync(fundIds, fromDate, toDate);
            _logger.LogInformation("ValidateDatesForFunds result: {IsValid} for funds {FundIds}", isValid, string.Join(",", fundIds));
            return Ok(new { IsValid = isValid });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating dates for funds {FundIds}", mutualFundIds ?? "null");
            return StatusCode(500, new { Message = ex.Message });
        }
    }

    [HttpGet("portfolio-holdings")]
    public async Task<IActionResult> GetPortfolioHoldings(
        [FromQuery] int mutualFundId,
        [FromQuery] DateTime targetDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "asc")
    {
        try
        {
            _logger.LogInformation("GetPortfolioHoldings called with fundId: {FundId}, targetDate: {TargetDate}, page: {Page}, pageSize: {PageSize}, sortBy: {SortBy}, sortDirection: {SortDirection}", 
                mutualFundId, targetDate, page, pageSize, sortBy ?? "default", sortDirection ?? "asc");

            if (mutualFundId <= 0)
            {
                return BadRequest(new { Message = "Valid mutual fund ID is required" });
            }

            if (targetDate == default)
            {
                return BadRequest(new { Message = "Valid target date is required" });
            }

            var request = new CurrentHoldingsRequestDto
            {
                MutualFundId = mutualFundId,
                TargetDate = targetDate,
                Page = page,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDirection = sortDirection
            };

            var result = await _analyticsService.GetCurrentHoldingsAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting portfolio holdings for fund {FundId} on date {Date}", 
                mutualFundId, targetDate);
            return StatusCode(500, new { Message = ex.Message });
        }
    }
}
