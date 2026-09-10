using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using SharedService.SharedKernel;

namespace DirectoryService.Infrastructure.Database;

public class TransactionManager : ITransactionManager
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<TransactionManager> _logger;
    private readonly ILoggerFactory _loggerFactory;

    public TransactionManager(
        AppDbContext dbContext,
        ILogger<TransactionManager> logger,
        ILoggerFactory loggerFactory)
    {
        _dbContext = dbContext;
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    public async Task<Result<ITransactionScope, Error>> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Блокировки строк защищают инварианты перемещения; Serializable добавил бы лишние ошибки сериализации.
            var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.ReadCommitted,
                cancellationToken);

            var transactionScopeLogger = _loggerFactory.CreateLogger<TransactionScope>();

            var transactionScope = new TransactionScope(transaction.GetDbTransaction(), transactionScopeLogger);

            return transactionScope;
        }
        catch (Exception e)
        {
            string message = "Failed to begin transaction.";

            _logger.LogError(e, message);

            return Error.Failure("database", message);
        }
    }

    public async Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (Exception e)
        {
            string message = "Failed to save changes.";

            _logger.LogError(e, message);

            return Error.Failure("database", message);
        }
    }
}
