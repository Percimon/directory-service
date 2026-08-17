using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.ChangeParent;
using DirectoryService.Application.Departments.Create;
using DirectoryService.Application.Departments.Update;
using DirectoryService.Application.Departments.UpdateLocations;
using DirectoryService.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;
using SharedService.Framework.EndpointResults;
using SharedService.SharedKernel;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
[Route("api/departments")]
public class DepartmentsController : Controller
{
    [HttpGet]
    public EndpointResult Get(CancellationToken cancellationToken = default)
    {
        return Result.Success<Error>();
    }

    [HttpGet("{id}")]
    public EndpointResult GetById([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        return Result.Success<Error>();
    }

    [HttpPatch("{id}")]
    public async Task<EndpointResult<Guid>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateDepartmentRequest request,
        [FromServices] UpdateDepartmentHandler handler,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateDepartmentCommand(id, request.Name, request.Slug);

        return await handler.Handle(command, cancellationToken);
    }

    [HttpDelete("{id}")]
    public EndpointResult Delete([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        return Result.Success<Error>();
    }

    [HttpPost]
    public async Task<EndpointResult<Guid>> Create(
        [FromServices] CreateDepartmentHandler handler,
        [FromBody] CreateDepartmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateDepartmentCommand(
            request.Name,
            request.Slug,
            request.ParentId,
            request.Locations);

        return await handler.Handle(command, cancellationToken);
    }

    [HttpPatch("{id}/locations/{locationId}")]
    public async Task<EndpointResult<Guid>> AddLocation(
        [FromRoute] Guid id,
        [FromRoute] Guid locationId,
        [FromServices] AddLocationHandler handler,
        CancellationToken cancellationToken = default)
    {
        var command = new AddLocationCommand(id, locationId);

        return await handler.Handle(command, cancellationToken);
    }

    [HttpDelete("{id}/locations/{locationId}")]
    public async Task<EndpointResult<Guid>> RemoveLocation(
       [FromRoute] Guid id,
       [FromRoute] Guid locationId,
       [FromServices] RemoveLocationHandler handler,
       CancellationToken cancellationToken = default)
    {
        var command = new RemoveLocationCommand(id, locationId);

        return await handler.Handle(command, cancellationToken);
    }

    [HttpPatch("{id}/locations")]
    public async Task<EndpointResult<Guid>> UpdateLocations(
       [FromRoute] Guid id,
       [FromServices] UpdateLocationsHandler handler,
       [FromBody] UpdateLocationsRequest request,
       CancellationToken cancellationToken = default)
    {
        var command = new UpdateLocationsCommand(id, request.LocationIds);

        return await handler.Handle(command, cancellationToken);
    }

    [HttpPut("{id}/parent")]
    public async Task<EndpointResult<Guid>> ChangeParent(
        [FromRoute] Guid id,
        [FromServices] ChangeParentHandler handler,
        [FromBody] ChangeParentRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new ChangeParentCommand(id, request.NewParentId);

        return await handler.Handle(command, cancellationToken);
    }
}