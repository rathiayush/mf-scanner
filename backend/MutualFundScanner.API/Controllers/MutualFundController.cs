using Microsoft.AspNetCore.Mvc;
using MutualFundScanner.Application.Interfaces;
using MutualFundScanner.Application.DTOs;

namespace MutualFundScanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MutualFundController : ControllerBase
{
    private readonly IMutualFundService _mutualFundService;
    private readonly ILogger<MutualFundController> _logger;

    public MutualFundController(
        IMutualFundService mutualFundService,
        ILogger<MutualFundController> logger)
    {
        _mutualFundService = mutualFundService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var funds = await _mutualFundService.GetAllAsync();
        return Ok(funds.Select(f => new
        {
            f.Id,
            f.FundCode,
            f.FundName,
            f.AMC,
            f.Category,
            f.SubCategory,
            f.LaunchDate,
            f.NAV,
            f.AUM,
            f.ExpenseRatio,
            f.Status,
            f.IsActive,
            f.CreatedDate,
            f.LastUpdated,
            PortfolioCount = f.Files.Count,
            LatestPortfolioDate = f.Files.OrderByDescending(pf => pf.ReportDate).FirstOrDefault()?.ReportDate
        }));
    }

    [HttpGet("{fundId}/analytics")]
    public async Task<IActionResult> GetMonthlyAnalytics(int fundId, [FromQuery] int fromSnapshotId, [FromQuery] int toSnapshotId)
    {
        try
        {
            var result = await _mutualFundService.GetMonthlyAnalyticsAsync(fundId, fromSnapshotId, toSnapshotId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting monthly analytics for fund {FundId}", fundId);
            return StatusCode(500, new { Message = ex.Message });
        }
    }

    // New CRUD endpoints
    [HttpPost]
    public async Task<IActionResult> CreateMutualFund([FromBody] CreateMutualFundDto createDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(createDto.FundName))
            {
                return BadRequest(new { Message = "Fund name is required" });
            }

            var result = await _mutualFundService.CreateMutualFundAsync(createDto);
            return CreatedAtAction(nameof(GetMutualFundById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating mutual fund");
            return StatusCode(500, new { Message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMutualFund(int id, [FromBody] UpdateMutualFundDto updateDto)
    {
        try
        {
            if (id != updateDto.Id)
            {
                return BadRequest(new { Message = "ID mismatch" });
            }

            if (string.IsNullOrWhiteSpace(updateDto.FundName))
            {
                return BadRequest(new { Message = "Fund name is required" });
            }

            var result = await _mutualFundService.UpdateMutualFundAsync(updateDto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating mutual fund {Id}", id);
            return StatusCode(500, new { Message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMutualFund(int id)
    {
        try
        {
            var result = await _mutualFundService.DeleteMutualFundAsync(id);
            if (!result)
            {
                return NotFound(new { Message = "Mutual fund not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting mutual fund {Id}", id);
            return StatusCode(500, new { Message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMutualFundById(int id)
    {
        try
        {
            var result = await _mutualFundService.GetMutualFundByIdAsync(id);
            if (result == null)
            {
                return NotFound(new { Message = "Mutual fund not found" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mutual fund {Id}", id);
            return StatusCode(500, new { Message = ex.Message });
        }
    }

    // Search and dropdown endpoints
    [HttpPost("search")]
    public async Task<IActionResult> SearchMutualFunds([FromBody] MutualFundSearchDto searchDto)
    {
        try
        {
            var result = await _mutualFundService.SearchMutualFundsAsync(searchDto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching mutual funds");
            return StatusCode(500, new { Message = ex.Message });
        }
    }

    [HttpGet("dropdown")]
    public async Task<IActionResult> GetMutualFundsForDropdown([FromQuery] string? searchTerm = null)
    {
        try
        {
            var result = await _mutualFundService.GetMutualFundsForDropdownAsync(searchTerm);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mutual funds for dropdown");
            return StatusCode(500, new { Message = ex.Message });
        }
    }
}
