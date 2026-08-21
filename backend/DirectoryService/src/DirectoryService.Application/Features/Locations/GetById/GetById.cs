using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Responses;
using DirectoryService.Domain.Identifiers;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedService.Core.Abstractions;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Features.Locations.GetById;

public sealed record GetLocationByIdQuery(Guid Id) : IQuery;

public sealed class GetLocationByIdQueryValidator : AbstractValidator<GetLocationByIdQuery>
{
    public GetLocationByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Location id is required.");
    }
}

public sealed class GetLocationByIdHandler : IQueryHandler<GetLocationResponse, GetLocationByIdQuery>
{
    private readonly IReadDbContext _readDbContext;
    private readonly IValidator<GetLocationByIdQuery> _validator;
    private readonly ILogger<GetLocationByIdHandler> _logger;

    public GetLocationByIdHandler(
        IReadDbContext readDbContext,
        IValidator<GetLocationByIdQuery> validator,
        ILogger<GetLocationByIdHandler> logger)
    {
        _readDbContext = readDbContext;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<GetLocationResponse, Error>> Handle(GetLocationByIdQuery query, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        var location = _readDbContext.LocationsRead
            .FirstOrDefault(l => l.Id == LocationId.Create(query.Id));

        if (location is null)
        {
            return GeneralErrors.NotFound(query.Id);
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