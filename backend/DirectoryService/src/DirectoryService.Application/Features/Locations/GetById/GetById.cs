using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Dtos;
using DirectoryService.Domain.Identifiers;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedService.Core.Abstractions;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Features.Locations.GetById;

public sealed record GetByIdLocationQuery(Guid Id) : IQuery;

public sealed class GetByIdLocationQueryValidator : AbstractValidator<GetByIdLocationQuery>
{
    public GetByIdLocationQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Location id is required.");
    }
}

public sealed class GetLocationByIdHandler : IQueryHandler<LocationDto, GetByIdLocationQuery>
{
    private readonly IReadDbContext _readDbContext;
    private readonly IValidator<GetByIdLocationQuery> _validator;
    private readonly ILogger<GetLocationByIdHandler> _logger;

    public GetLocationByIdHandler(
        IReadDbContext readDbContext,
        IValidator<GetByIdLocationQuery> validator,
        ILogger<GetLocationByIdHandler> logger)
    {
        _readDbContext = readDbContext;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<LocationDto, Error>> Handle(GetByIdLocationQuery query, CancellationToken cancellationToken = default)
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

        return new LocationDto(
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