using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MutualFundScanner.Application.DTOs;
using MutualFundScanner.Application.Services;

namespace MutualFundScanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UserManagementController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<UserManagementController> _logger;

    public UserManagementController(IAuthService authService, ILogger<UserManagementController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Get all users (Admin only)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<UsersListResponseDto>> GetAllUsers([FromQuery] bool includeInactive = false)
    {
        try
        {
            _logger.LogInformation("Admin user {UserId} requesting all users list", GetCurrentUserId());
            var response = await _authService.GetAllUsersAsync(includeInactive);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users list");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Get user by ID (Admin only)
    /// </summary>
    [HttpGet("{userId}")]
    public async Task<ActionResult<UserDto>> GetUserById(int userId)
    {
        try
        {
            var user = await _authService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID: {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Create new user (Admin only)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserRequestDto request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Admin user {UserId} creating new user: {Username}", currentUserId, request.Username);
            
            var user = await _authService.CreateUserAsync(request, currentUserId);
            return CreatedAtAction(nameof(GetUserById), new { userId = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Update user (Admin only)
    /// </summary>
    [HttpPut("{userId}")]
    public async Task<ActionResult<UserDto>> UpdateUser(int userId, [FromBody] UpdateUserRequestDto request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Admin user {UserId} updating user: {TargetUserId}", currentUserId, userId);
            
            var user = await _authService.UpdateUserAsync(userId, request, currentUserId);
            return Ok(user);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Delete user (Admin only)
    /// </summary>
    [HttpDelete("{userId}")]
    public async Task<ActionResult> DeleteUser(int userId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Admin user {UserId} deleting user: {TargetUserId}", currentUserId, userId);
            
            await _authService.DeleteUserAsync(userId, currentUserId);
            return Ok(new { message = "User deleted successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Deactivate user (Admin only)
    /// </summary>
    [HttpPost("{userId}/deactivate")]
    public async Task<ActionResult> DeactivateUser(int userId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Admin user {UserId} deactivating user: {TargetUserId}", currentUserId, userId);
            
            await _authService.DeactivateUserAsync(userId, currentUserId);
            return Ok(new { message = "User deactivated successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user: {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Activate user (Admin only)
    /// </summary>
    [HttpPost("{userId}/activate")]
    public async Task<ActionResult> ActivateUser(int userId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Admin user {UserId} activating user: {TargetUserId}", currentUserId, userId);
            
            await _authService.ActivateUserAsync(userId, currentUserId);
            return Ok(new { message = "User activated successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating user: {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Reset user password (Admin only)
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Admin user {UserId} resetting password for user: {TargetUserId}", currentUserId, request.UserId);
            
            await _authService.ResetPasswordAsync(request, currentUserId);
            return Ok(new { message = "Password reset successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    #region Helper Methods

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user token");
        }
        return userId;
    }

    #endregion
}
