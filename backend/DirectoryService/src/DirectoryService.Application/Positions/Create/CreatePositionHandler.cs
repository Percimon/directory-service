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

namespace DirectoryService.Application.Positions.Create;

public class CreatePositionHandler
{
    private readonly IPositionsRepository _postionsRepository;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ILogger<CreatePositionHandler> _logger;
    private readonly IValidator<CreatePositionCommand> _validator;

    public CreatePositionHandler(
        IPositionsRepository postionsRepository,
        IDepartmentsRepository departmentsRepository,
        ISqlConnectionFactory sqlConnectionFactory,
        ILogger<CreatePositionHandler> logger,
        IValidator<CreatePositionCommand> validator)
    {
        _postionsRepository = postionsRepository;
        _departmentsRepository = departmentsRepository;
        _sqlConnectionFactory = sqlConnectionFactory;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result<Guid, Error>> Handle(
        CreatePositionCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        PositionId positionId = PositionId.New();

        Name name = Name.Create(command.Name).Value;

        Description description = Description.Create(command.Description).Value;

        bool departmentsAreExist = AllIdsAreExist(command.Departments);

        if (!departmentsAreExist)
        {
            return GeneralErrors.NotFound();
        }

        DateTime createdAt = DateTime.UtcNow;

        var position = new Position(positionId, name, description, createdAt);

        var saveResult = await _postionsRepository.Add(position, cancellationToken);

        if (saveResult.IsFailure)
            return saveResult.Error;

        _logger.LogInformation("Position created with id={Id}", positionId.Value);

        foreach (Guid departmentId in command.Departments)
        {
            Result<Department, Error> departmentQuery = await _departmentsRepository.GetById(departmentId);

            //сразу обращаюсь к Value, т.к. выше уже была проверка на существование Department Id
            var positionAddResult = departmentQuery.Value.AddPosition(positionId.Value);

            if (positionAddResult.IsFailure)
                return positionAddResult.Error;
        }

        return position.Id.Value;
    }

    private bool AllIdsAreExist(IReadOnlyList<Guid> ids)
    {
        DynamicParameters parameters = new DynamicParameters();

        parameters.Add("@Ids", ids);

        string query =
            $"""
                SELECT COUNT(id) 
                FROM departments 
                WHERE id IN ANY(@Ids) AND is_active = TRUE;
            """;

        using (var connection = _sqlConnectionFactory.Create())
        {
            int count = connection.ExecuteScalar<int>(query, parameters);

            return count == ids.Count;
        }
    }
}
