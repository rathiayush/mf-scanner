using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MutualFundScanner.Infrastructure.Data;

namespace MutualFundScanner.Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MutualFundContext _context;
        private IDbContextTransaction? _transaction;
        private bool _disposed = false;

        public UnitOfWork(MutualFundContext context)
        {
            _context = context;
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("A transaction is already in progress.");
            }

            _transaction = await _context.Database.BeginTransactionAsync();
            return _transaction;
        }

        public async Task CommitAsync()
        {
            try
            {
                if (_transaction == null)
                {
                    throw new InvalidOperationException("No transaction to commit.");
                }

                await _context.SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            catch
            {
                await RollbackAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackAsync()
        {
            try
            {
                if (_transaction != null)
                {
                    await _transaction.RollbackAsync();
                }
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
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
                _transaction?.Dispose();
                _context?.Dispose();
                _disposed = true;
            }
        }
    }
}
