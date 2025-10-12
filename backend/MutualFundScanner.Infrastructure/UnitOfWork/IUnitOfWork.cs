using MutualFundScanner.Infrastructure.Data;

namespace MutualFundScanner.Infrastructure.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        MutualFundContext Context { get; }
        Task<int> SaveChangesAsync();
        Task<T> ExecuteInTransactionAsync<T>(Func<MutualFundContext, Task<T>> operation);
        Task ExecuteInTransactionAsync(Func<MutualFundContext, Task> operation);
    }
}
