using Microsoft.Extensions.Logging;
using MutualFundScanner.Domain.Entities;

namespace MutualFundScanner.Application.Interfaces;

/// <summary>
/// Interface for snapshot fetching operations with date range filtering
/// </summary>
public interface ISnapshotFetchingHelper
{
    /// <summary>
    /// Fetches snapshots for specified mutual funds within a date range
    /// Only includes mutual funds that have snapshots in the specified date range
    /// </summary>
    Task<List<PortfolioSnapshot>> GetSnapshotsInDateRange(
        List<int> mutualFundIds, 
        DateTime fromDate, 
        DateTime toDate);

    /// <summary>
    /// Gets the latest snapshot for each mutual fund up to a specific date
    /// Returns only the most recent snapshot for each fund on or before the specified date
    /// </summary>
    Task<List<PortfolioSnapshot>> GetLatestSnapshotsBeforeDate(
        List<int> mutualFundIds, 
        DateTime beforeDate);

    /// <summary>
    /// Gets mutual fund names for the provided fund IDs
    /// </summary>
    Task<Dictionary<int, string>> GetMutualFundNames(List<int> mutualFundIds);

    /// <summary>
    /// Resolves "All Funds" request by getting all mutual fund IDs
    /// </summary>
    Task<List<int>> ResolveAllFunds(List<int>? mutualFundIds, ILogger logger);

    /// <summary>
    /// Validates that mutual funds exist in the database
    /// </summary>
    Task<(bool IsValid, string ErrorMessage, List<int> ValidFundIds)> ValidateMutualFunds(List<int> mutualFundIds);

    /// <summary>
    /// Fetches both from and to snapshots for specified mutual funds in a single call
    /// Returns snapshots grouped by date (from and to dates)
    /// </summary>
    Task<(List<PortfolioSnapshot> FromSnapshots, List<PortfolioSnapshot> ToSnapshots)> GetFromAndToSnapshots(
        List<int> mutualFundIds, 
        DateTime fromDate, 
        DateTime toDate);
}
