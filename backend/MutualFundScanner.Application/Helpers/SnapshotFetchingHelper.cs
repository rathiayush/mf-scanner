using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MutualFundScanner.Application.Interfaces;
using MutualFundScanner.Domain.Entities;
using MutualFundScanner.Infrastructure.Data;

namespace MutualFundScanner.Application.Helpers;

/// <summary>
/// Helper class for common snapshot fetching operations with date range filtering
/// </summary>
public class SnapshotFetchingHelper : ISnapshotFetchingHelper
{
    private readonly MutualFundContext _context;
    private readonly ILogger<SnapshotFetchingHelper> _logger;

    public SnapshotFetchingHelper(MutualFundContext context, ILogger<SnapshotFetchingHelper> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Fetches snapshots for specified mutual funds within a date range
    /// Only includes mutual funds that have snapshots in the specified date range
    /// </summary>
    public async Task<List<PortfolioSnapshot>> GetSnapshotsInDateRange(
        List<int> mutualFundIds, 
        DateTime fromDate, 
        DateTime toDate)
    {
        // Input validation
        if (mutualFundIds == null || !mutualFundIds.Any())
        {
            _logger.LogWarning("No mutual fund IDs provided for snapshot fetching");
            return new List<PortfolioSnapshot>();
        }

        if (fromDate > toDate)
        {
            _logger.LogWarning("Invalid date range: fromDate {FromDate:yyyy-MM-dd} is after toDate {ToDate:yyyy-MM-dd}", fromDate, toDate);
            return new List<PortfolioSnapshot>();
        }

        var snapshots = await _context.PortfolioSnapshots
            .Include(s => s.Holdings)
            
            .Where(s => mutualFundIds.Contains(s.MutualFundId) &&
                       s.SnapshotDate >= fromDate.Date &&
                       s.SnapshotDate <= toDate.Date)
            .ToListAsync();

        // Get fund IDs that have snapshots in the date range
        var fundsWithSnapshots = snapshots.Select(s => s.MutualFundId).Distinct().ToList();

        // Log information about missing funds
        if (fundsWithSnapshots.Count < mutualFundIds.Count)
        {
            var missingFunds = mutualFundIds.Except(fundsWithSnapshots).ToList();
            _logger.LogWarning("Excluding {MissingCount} mutual funds that don't have snapshots in date range {FromDate:yyyy-MM-dd} to {ToDate:yyyy-MM-dd}: {MissingFunds}",
                missingFunds.Count, fromDate, toDate, string.Join(",", missingFunds));
        }

        _logger.LogInformation("Retrieved {SnapshotCount} snapshots for {FundCount} funds in date range {FromDate:yyyy-MM-dd} to {ToDate:yyyy-MM-dd}", 
            snapshots.Count, fundsWithSnapshots.Count, fromDate, toDate);

        return snapshots;
    }

    /// <summary>
    /// Gets the latest snapshot for each mutual fund up to a specific date
    /// Returns only the most recent snapshot for each fund on or before the specified date
    /// </summary>
    public async Task<List<PortfolioSnapshot>> GetLatestSnapshotsBeforeDate(
        List<int> mutualFundIds, 
        DateTime beforeDate)
    {
        // Input validation
        if (mutualFundIds == null || !mutualFundIds.Any())
        {
            _logger.LogWarning("No mutual fund IDs provided for latest snapshot fetching");
            return new List<PortfolioSnapshot>();
        }

        // Get the latest snapshot for each mutual fund before the specified date
        var latestSnapshots = await _context.PortfolioSnapshots
            .Include(s => s.Holdings)
            .Where(s => mutualFundIds.Contains(s.MutualFundId) &&
                       s.SnapshotDate <= beforeDate.Date)
            .GroupBy(s => s.MutualFundId)
            .Select(g => g.OrderByDescending(s => s.SnapshotDate).First())
            .ToListAsync();

        // Get fund IDs that have snapshots
        var fundsWithSnapshots = latestSnapshots.Select(s => s.MutualFundId).Distinct().ToList();

        // Log information about missing funds
        if (fundsWithSnapshots.Count < mutualFundIds.Count)
        {
            var missingFunds = mutualFundIds.Except(fundsWithSnapshots).ToList();
            _logger.LogWarning("Excluding {MissingCount} mutual funds that don't have snapshots before date {BeforeDate:yyyy-MM-dd}: {MissingFunds}",
                missingFunds.Count, beforeDate, string.Join(",", missingFunds));
        }

        _logger.LogInformation("Retrieved {SnapshotCount} latest snapshots for {FundCount} funds before date {BeforeDate:yyyy-MM-dd}", 
            latestSnapshots.Count, fundsWithSnapshots.Count, beforeDate);

        return latestSnapshots;
    }

    /// <summary>
    /// Gets mutual fund names for the provided fund IDs
    /// </summary>
    public async Task<Dictionary<int, string>> GetMutualFundNames(List<int> mutualFundIds)
    {
        return await _context.MutualFunds
            .Where(mf => mutualFundIds.Contains(mf.Id))
            .ToDictionaryAsync(mf => mf.Id, mf => mf.FundName);
    }

    /// <summary>
    /// Resolves "All Funds" request by getting all mutual fund IDs
    /// </summary>
    public async Task<List<int>> ResolveAllFunds(List<int>? mutualFundIds, ILogger logger)
    {
        if (mutualFundIds == null || !mutualFundIds.Any())
        {
            var allFundIds = await _context.MutualFunds.Select(mf => mf.Id).ToListAsync();
            logger.LogInformation("All Funds analysis requested. Using {Count} funds: {FundIds}", 
                allFundIds.Count, string.Join(",", allFundIds));
            return allFundIds;
        }

        return mutualFundIds;
    }

    /// <summary>
    /// Validates that mutual funds exist in the database
    /// </summary>
    public async Task<(bool IsValid, string ErrorMessage, List<int> ValidFundIds)> ValidateMutualFunds(List<int> mutualFundIds)
    {
        var existingFundIds = await _context.MutualFunds
            .Where(mf => mutualFundIds.Contains(mf.Id))
            .Select(mf => mf.Id)
            .ToListAsync();

        var missingFundIds = mutualFundIds.Except(existingFundIds).ToList();
        
        if (missingFundIds.Any())
        {
            return (false, $"Mutual fund(s) not found: {string.Join(",", missingFundIds)}", existingFundIds);
        }

        return (true, string.Empty, existingFundIds);
    }

    /// <summary>
    /// Fetches both from and to snapshots for specified mutual funds in a single call
    /// Returns snapshots grouped by date (from and to dates)
    /// </summary>
    public async Task<(List<PortfolioSnapshot> FromSnapshots, List<PortfolioSnapshot> ToSnapshots)> GetFromAndToSnapshots(
        List<int> mutualFundIds, 
        DateTime fromDate, 
        DateTime toDate)
    {
        // Input validation
        if (mutualFundIds == null || !mutualFundIds.Any())
        {
            _logger.LogWarning("No mutual fund IDs provided for snapshot fetching");
            return (new List<PortfolioSnapshot>(), new List<PortfolioSnapshot>());
        }

        if (fromDate > toDate)
        {
            _logger.LogWarning("From date {FromDate:yyyy-MM-dd} is after to date {ToDate:yyyy-MM-dd}", fromDate, toDate);
            return (new List<PortfolioSnapshot>(), new List<PortfolioSnapshot>());
        }

        // Get all snapshots in the date range for the specified funds
        var allSnapshots = await _context.PortfolioSnapshots
            .Where(ps => mutualFundIds.Contains(ps.MutualFundId) && 
                        ps.SnapshotDate.Date >= fromDate.Date && 
                        ps.SnapshotDate.Date <= toDate.Date)
            .Include(ps => ps.Holdings)
            .ToListAsync();

        // Group snapshots by date and fund, then get the closest to each target date
        var fromSnapshots = new List<PortfolioSnapshot>();
        var toSnapshots = new List<PortfolioSnapshot>();

        foreach (var fundId in mutualFundIds)
        {
            var fundSnapshots = allSnapshots.Where(s => s.MutualFundId == fundId).ToList();
            
            if (fundSnapshots.Any())
            {
                // Find closest snapshot to from date
                var fromSnapshot = fundSnapshots
                    .Where(s => s.SnapshotDate.Date >= fromDate.Date)
                    .OrderBy(s => s.SnapshotDate)
                    .FirstOrDefault();
                
                if (fromSnapshot != null)
                {
                    fromSnapshots.Add(fromSnapshot);
                }

                // Find closest snapshot to to date
                var toSnapshot = fundSnapshots
                    .Where(s => s.SnapshotDate.Date <= toDate.Date)
                    .OrderByDescending(s => s.SnapshotDate)
                    .FirstOrDefault();
                
                if (toSnapshot != null)
                {
                    toSnapshots.Add(toSnapshot);
                }
            }
        }

        var fundsWithFromSnapshots = fromSnapshots.Select(s => s.MutualFundId).ToHashSet();
        var fundsWithToSnapshots = toSnapshots.Select(s => s.MutualFundId).ToHashSet();

        _logger.LogInformation("Retrieved {FromCount} from snapshots and {ToCount} to snapshots for date range {FromDate:yyyy-MM-dd} to {ToDate:yyyy-MM-dd}", 
            fromSnapshots.Count, toSnapshots.Count, fromDate, toDate);

        return (fromSnapshots, toSnapshots);
    }
}
