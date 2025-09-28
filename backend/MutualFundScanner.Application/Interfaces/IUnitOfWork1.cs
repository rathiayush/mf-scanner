using System;
using System.Threading.Tasks;

namespace MutualFundScanner.Application.Interfaces
{
    public interface IUnitOfWork1 : IDisposable
    {
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
        Task SaveChangesAsync();
        bool HasActiveTransaction { get; }
    }
}
