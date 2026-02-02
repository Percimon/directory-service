using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using SharedService.Core.Abstractions;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Departments.ChangeParent;

public class ChangeParentHandler : ICommandHandler<Guid, ChangeParentCommand>
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<ChangeParentHandler> _logger;
    private readonly IValidator<ChangeParentCommand> _validator;

    public ChangeParentHandler(
        IDepartmentsRepository departmentsRepository,
        ITransactionManager transactionManager,
        ILogger<ChangeParentHandler> logger,
        IValidator<ChangeParentCommand> validator)
    {
        _departmentsRepository = departmentsRepository;
        _transactionManager = transactionManager;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result<Guid, Error>> Handle(
        ChangeParentCommand command,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        return command.DepartmentId;
    }
}