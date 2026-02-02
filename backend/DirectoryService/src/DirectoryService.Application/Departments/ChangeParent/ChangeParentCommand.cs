using SharedService.Core.Abstractions;

namespace DirectoryService.Application.Departments.ChangeParent;

public record ChangeParentCommand(Guid DepartmentId, Guid? NewParentId) : ICommand;
