using System.ComponentModel.DataAnnotations;

namespace MutualFundScanner.Application.DTOs;

public class ConcreteMutualFundDto
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
    
    public DateTime CreatedDate { get; set; }
    
    public DateTime? LastUpdated { get; set; }
    
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    public int MatchPriority { get; set; } = 100;
}

public class CreateConcreteMutualFundDto
{
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
    
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    public int MatchPriority { get; set; } = 100;
}

public class UpdateConcreteMutualFundDto
{
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
    
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    public int MatchPriority { get; set; } = 100;
}

public class ConcreteMutualFundListDto
{
    public int Id { get; set; }
    public string FundName { get; set; } = string.Empty;
    public string? AMCName { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; }
    public int MatchPriority { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class ConcreteMutualFundSearchDto
{
    public string? SearchTerm { get; set; }
    public string? AMCName { get; set; }
    public string? Category { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string SortBy { get; set; } = "FundName";
    public bool SortDescending { get; set; } = false;
}
