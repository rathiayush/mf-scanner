using MutualFundScanner.Domain.Entities;

namespace MutualFundScanner.Infrastructure.Repositories;

public interface IConcreteMutualFundRepository
{
    Task<IEnumerable<ConcreteMutualFund>> GetAllAsync();
    Task<ConcreteMutualFund?> GetByIdAsync(int id);
    Task<(IEnumerable<ConcreteMutualFund> Items, int TotalCount)> SearchAsync(
        string? searchTerm = null,
        string? amcName = null,
        string? category = null,
        bool? isActive = null,
        int page = 1,
        int pageSize = 50,
        string sortBy = "FundName",
        bool sortDescending = false);
    Task<ConcreteMutualFund> CreateAsync(ConcreteMutualFund entity);
    Task<ConcreteMutualFund> UpdateAsync(ConcreteMutualFund entity);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> FundNameExistsAsync(string fundName, int? excludeId = null);
    Task<IEnumerable<ConcreteMutualFund>> GetActiveForDetectionAsync();
    Task<IEnumerable<string>> GetDistinctAMCNamesAsync();
    Task<IEnumerable<string>> GetDistinctCategoriesAsync();
    Task<IEnumerable<ConcreteMutualFund>> BulkCreateAsync(IEnumerable<ConcreteMutualFund> entities);
}
