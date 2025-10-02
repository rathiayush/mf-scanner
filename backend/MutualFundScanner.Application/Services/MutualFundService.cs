using MutualFundScanner.Application.Interfaces;
using MutualFundScanner.Application.DTOs;
using MutualFundScanner.Domain.Entities;
using MutualFundScanner.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MutualFundScanner.Application.Services;

public class MutualFundService : IMutualFundService
{
    private readonly MutualFundContext _context;
    private readonly ILogger<MutualFundService> _logger;

    public MutualFundService(MutualFundContext context, ILogger<MutualFundService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<MutualFund?> GetByCodeAsync(string fundCode)
    {
        return await _context.MutualFunds
            .Include(f => f.Files)
            .Include(f => f.Snapshots)
            .FirstOrDefaultAsync(f => f.FundCode == fundCode);
    }

    public async Task<MutualFund?> GetByIdAsync(int fundId)
    {
        return await _context.MutualFunds
            .Include(f => f.Files)
            .Include(f => f.Snapshots)
            .ThenInclude(s => s.Holdings)
            .FirstOrDefaultAsync(f => f.Id == fundId);
    }

    public async Task<List<MutualFund>> GetAllAsync()
    {
        return await _context.MutualFunds
            .Include(f => f.Files)
            .OrderBy(f => f.FundName)
            .ToListAsync();
    }

    public async Task<MutualFund> CreateAsync(string fundCode, string fundName, string folderPath)
    {
        var fund = new MutualFund
        {
            FundCode = fundCode,
            FundName = fundName,
            FolderPath = folderPath,
            CreatedDate = DateTime.UtcNow
        };

        _context.MutualFunds.Add(fund);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created new mutual fund: {FundCode} - {FundName}", fundCode, fundName);

        return fund;
    }

    public async Task<MutualFund> UpdateAsync(MutualFund fund)
    {
        _context.MutualFunds.Update(fund);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated mutual fund: {FundCode}", fund.FundCode);

        return fund;
    }

    public async Task<bool> ExistsAsync(string fundCode)
    {
        return await _context.MutualFunds
            .AnyAsync(f => f.FundCode == fundCode);
    }

    public async Task<MonthlyAnalyticsResult> GetMonthlyAnalyticsAsync(int fundId, int fromSnapshotId, int toSnapshotId)
    {
        var fund = await _context.MutualFunds
            .Include(f => f.Snapshots)
            .ThenInclude(s => s.Holdings)
            .FirstOrDefaultAsync(f => f.Id == fundId);
        if (fund == null)
            throw new Exception("Mutual fund not found");
        var fromSnapshot = fund.Snapshots.FirstOrDefault(s => s.Id == fromSnapshotId);
        var toSnapshot = fund.Snapshots.FirstOrDefault(s => s.Id == toSnapshotId);
        if (fromSnapshot == null || toSnapshot == null)
            throw new Exception("Snapshot(s) not found");
        var fromHoldings = CreateHoldingDictionaryByISIN(fromSnapshot.Holdings);
        var toHoldings = CreateHoldingDictionaryByISIN(toSnapshot.Holdings);
        var changes = new List<HoldingChangeAnalytics>();
        foreach (var kvp in toHoldings)
        {
            var isin = kvp.Key;
            var to = kvp.Value;
            fromHoldings.TryGetValue(isin, out var from);
            changes.Add(new HoldingChangeAnalytics
            {
                ISIN = isin,
                SecurityName = to.SecurityName ?? string.Empty,
                PreviousWeight = from?.Weight,
                CurrentWeight = to.Weight,
                WeightChange = to.Weight - (from?.Weight ?? 0),
                IsNew = from == null,
                IsRemoved = false
            });
        }
        foreach (var kvp in fromHoldings)
        {
            if (!toHoldings.ContainsKey(kvp.Key))
            {
                var from = kvp.Value;
                changes.Add(new HoldingChangeAnalytics
                {
                    ISIN = kvp.Key,
                    SecurityName = from.SecurityName ?? string.Empty,
                    PreviousWeight = from.Weight,
                    CurrentWeight = 0,
                    WeightChange = -from.Weight,
                    IsNew = false,
                    IsRemoved = true
                });
            }
        }
        return new MonthlyAnalyticsResult
        {
            FromDate = fromSnapshot.SnapshotDate,
            ToDate = toSnapshot.SnapshotDate,
            Changes = changes.OrderByDescending(c => Math.Abs(c.WeightChange)).ToList()
        };
    }

    // New CRUD implementations
    public async Task<MutualFundDto> CreateMutualFundAsync(CreateMutualFundDto createDto)
    {
        var fund = new MutualFund
        {
            FundName = createDto.FundName,
            FundCode = createDto.FundCode ?? string.Empty,
            AMC = createDto.AMC ?? string.Empty,
            Category = createDto.Category ?? string.Empty,
            SubCategory = createDto.SubCategory ?? string.Empty,
            LaunchDate = createDto.LaunchDate ?? DateTime.MinValue,
            NAV = createDto.NAV ?? 0,
            AUM = createDto.AUM ?? 0,
            ExpenseRatio = createDto.ExpenseRatio ?? 0,
            CreatedDate = DateTime.UtcNow,
            IsActive = true
        };

        _context.MutualFunds.Add(fund);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created new mutual fund: {FundName}", fund.FundName);

        return MapToDto(fund);
    }

    public async Task<MutualFundDto> UpdateMutualFundAsync(UpdateMutualFundDto updateDto)
    {
        var fund = await _context.MutualFunds.FindAsync(updateDto.Id);
        if (fund == null)
            throw new ArgumentException($"Mutual fund with ID {updateDto.Id} not found");

        fund.FundName = updateDto.FundName;
        fund.FundCode = updateDto.FundCode ?? fund.FundCode;
        fund.AMC = updateDto.AMC ?? fund.AMC;
        fund.Category = updateDto.Category ?? fund.Category;
        fund.SubCategory = updateDto.SubCategory ?? fund.SubCategory;
        fund.LaunchDate = updateDto.LaunchDate ?? fund.LaunchDate;
        fund.NAV = updateDto.NAV ?? fund.NAV;
        fund.AUM = updateDto.AUM ?? fund.AUM;
        fund.ExpenseRatio = updateDto.ExpenseRatio ?? fund.ExpenseRatio;
        fund.Status = updateDto.Status;
        fund.IsActive = updateDto.IsActive;
        fund.LastUpdated = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated mutual fund: {FundName}", fund.FundName);

        return MapToDto(fund);
    }

    public async Task<bool> DeleteMutualFundAsync(int id)
    {
        var fund = await _context.MutualFunds.FindAsync(id);
        if (fund == null)
            return false;

        // Soft delete - just mark as inactive
        fund.IsActive = false;
        fund.LastUpdated = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted mutual fund: {FundName}", fund.FundName);

        return true;
    }

    public async Task<MutualFundDto?> GetMutualFundByIdAsync(int id)
    {
        var fund = await _context.MutualFunds
            .Include(f => f.Files)
            .FirstOrDefaultAsync(f => f.Id == id);

        return fund != null ? MapToDto(fund) : null;
    }

    public async Task<PagedResult<MutualFundDto>> SearchMutualFundsAsync(MutualFundSearchDto searchDto)
    {
        var query = _context.MutualFunds
            .Include(f => f.Files)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(searchDto.SearchTerm))
        {
            var searchTerm = searchDto.SearchTerm.ToLower();
            query = query.Where(f => 
                f.FundName.ToLower().Contains(searchTerm) ||
                f.FundCode.ToLower().Contains(searchTerm) ||
                f.AMC.ToLower().Contains(searchTerm));
        }

        if (!string.IsNullOrEmpty(searchDto.Category))
        {
            query = query.Where(f => f.Category == searchDto.Category);
        }

        if (!string.IsNullOrEmpty(searchDto.AMC))
        {
            query = query.Where(f => f.AMC == searchDto.AMC);
        }

        if (searchDto.IsActive.HasValue)
        {
            query = query.Where(f => f.IsActive == searchDto.IsActive.Value);
        }

        var totalCount = await query.CountAsync();

        var funds = await query
            .OrderBy(f => f.FundName)
            .Skip((searchDto.Page - 1) * searchDto.PageSize)
            .Take(searchDto.PageSize)
            .ToListAsync();

        return new PagedResult<MutualFundDto>
        {
            Data = funds.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = searchDto.Page,
            PageSize = searchDto.PageSize
        };
    }

