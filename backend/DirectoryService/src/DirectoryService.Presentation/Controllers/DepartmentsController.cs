using DirectoryService.Application.Departments.ChangeParent;
using DirectoryService.Application.Departments.Create;
using DirectoryService.Application.Departments.UpdateLocations;
using DirectoryService.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;
using SharedService.Framework.EndpointResults;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
[Route("api/departments")]
public class DepartmentsController : Controller
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> Create(
        [FromServices] CreateDepartmentHandler handler,
        [FromBody] CreateDepartmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateDepartmentCommand(
            request.Name,
            request.Identifier,
            request.ParentId,
            request.Locations);

        return await handler.Handle(command, cancellationToken);
    }

    [HttpPatch("{departmentId}/locations")]
    public async Task<EndpointResult<Guid>> UpdateLocations(
        [FromRoute] Guid departmentId,
        [FromServices] UpdateLocationsHandler handler,
        [FromBody] UpdateLocationsRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateLocationsCommand(departmentId, request.LocationIds);

        return await handler.Handle(command, cancellationToken);
    }

    [HttpPut("{departmentId}/parent")]
    public async Task<EndpointResult<Guid>> ChangeParent(
        [FromRoute] Guid departmentId,
        [FromServices] ChangeParentHandler handler,
        [FromBody] ChangeParentRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new ChangeParentCommand(departmentId, request.NewParentId);

        return await handler.Handle(command, cancellationToken);
    }
}