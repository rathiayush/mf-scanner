using System.ComponentModel.DataAnnotations;
using MutualFundScanner.Domain.Enums;

namespace MutualFundScanner.Domain.Entities;

/// <summary>
/// User entity for authentication and authorization
/// </summary>
public class User
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    [Required]
    public UserRole Role { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastLoginDate { get; set; }
    
    public int? CreatedByUserId { get; set; }
    
    public virtual User? CreatedBy { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
}
