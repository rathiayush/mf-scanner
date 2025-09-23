using MutualFundScanner.Application.DTOs;
using MutualFundScanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MutualFundScanner.Infrastructure.Data;

namespace MutualFundScanner.Application.Helpers;

/// <summary>
/// Helper class for validating data and requests
/// </summary>
public class DataValidationHelper
{
    private readonly MutualFundContext _context;
    private readonly ILogger<DataValidationHelper> _logger;

    public DataValidationHelper(MutualFundContext context, ILogger<DataValidationHelper> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Validates the analytics request parameters
    /// </summary>
    public (bool IsValid, string ErrorMessage) ValidateAnalyticsRequest(AnalyticsRequestDto request)
    {
        if (request.MutualFundIds == null || !request.MutualFundIds.Any())
        {
            return (false, "At least one mutual fund must be selected");
        }

        if (request.FromDate >= request.ToDate)
        {
            return (false, "From date must be earlier than to date");
        }

        if (request.Page < 1)
        {
            return (false, "Page number must be greater than 0");
        }

        if (request.PageSize < 1)
        {
            return (false, "Page size must be greater than 0");
        }

        return (true, string.Empty);
    }

    /// <summary>
    /// Validates that dates are available for all selected mutual funds
    /// </summary>
    public async Task<(bool IsValid, string ErrorMessage)> ValidateDatesForFundsAsync(List<int> mutualFundIds, DateTime fromDate, DateTime toDate)
    {
        if (mutualFundIds == null || !mutualFundIds.Any())
            return (false, "No mutual funds selected");

        if (fromDate >= toDate)
            return (false, "From date must be earlier than to date");

        // For each selected fund, check if there are snapshots within the date range
        foreach (var fundId in mutualFundIds)
        {
            var hasValidSnapshots = await HasValidSnapshotsForFund(fundId, fromDate, toDate);
            if (!hasValidSnapshots)
            {
                var fundName = await _context.MutualFunds
                    .Where(f => f.Id == fundId)
                    .Select(f => f.FundName)
                    .FirstOrDefaultAsync() ?? $"Fund {fundId}";
                
                return (false, $"No valid snapshots found for {fundName} within the selected date range");
            }
        }

        return (true, string.Empty);
    }

    /// <summary>
    /// Checks if a fund has valid snapshots within the date range
    /// </summary>
    private async Task<bool> HasValidSnapshotsForFund(int fundId, DateTime fromDate, DateTime toDate)
    {
        try
        {
            

            

            //// Additionally, check if there are any snapshots within the actual date range
            //var snapshotsInRange = await _context.PortfolioSnapshots
            //    .Where(s => s.MutualFundId == fundId && 
            //               s.SnapshotDate.Date >= fromDate.Date && 
            //               s.SnapshotDate.Date <= toDate.Date)
            //    .AnyAsync();

            //if (!snapshotsInRange)
            //{
            //    _logger.LogWarning("No snapshots found within date range for fund {FundId}", fundId);
            //    return false;
            //}

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating snapshots for fund {FundId}", fundId);
            return false;
        }
    }

    /// <summary>
    /// Validates that mutual fund IDs exist in the database
    /// </summary>
    public async Task<(bool IsValid, string ErrorMessage)> ValidateMutualFundIdsAsync(List<int> mutualFundIds)
    {
        if (mutualFundIds == null || !mutualFundIds.Any())
            return (false, "No mutual fund IDs provided");

        var existingFundIds = await _context.MutualFunds
            .Where(f => mutualFundIds.Contains(f.Id))
            .Select(f => f.Id)
            .ToListAsync();

        var missingFundIds = mutualFundIds.Except(existingFundIds).ToList();
        if (missingFundIds.Any())
        {
            return (false, $"The following mutual fund IDs do not exist: {string.Join(", ", missingFundIds)}");
        }

        return (true, string.Empty);
    }

    /// <summary>
    /// Validates current holdings request
    /// </summary>
    public async Task<(bool IsValid, string ErrorMessage)> ValidateCurrentHoldingsRequest(CurrentHoldingsRequestDto request)
    {
        if (request.MutualFundId <= 0)
        {
            return (false, "Invalid mutual fund ID");
        }

        if (request.TargetDate == default)
        {
            return (false, "Invalid target date specified");
        }

        var fundExists = await _context.MutualFunds.AnyAsync(f => f.Id == request.MutualFundId);
        if (!fundExists)
        {
            return (false, $"Mutual fund with ID {request.MutualFundId} not found");
        }

        return (true, string.Empty);
    }

    /// <summary>
    /// Gets available date ranges for mutual funds
    /// </summary>
    public async Task<AvailableDatesResponseDto> GetAvailableDatesAsync(List<int>? mutualFundIds = null)
    {
        var query = _context.PortfolioSnapshots.AsQueryable();

        if (mutualFundIds != null && mutualFundIds.Any())
        {
            query = query.Where(s => mutualFundIds.Contains(s.MutualFundId));
        }

        var dates = await query
            .Select(s => s.SnapshotDate.Date)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();

        var availableDates = dates.Select(d => new DateRangeDto
        {
            Date = d,
            DisplayName = d.ToString("MMM yyyy"),
            HasData = true
        }).ToList();

        var result = new AvailableDatesResponseDto
        {
            AvailableDates = availableDates,
            MinDate = dates.FirstOrDefault(),
            MaxDate = dates.LastOrDefault()
        };

        // If specific mutual funds are requested, get detailed availability per fund
        if (mutualFundIds != null && mutualFundIds.Any())
        {
            var fundNames = await _context.MutualFunds
                .Where(f => mutualFundIds.Contains(f.Id))
                .ToDictionaryAsync(f => f.Id, f => f.FundName);

            var fundAvailabilities = new List<MutualFundDateAvailability>();
            
            foreach (var fundId in mutualFundIds)
            {
                var fundDates = await _context.PortfolioSnapshots
                    .Where(s => s.MutualFundId == fundId)
                    .Select(s => s.SnapshotDate.Date)
                    .Distinct()
                    .OrderBy(d => d)
                    .ToListAsync();

                fundAvailabilities.Add(new MutualFundDateAvailability
                {
                    MutualFundId = fundId,
                    MutualFundName = fundNames.TryGetValue(fundId, out var name) ? name : $"Fund {fundId}",
                    MinDate = fundDates.FirstOrDefault(),
                    MaxDate = fundDates.LastOrDefault(),
                    AvailableDates = fundDates
                });
            }

            result.FundDateAvailabilities = fundAvailabilities;
        }

        return result;
    }
}
