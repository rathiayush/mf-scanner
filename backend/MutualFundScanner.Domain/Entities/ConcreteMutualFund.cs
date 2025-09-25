using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MutualFundScanner.Domain.Entities;

/// <summary>
/// Represents concrete mutual fund patterns used for fund detection
/// </summary>
public class ConcreteMutualFund
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string FundName { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? AMCName { get; set; }
    
    [MaxLength(100)]
    public string? Category { get; set; }
    
    [MaxLength(100)]
    public string? SubCategory { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastUpdated { get; set; }
    
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Priority for matching - higher numbers get matched first
    /// </summary>
    public int MatchPriority { get; set; } = 100;
}
