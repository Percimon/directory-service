using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Identifiers;
using DirectoryService.Domain.ValueObjects;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.Departments.Create;

public class CreateDepartmentHandler
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ILogger<CreateDepartmentHandler> _logger;
    private readonly IValidator<CreateDepartmentCommand> _validator;

    public CreateDepartmentHandler(
        IDepartmentsRepository departmentsRepository,
        ISqlConnectionFactory sqlConnectionFactory,
        ILogger<CreateDepartmentHandler> logger,
        IValidator<CreateDepartmentCommand> validator)
    {
        _departmentsRepository = departmentsRepository;
        _sqlConnectionFactory = sqlConnectionFactory;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result<Guid, Error>> Handle(
        CreateDepartmentCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        DepartmentId departmentId = DepartmentId.New().Value;

        Name name = Name.Create(command.Name).Value;

        Identifier identifier = Identifier.Create(command.Identifier).Value;

        bool locationsAreExist = AllIdsAreExist(command.Locations);

        if (!locationsAreExist)
        {
            return GeneralErrors.NotFound();
        }

        IEnumerable<DepartmentLocation> departmentLocations = command.Locations
            .Select(id => DepartmentLocation.Create(departmentId, LocationId.Create(id)).Value);

        if (command.ParentId is null)
        {
            var departmentResult = Department.CreateParent(
                name,
                identifier,
                departmentLocations,
                departmentId);

            if (departmentResult.IsFailure)
                return departmentResult.Error;

            var saveResult = await _departmentsRepository.Add(departmentResult.Value, cancellationToken);

            if (saveResult.IsFailure)
                return saveResult.Error;

            _logger.LogInformation("Department created with id={Id}", departmentId.Value);

            return departmentResult.Value.Id.Value;
        }
        else
        {
            var parentQuery = await _departmentsRepository.GetById(command.ParentId, cancellationToken);

            if (parentQuery.IsFailure)
                return parentQuery.Error;

            var departmentResult = Department.CreateChild(
                name,
                identifier,
                parentQuery.Value,
                departmentLocations,
                departmentId);

            if (departmentResult.IsFailure)
                return departmentResult.Error;

            var saveResult = await _departmentsRepository.Add(departmentResult.Value, cancellationToken);

            if (saveResult.IsFailure)
                return saveResult.Error;

            _logger.LogInformation("Child department created with id={Id}", departmentId.Value);

            return departmentResult.Value.Id.Value;
        }
    }

    private bool AllIdsAreExist(IReadOnlyList<Guid> ids)
    {
        DynamicParameters parameters = new DynamicParameters();

        parameters.Add("@Ids", ids);

        string query =
            """
                SELECT COUNT(id) 
                FROM locations 
                WHERE id = ANY(@Ids) AND is_active = TRUE;
            """;

        using (var connection = _sqlConnectionFactory.Create())
        {
            int count = connection.ExecuteScalar<int>(query, parameters);

            return count == ids.Count;
        }
    }
}