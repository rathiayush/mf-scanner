using MutualFundScanner.Application.DTOs;

namespace MutualFundScanner.Application.Interfaces;

public interface IConcreteMutualFundService
{
    Task<IEnumerable<ConcreteMutualFundDto>> GetAllAsync();
    Task<ConcreteMutualFundDto?> GetByIdAsync(int id);
    Task<(IEnumerable<ConcreteMutualFundListDto> Items, int TotalCount)> SearchAsync(ConcreteMutualFundSearchDto searchDto);
    Task<ConcreteMutualFundDto> CreateAsync(CreateConcreteMutualFundDto createDto);
    Task<ConcreteMutualFundDto> UpdateAsync(int id, UpdateConcreteMutualFundDto updateDto);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<IEnumerable<string>> GetDistinctAMCNamesAsync();
    Task<IEnumerable<string>> GetDistinctCategoriesAsync();
    Task<int> BulkCreateAsync(IEnumerable<CreateConcreteMutualFundDto> createDtos);
}
