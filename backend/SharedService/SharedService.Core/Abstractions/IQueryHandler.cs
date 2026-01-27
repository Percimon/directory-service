using CSharpFunctionalExtensions;
using SharedService.SharedKernel;

namespace SharedService.Core.Abstractions;

public interface IQueryHandler<TResponse, in TQuery>
    where TQuery : IQuery
{
    Task<Result<TResponse, Error>> Handle(TQuery command, CancellationToken cancellationToken);
}