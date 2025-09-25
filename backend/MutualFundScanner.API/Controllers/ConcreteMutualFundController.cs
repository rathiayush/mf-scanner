using Microsoft.AspNetCore.Mvc;
using MutualFundScanner.Application.DTOs;
using MutualFundScanner.Application.Interfaces;

namespace MutualFundScanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConcreteMutualFundController : ControllerBase
{
    private readonly IConcreteMutualFundService _service;
    private readonly ILogger<ConcreteMutualFundController> _logger;

    public ConcreteMutualFundController(
        IConcreteMutualFundService service,
        ILogger<ConcreteMutualFundController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Get all concrete mutual funds
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConcreteMutualFundDto>>> GetAll()
    {
        try
        {
            var funds = await _service.GetAllAsync();
            return Ok(funds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all concrete mutual funds");
            return StatusCode(500, "An error occurred while retrieving mutual funds");
        }
    }

    /// <summary>
    /// Search concrete mutual funds with pagination and filtering
    /// </summary>
    [HttpPost("search")]
    public async Task<ActionResult<object>> Search([FromBody] ConcreteMutualFundSearchDto searchDto)
    {
        try
        {
            var (items, totalCount) = await _service.SearchAsync(searchDto);
            
            return Ok(new
            {
                Items = items,
                TotalCount = totalCount,
                Page = searchDto.Page,
                PageSize = searchDto.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / searchDto.PageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching concrete mutual funds");
            return StatusCode(500, "An error occurred while searching mutual funds");
        }
    }

    /// <summary>
    /// Get concrete mutual fund by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ConcreteMutualFundDto>> GetById(int id)
    {
        try
        {
            var fund = await _service.GetByIdAsync(id);
            if (fund == null)
            {
                return NotFound($"Concrete mutual fund with ID {id} not found");
            }
            
            return Ok(fund);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving concrete mutual fund with ID {Id}", id);
            return StatusCode(500, "An error occurred while retrieving the mutual fund");
        }
    }

    /// <summary>
    /// Create a new concrete mutual fund
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ConcreteMutualFundDto>> Create([FromBody] CreateConcreteMutualFundDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdFund = await _service.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = createdFund.Id }, createdFund);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating concrete mutual fund");
            return StatusCode(500, "An error occurred while creating the mutual fund");
        }
    }

    /// <summary>
    /// Update an existing concrete mutual fund
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ConcreteMutualFundDto>> Update(int id, [FromBody] UpdateConcreteMutualFundDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedFund = await _service.UpdateAsync(id, updateDto);
            return Ok(updatedFund);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating concrete mutual fund with ID {Id}", id);
            return StatusCode(500, "An error occurred while updating the mutual fund");
        }
    }

    /// <summary>
    /// Delete a concrete mutual fund (hard delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var result = await _service.DeleteAsync(id);
            if (!result)
            {
                return NotFound($"Concrete mutual fund with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting concrete mutual fund with ID {Id}", id);
            return StatusCode(500, "An error occurred while deleting the mutual fund");
        }
    }

    /// <summary>
    /// Check if a concrete mutual fund exists
    /// </summary>
    [HttpHead("{id}")]
    public async Task<ActionResult> Exists(int id)
    {
        try
        {
            var exists = await _service.ExistsAsync(id);
            return exists ? Ok() : NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence of concrete mutual fund with ID {Id}", id);
            return StatusCode(500, "An error occurred while checking the mutual fund");
        }
    }

    /// <summary>
    /// Get distinct AMC names
    /// </summary>
    [HttpGet("amc-names")]
    public async Task<ActionResult<IEnumerable<string>>> GetDistinctAMCNames()
    {
        try
        {
            var amcNames = await _service.GetDistinctAMCNamesAsync();
            return Ok(amcNames);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving distinct AMC names");
            return StatusCode(500, "An error occurred while retrieving AMC names");
        }
    }

    /// <summary>
    /// Get distinct categories
    /// </summary>
    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<string>>> GetDistinctCategories()
    {
        try
        {
            var categories = await _service.GetDistinctCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving distinct categories");
            return StatusCode(500, "An error occurred while retrieving categories");
        }
    }

    /// <summary>
    /// Bulk create concrete mutual funds
    /// </summary>
    [HttpPost("bulk")]
    public async Task<ActionResult<object>> BulkCreate([FromBody] IEnumerable<CreateConcreteMutualFundDto> createDtos)
    {
        try
        {
            if (!createDtos.Any())
            {
                return BadRequest("No mutual funds provided for bulk creation");
            }

            var createdCount = await _service.BulkCreateAsync(createDtos);
            
            return Ok(new
            {
                Message = $"Successfully created {createdCount} concrete mutual funds",
                CreatedCount = createdCount,
                TotalProvided = createDtos.Count()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk creating concrete mutual funds");
            return StatusCode(500, "An error occurred while bulk creating mutual funds");
        }
    }
}
