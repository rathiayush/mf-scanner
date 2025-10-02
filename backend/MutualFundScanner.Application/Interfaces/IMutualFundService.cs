using MutualFundScanner.Domain.Entities;
using MutualFundScanner.Application.DTOs;

namespace MutualFundScanner.Application.Interfaces;

public interface IMutualFundService
{
    // Existing methods
    Task<MutualFund?> GetByCodeAsync(string fundCode);
    Task<MutualFund?> GetByIdAsync(int fundId);
    Task<List<MutualFund>> GetAllAsync();
    Task<MutualFund> CreateAsync(string fundCode, string fundName, string folderPath);
    Task<MutualFund> UpdateAsync(MutualFund fund);
    Task<bool> ExistsAsync(string fundCode);
    Task<MonthlyAnalyticsResult> GetMonthlyAnalyticsAsync(int fundId, int fromSnapshotId, int toSnapshotId);
    
    // New CRUD methods
    Task<MutualFundDto> CreateMutualFundAsync(CreateMutualFundDto createDto);
    Task<MutualFundDto> UpdateMutualFundAsync(UpdateMutualFundDto updateDto);
    Task<bool> DeleteMutualFundAsync(int id);
    Task<MutualFundDto?> GetMutualFundByIdAsync(int id);
    
    // New search and pagination methods
    Task<PagedResult<MutualFundDto>> SearchMutualFundsAsync(MutualFundSearchDto searchDto);
    Task<List<MutualFundDto>> GetMutualFundsForDropdownAsync(string? searchTerm = null);
    
    // Hard delete methods
    Task<DeleteMutualFundsResponseDto> HardDeleteMutualFundsAsync(DeleteMutualFundsRequestDto request);
}

public class MonthlyAnalyticsResult
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<HoldingChangeAnalytics> Changes { get; set; } = new();
}

public class HoldingChangeAnalytics
{
    public string ISIN { get; set; } = string.Empty;
    public string SecurityName { get; set; } = string.Empty;
    public decimal? PreviousWeight { get; set; }
    public decimal CurrentWeight { get; set; }
    public decimal WeightChange { get; set; }
    public bool IsNew { get; set; }
    public bool IsRemoved { get; set; }
}
