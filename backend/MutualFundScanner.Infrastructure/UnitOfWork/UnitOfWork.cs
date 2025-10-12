using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using MutualFundScanner.Infrastructure.Data;

namespace MutualFundScanner.Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MutualFundContext _context;
        private readonly ILogger<UnitOfWork> _logger;
        private bool _disposed = false;

        public MutualFundContext Context => _context;

        public UnitOfWork(MutualFundContext context, ILogger<UnitOfWork> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<int> SaveChangesAsync()
        {
            try
            {
                var trackedEntities = _context.ChangeTracker.Entries()
                    .Where(e => e.State != EntityState.Unchanged)
                    .ToList();
                
                _logger.LogDebug("Saving {ChangedCount} changed entities", trackedEntities.Count);
                
                var savedCount = await _context.SaveChangesAsync();
                _logger.LogDebug("SaveChangesAsync completed: {SavedCount} entities persisted", savedCount);
                
                return savedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during SaveChangesAsync");
                throw;
            }
        }

        public async Task<T> ExecuteInTransactionAsync<T>(Func<MutualFundContext, Task<T>> operation)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _logger.LogDebug("Starting transaction with Context ID: {ContextId}", _context.ContextId);
                    
                    var result = await operation(_context);
                    
                    // CRITICAL: Save all changes before committing the transaction
                    var savedCount = await _context.SaveChangesAsync();
                    _logger.LogDebug("Saved {SavedCount} entities before transaction commit", savedCount);
                    
                    await transaction.CommitAsync();
                    _logger.LogDebug("Transaction committed successfully");
                    
                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Transaction failed, rolling back");
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task ExecuteInTransactionAsync(Func<MutualFundContext, Task> operation)
        {
            await ExecuteInTransactionAsync(async (context) =>
            {
                await operation(context);
                return 0; // Dummy return value
            });
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                // Context is managed by DI container, don't dispose it here
                _disposed = true;
            }
        }
    }
}
