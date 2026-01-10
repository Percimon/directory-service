using System.Data;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Infrastructure.Database;

public class TransactionScope : ITransactionScope
{
    private readonly IDbTransaction _transaction;
    private readonly ILogger<TransactionScope> _logger;

    public TransactionScope(IDbTransaction transaction, ILogger<TransactionScope> logger)
    {
        _transaction = transaction;
        _logger = logger;
    }

    public UnitResult<Error> Commit()
    {
        try
        {
            _transaction.Commit();

            return UnitResult.Success<Error>();
        }
        catch (Exception e)
        {
            string message = "Failed to commit transaction.";

            _logger.LogError(e, message);

            return Error.Failure("transaction.commit.failure", message);
        }
    }

    public UnitResult<Error> Rollback()
    {
        try
        {
            _transaction.Rollback();

            return UnitResult.Success<Error>();
        }
        catch (Exception e)
        {
            string message = "Failed to rollback transaction.";

            _logger.LogError(e, message);

            return Error.Failure("transaction.rollback.failure", message);
        }
    }

    public void Dispose()
    {
        _transaction.Dispose();
    }
}