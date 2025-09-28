using Microsoft.Extensions.Logging;
using MutualFundScanner.Application.DTOs;
using MutualFundScanner.Application.Interfaces;
using MutualFundScanner.Domain.Entities;
using MutualFundScanner.Infrastructure.Repositories;

namespace MutualFundScanner.Application.Services;

public class ConcreteMutualFundService : IConcreteMutualFundService
{
    private readonly IConcreteMutualFundRepository _repository;
    private readonly ILogger<ConcreteMutualFundService> _logger;

    public ConcreteMutualFundService(
        IConcreteMutualFundRepository repository,
        ILogger<ConcreteMutualFundService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<ConcreteMutualFundDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<ConcreteMutualFundDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<(IEnumerable<ConcreteMutualFundListDto> Items, int TotalCount)> SearchAsync(ConcreteMutualFundSearchDto searchDto)
    {
        var (items, totalCount) = await _repository.SearchAsync(
            searchDto.SearchTerm,
            searchDto.AMCName,
            searchDto.Category,
            searchDto.IsActive,
            searchDto.Page,
            searchDto.PageSize,
            searchDto.SortBy,
            searchDto.SortDescending);

        var mappedItems = items.Select(x => new ConcreteMutualFundListDto
        {
            Id = x.Id,
            FundName = x.FundName,
            AMCName = x.AMCName,
            Category = x.Category,
            IsActive = x.IsActive,
            MatchPriority = x.MatchPriority,
            CreatedDate = x.CreatedDate
        });

        return (mappedItems, totalCount);
    }

    public async Task<ConcreteMutualFundDto> CreateAsync(CreateConcreteMutualFundDto createDto)
    {
        // Check if fund name already exists
        if (await _repository.FundNameExistsAsync(createDto.FundName))
        {
            throw new InvalidOperationException($"A mutual fund with the name '{createDto.FundName}' already exists.");
        }

        var entity = new ConcreteMutualFund
        {
            FundName = createDto.FundName.Trim(),
            AMCName = createDto.AMCName?.Trim(),
            Category = createDto.Category?.Trim(),
            SubCategory = createDto.SubCategory?.Trim(),
            IsActive = createDto.IsActive,
            Notes = createDto.Notes?.Trim(),
            MatchPriority = createDto.MatchPriority
        };

        var createdEntity = await _repository.CreateAsync(entity);
        _logger.LogInformation("Created concrete mutual fund: {FundName} (ID: {Id})", createdEntity.FundName, createdEntity.Id);
        
        return MapToDto(createdEntity);
    }

    public async Task<ConcreteMutualFundDto> UpdateAsync(int id, UpdateConcreteMutualFundDto updateDto)
    {
        var existingEntity = await _repository.GetByIdAsync(id);
        if (existingEntity == null)
        {
            throw new InvalidOperationException($"Concrete mutual fund with ID {id} not found.");
        }

        // Check if fund name already exists (excluding current entity)
        if (await _repository.FundNameExistsAsync(updateDto.FundName, id))
        {
            throw new InvalidOperationException($"A mutual fund with the name '{updateDto.FundName}' already exists.");
        }

        existingEntity.FundName = updateDto.FundName.Trim();
        existingEntity.AMCName = updateDto.AMCName?.Trim();
        existingEntity.Category = updateDto.Category?.Trim();
        existingEntity.SubCategory = updateDto.SubCategory?.Trim();
        existingEntity.IsActive = updateDto.IsActive;
        existingEntity.Notes = updateDto.Notes?.Trim();
        existingEntity.MatchPriority = updateDto.MatchPriority;

        var updatedEntity = await _repository.UpdateAsync(existingEntity);
        _logger.LogInformation("Updated concrete mutual fund: {FundName} (ID: {Id})", updatedEntity.FundName, updatedEntity.Id);
        
        return MapToDto(updatedEntity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
        {
            return false;
        }

        var result = await _repository.DeleteAsync(id);
        if (result)
        {
            _logger.LogInformation("Deleted concrete mutual fund: {FundName} (ID: {Id})", entity.FundName, entity.Id);
        }
        
        return result;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _repository.ExistsAsync(id);
    }

    public async Task<IEnumerable<string>> GetDistinctAMCNamesAsync()
    {
        return await _repository.GetDistinctAMCNamesAsync();
    }

    public async Task<IEnumerable<string>> GetDistinctCategoriesAsync()
    {
        return await _repository.GetDistinctCategoriesAsync();
    }

    public async Task<int> BulkCreateAsync(IEnumerable<CreateConcreteMutualFundDto> createDtos)
    {
        var createdCount = 0;
        
        foreach (var createDto in createDtos)
        {
            try
            {
                // Skip if fund name already exists
                if (await _repository.FundNameExistsAsync(createDto.FundName))
                {
                    _logger.LogWarning("Skipping duplicate fund name: {FundName}", createDto.FundName);
                    continue;
                }

                var entity = new ConcreteMutualFund
                {
                    FundName = createDto.FundName.Trim(),
                    AMCName = createDto.AMCName?.Trim(),
                    Category = createDto.Category?.Trim(),
                    SubCategory = createDto.SubCategory?.Trim(),
                    IsActive = createDto.IsActive,
                    Notes = createDto.Notes?.Trim(),
                    MatchPriority = createDto.MatchPriority
                };

                await _repository.CreateAsync(entity);
                createdCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating concrete mutual fund: {FundName}", createDto.FundName);
            }
        }

        _logger.LogInformation("Bulk created {Count} concrete mutual funds", createdCount);
        return createdCount;
    }

    private static ConcreteMutualFundDto MapToDto(ConcreteMutualFund entity)
    {
        return new ConcreteMutualFundDto
        {
            Id = entity.Id,
            FundName = entity.FundName,
            AMCName = entity.AMCName,
            Category = entity.Category,
            SubCategory = entity.SubCategory,
            IsActive = entity.IsActive,
            CreatedDate = entity.CreatedDate,
            LastUpdated = entity.LastUpdated,
            Notes = entity.Notes,
            MatchPriority = entity.MatchPriority
        };
    }
}
