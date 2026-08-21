using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Responses;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedService.Core.Abstractions;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Features.Locations.GetTop;

public sealed record GetLocationTopQuery() : IQuery;

public sealed class GetLocationTopQueryValidator : AbstractValidator<GetLocationTopQuery>
{
    public GetLocationTopQueryValidator()
    { }
}

public sealed class GetLocationByIdHandler : IQueryHandler<GetLocationTopResponse, GetLocationTopQuery>
{
    private readonly IReadDbContext _readDbContext;
    private readonly IValidator<GetLocationTopQuery> _validator;
    private readonly ILogger<GetLocationByIdHandler> _logger;

    public GetLocationByIdHandler(
        IReadDbContext readDbContext,
        IValidator<GetLocationTopQuery> validator,
        ILogger<GetLocationByIdHandler> logger)
    {
        _readDbContext = readDbContext;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<GetLocationTopResponse, Error>> Handle(GetLocationTopQuery query, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        _logger.LogInformation("Location with id {LocationId} found.", query.Id);

        return new GetLocationResponse(
                location.Id.Value,
                location.Name.Value,
                location.Address.City,
                location.Address.District,
                location.Address.Street,
                location.Address.Structure,
                location.TimeZone.Value,
                location.CreatedAt,
                location.UpdatedAt);
    }
}