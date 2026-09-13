using CSharpFunctionalExtensions;
using SharedService.SharedKernel;

namespace SharedService.Core.Database;

public interface ITransactionScope : IDisposable
{
    UnitResult<Error> Commit();

    UnitResult<Error> Rollback();
}