    public async Task<List<MutualFundDto>> GetMutualFundsForDropdownAsync(string? searchTerm = null)
    {
        var query = _context.MutualFunds
            .Where(f => f.IsActive)
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var search = searchTerm.ToLower();
            query = query.Where(f => 
                f.FundName.ToLower().Contains(search) ||
                f.FundCode.ToLower().Contains(search));
        }

        var funds = await query
            .OrderBy(f => f.FundName)
            .Take(50) // Limit for dropdown
            .ToListAsync();

        return funds.Select(f => new MutualFundDto
        {
            Id = f.Id,
            FundCode = f.FundCode,
            FundName = f.FundName,
            AMC = f.AMC,
            Category = f.Category,
            IsActive = f.IsActive
        }).ToList();
    }

    public async Task<DeleteMutualFundsResponseDto> HardDeleteMutualFundsAsync(DeleteMutualFundsRequestDto request)
    {
        var response = new DeleteMutualFundsResponseDto();
        
        if (!request.ConfirmDeletion)
        {
            response.Success = false;
            response.Message = "Deletion confirmation is required.";
            return response;
        }

        if (request.MutualFundIds == null || !request.MutualFundIds.Any())
        {
            response.Success = false;
            response.Message = "No mutual fund IDs provided for deletion.";
            return response;
        }

        try
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            // First, get fund information for logging and response
            var fundIdsString = string.Join(",", request.MutualFundIds);
            var fundsQuery = $"SELECT Id, FundName, FundCode FROM MutualFunds WHERE Id IN ({fundIdsString})";
            var funds = await _context.Database.SqlQueryRaw<MutualFundInfo>(fundsQuery).ToListAsync();
            
            var foundFundIds = funds.Select(f => f.Id).ToHashSet();
            var missingFundIds = request.MutualFundIds.Except(foundFundIds).ToList();
            
            foreach (var missingFundId in missingFundIds)
            {
                response.Errors.Add($"Mutual fund with ID {missingFundId} not found.");
            }
            
            if (!foundFundIds.Any())
            {
                response.Success = false;
                response.Message = "No valid mutual funds found for deletion.";
                return response;
            }
            
            var validFundIdsString = string.Join(",", foundFundIds);
            
            // Execute deletion in the correct order to respect foreign key constraints
            
            // 1. Delete holdings from portfolio files
            var deletePortfolioFileHoldingsSql = $"DELETE FROM PortfolioHoldings WHERE PortfolioFileId IN (SELECT Id FROM PortfolioFiles WHERE MutualFundId IN ({validFundIdsString}))";
            var portfolioFileHoldingsDeleted = await _context.Database.ExecuteSqlRawAsync(deletePortfolioFileHoldingsSql);
            
            // 2. Delete holdings from portfolio snapshots
            var deletePortfolioSnapshotHoldingsSql = $"DELETE FROM PortfolioHoldings WHERE PortfolioSnapshotId IN (SELECT Id FROM PortfolioSnapshots WHERE MutualFundId IN ({validFundIdsString}))";
            var portfolioSnapshotHoldingsDeleted = await _context.Database.ExecuteSqlRawAsync(deletePortfolioSnapshotHoldingsSql);
            
            // 3. Delete holding changes
            var deleteHoldingChangesSql = $"DELETE FROM HoldingChanges WHERE MutualFundId IN ({validFundIdsString})";
            var holdingChangesDeleted = await _context.Database.ExecuteSqlRawAsync(deleteHoldingChangesSql);
            
            // 4. Delete portfolio files
            var deletePortfolioFilesSql = $"DELETE FROM PortfolioFiles WHERE MutualFundId IN ({validFundIdsString})";
            var filesDeleted = await _context.Database.ExecuteSqlRawAsync(deletePortfolioFilesSql);
            
            // 5. Delete portfolio snapshots
            var deletePortfolioSnapshotsSql = $"DELETE FROM PortfolioSnapshots WHERE MutualFundId IN ({validFundIdsString})";
            var snapshotsDeleted = await _context.Database.ExecuteSqlRawAsync(deletePortfolioSnapshotsSql);
            
            // 6. Delete the mutual funds themselves
            var deleteMutualFundsSql = $"DELETE FROM MutualFunds WHERE Id IN ({validFundIdsString})";
            var mutualFundsDeleted = await _context.Database.ExecuteSqlRawAsync(deleteMutualFundsSql);
            
            await transaction.CommitAsync();
            
            // Build response with deleted fund information
            foreach (var fund in funds)
            {
                var deletedFundInfo = new DeletedMutualFundInfo
                {
                    Id = fund.Id,
                    FundName = fund.FundName,
                    FundCode = fund.FundCode,
                    HoldingsDeleted = 0, // We don't have per-fund breakdown with this approach
                    HoldingChangesDeleted = 0,
                    FilesDeleted = 0,
                    SnapshotsDeleted = 0
                };
                
                response.DeletedFunds.Add(deletedFundInfo);
                
                _logger.LogInformation("Hard deleted mutual fund: {FundName} (ID: {Id})", fund.FundName, fund.Id);
            }
            
            // Set summary counts
            if (response.DeletedFunds.Any())
            {
                var firstFund = response.DeletedFunds.First();
                firstFund.HoldingsDeleted = portfolioFileHoldingsDeleted + portfolioSnapshotHoldingsDeleted;
                firstFund.HoldingChangesDeleted = holdingChangesDeleted;
                firstFund.FilesDeleted = filesDeleted;
                firstFund.SnapshotsDeleted = snapshotsDeleted;
            }
            
            response.Success = mutualFundsDeleted > 0;
            response.Message = response.Success 
                ? $"Successfully deleted {mutualFundsDeleted} mutual fund(s). Deleted {portfolioFileHoldingsDeleted + portfolioSnapshotHoldingsDeleted} holdings, {holdingChangesDeleted} holding changes, {filesDeleted} files, and {snapshotsDeleted} snapshots." 
                : "No mutual funds were deleted.";
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = "An error occurred during the deletion process.";
            response.Errors.Add(ex.Message);
            _logger.LogError(ex, "Error in HardDeleteMutualFundsAsync");
        }

        return response;
    }
    
    // Helper class for SQL query results
    private class MutualFundInfo
    {
        public int Id { get; set; }
        public string FundName { get; set; } = string.Empty;
        public string FundCode { get; set; } = string.Empty;
    }

    private MutualFundDto MapToDto(MutualFund fund)
    {
        return new MutualFundDto
        {
            Id = fund.Id,
            FundCode = fund.FundCode,
            FundName = fund.FundName,
            AMC = fund.AMC,
            Category = fund.Category,
            SubCategory = fund.SubCategory,
            LaunchDate = fund.LaunchDate == DateTime.MinValue ? null : fund.LaunchDate,
            NAV = fund.NAV == 0 ? null : fund.NAV,
            AUM = fund.AUM == 0 ? null : fund.AUM,
            ExpenseRatio = fund.ExpenseRatio == 0 ? null : fund.ExpenseRatio,
            Status = fund.Status,
            IsActive = fund.IsActive,
            CreatedDate = fund.CreatedDate,
            LastUpdated = fund.LastUpdated,
            PortfolioCount = fund.Files.Count,
            LatestPortfolioDate = fund.Files.OrderByDescending(pf => pf.ReportDate).FirstOrDefault()?.ReportDate
        };
    }

    /// <summary>
    /// Creates a dictionary from holdings using ISIN as key, handling duplicates by taking the first occurrence
    /// </summary>
    private Dictionary<string, PortfolioHolding> CreateHoldingDictionaryByISIN(IEnumerable<PortfolioHolding> holdings)
    {
        var result = new Dictionary<string, PortfolioHolding>();
        var duplicateIsins = new List<string>();
        
        foreach (var holding in holdings)
        {
            var isin = holding.ISIN ?? string.Empty;
            
            if (string.IsNullOrEmpty(isin))
            {
                _logger.LogWarning("Found holding with empty ISIN for security {SecurityName}", holding.SecurityName ?? "Unknown");
                continue;
            }
            
            if (!result.ContainsKey(isin))
            {
                result[isin] = holding;
            }
            else
            {
                if (!duplicateIsins.Contains(isin))
                {
                    duplicateIsins.Add(isin);
                    _logger.LogWarning("Duplicate ISIN {ISIN} found. Using first occurrence for security {SecurityName}, skipping {SecurityName2}", 
                        isin, result[isin].SecurityName ?? "Unknown", holding.SecurityName ?? "Unknown");
                }
            }
        }
        
        if (duplicateIsins.Any())
        {
            _logger.LogInformation("Processed {TotalCount} holdings with {DuplicateCount} duplicate ISINs. Final dictionary contains {UniqueCount} unique holdings.", 
                holdings.Count(), duplicateIsins.Count, result.Count);
        }
        
        return result;
    }
}
