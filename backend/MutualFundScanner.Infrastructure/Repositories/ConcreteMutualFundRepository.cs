using Microsoft.EntityFrameworkCore;
using MutualFundScanner.Domain.Entities;
using MutualFundScanner.Infrastructure.Data;

namespace MutualFundScanner.Infrastructure.Repositories;

public class ConcreteMutualFundRepository : IConcreteMutualFundRepository
{
    private readonly MutualFundContext _context;

    public ConcreteMutualFundRepository(MutualFundContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ConcreteMutualFund>> GetAllAsync()
    {
        return await _context.ConcreteMutualFunds
            .OrderBy(x => x.AMCName)
            .ThenBy(x => x.FundName)
            .ToListAsync();
    }

    public async Task<ConcreteMutualFund?> GetByIdAsync(int id)
    {
        return await _context.ConcreteMutualFunds
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<(IEnumerable<ConcreteMutualFund> Items, int TotalCount)> SearchAsync(
        string? searchTerm = null,
        string? amcName = null,
        string? category = null,
        bool? isActive = null,
        int page = 1,
        int pageSize = 50,
        string sortBy = "FundName",
        bool sortDescending = false)
    {
        var query = _context.ConcreteMutualFunds.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(x => x.FundName.Contains(searchTerm) ||
                                   (x.AMCName != null && x.AMCName.Contains(searchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(amcName))
        {
            query = query.Where(x => x.AMCName == amcName);
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(x => x.Category == category);
        }

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = sortBy.ToLower() switch
        {
            "amcname" => sortDescending ? query.OrderByDescending(x => x.AMCName) : query.OrderBy(x => x.AMCName),
            "category" => sortDescending ? query.OrderByDescending(x => x.Category) : query.OrderBy(x => x.Category),
            "isactive" => sortDescending ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
            "matchpriority" => sortDescending ? query.OrderByDescending(x => x.MatchPriority) : query.OrderBy(x => x.MatchPriority),
            "createddate" => sortDescending ? query.OrderByDescending(x => x.CreatedDate) : query.OrderBy(x => x.CreatedDate),
            _ => sortDescending ? query.OrderByDescending(x => x.FundName) : query.OrderBy(x => x.FundName)
        };

        // Apply pagination
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<ConcreteMutualFund> CreateAsync(ConcreteMutualFund entity)
    {
        entity.CreatedDate = DateTime.UtcNow;
        _context.ConcreteMutualFunds.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<ConcreteMutualFund> UpdateAsync(ConcreteMutualFund entity)
    {
        entity.LastUpdated = DateTime.UtcNow;
        _context.ConcreteMutualFunds.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.ConcreteMutualFunds.FindAsync(id);
        if (entity == null)
            return false;

        _context.ConcreteMutualFunds.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.ConcreteMutualFunds
            .AnyAsync(x => x.Id == id);
    }

    public async Task<bool> FundNameExistsAsync(string fundName, int? excludeId = null)
    {
        var query = _context.ConcreteMutualFunds
            .Where(x => x.FundName.ToLower() == fundName.ToLower());

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<IEnumerable<ConcreteMutualFund>> GetActiveForDetectionAsync()
    {
        return await _context.ConcreteMutualFunds
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.MatchPriority)
            .ThenBy(x => x.FundName)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetDistinctAMCNamesAsync()
    {
        return await _context.ConcreteMutualFunds
            .Where(x => !string.IsNullOrEmpty(x.AMCName))
            .Select(x => x.AMCName!)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetDistinctCategoriesAsync()
    {
        return await _context.ConcreteMutualFunds
            .Where(x => !string.IsNullOrEmpty(x.Category))
            .Select(x => x.Category!)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();
    }

    public async Task<IEnumerable<ConcreteMutualFund>> BulkCreateAsync(IEnumerable<ConcreteMutualFund> entities)
    {
        var entityList = entities.ToList();
        await _context.ConcreteMutualFunds.AddRangeAsync(entityList);
        await _context.SaveChangesAsync();
        return entityList;
    }
}
