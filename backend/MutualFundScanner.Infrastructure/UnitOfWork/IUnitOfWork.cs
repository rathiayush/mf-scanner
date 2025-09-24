using Microsoft.EntityFrameworkCore.Storage;

namespace MutualFundScanner.Infrastructure.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
        Task<int> SaveChangesAsync();
    }
}
