using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MutualFundScanner.Infrastructure.UnitOfWork;



namespace MutualFundScanner.Application.Services
{
    public interface IUnitOfWorkWrapper
    {
        Task ExecuteAsync(Func<Task> operation);
        Task<T> ExecuteAsync<T>(Func<Task<T>> operation);
        Task ExecuteAsync(Action operation);
        Task<T> ExecuteAsync<T>(Func<T> operation);
    }

    public class UnitOfWorkWrapper : IUnitOfWorkWrapper
    {
        private readonly Func<IUnitOfWork> _unitOfWorkFactory;
        private readonly ILogger<UnitOfWorkWrapper> _logger;

        public UnitOfWorkWrapper(Func<IUnitOfWork> unitOfWorkFactory, ILogger<UnitOfWorkWrapper> logger)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _logger = logger;
        }

        public async Task ExecuteAsync(Func<Task> operation)
        {
            using var unitOfWork = _unitOfWorkFactory();
            try
            {
                await unitOfWork.BeginTransactionAsync();
                await operation();
                await unitOfWork.CommitAsync();
                _logger.LogDebug("Transaction completed successfully");
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Transaction failed and was rolled back");
                throw;
            }
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
        {
            using var unitOfWork = _unitOfWorkFactory();
            try
            {
                await unitOfWork.BeginTransactionAsync();
                var result = await operation();
                await unitOfWork.CommitAsync();
                _logger.LogDebug("Transaction completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Transaction failed and was rolled back");
                throw;
            }
        }

        public async Task ExecuteAsync(Action operation)
        {
            using var unitOfWork = _unitOfWorkFactory();
            try
            {
                await unitOfWork.BeginTransactionAsync();
                operation();
                await unitOfWork.CommitAsync();
                _logger.LogDebug("Transaction completed successfully");
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Transaction failed and was rolled back");
                throw;
            }
        }

        public async Task<T> ExecuteAsync<T>(Func<T> operation)
        {
            using var unitOfWork = _unitOfWorkFactory();
            try
            {
                await unitOfWork.BeginTransactionAsync();
                var result = operation();
                await unitOfWork.CommitAsync();
                _logger.LogDebug("Transaction completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Transaction failed and was rolled back");
                throw;
            }
        }
    }
}
