using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.ChangeParent;
using DirectoryService.Application.Departments.Create;
using DirectoryService.Application.Departments.RemoveLocation;
using DirectoryService.Application.Departments.Update;
using DirectoryService.Application.Departments.UpdateLocations;
using DirectoryService.Application.Features.Departments.AddLocation;
using DirectoryService.Application.Features.Departments.AddPosition;
using DirectoryService.Application.Features.Departments.Delete;
using DirectoryService.Application.Features.Departments.GetById;
using DirectoryService.Application.Features.Departments.RemovePosition;
using DirectoryService.Contracts.Dtos;
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
    public async Task<EndpointResult<DepartmentDto>> GetById(
        [FromRoute] Guid id,
        [FromServices] GetDepartmentByIdHandler handler,
        CancellationToken cancellationToken = default)
    {
        var query = new GetByIdDepartmentQuery(id);

        return await handler.Handle(query, cancellationToken);
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
    public async Task<EndpointResult<Guid>> Delete(
        [FromRoute] Guid id,
        [FromServices] DeleteDepartmentHandler handler,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteDepartmentCommand(id);

        return await handler.Handle(command, cancellationToken);
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

    [HttpPatch("{id}/positions/{positionId}")]
    public async Task<EndpointResult<Guid>> AddPosition(
        [FromRoute] Guid id,
        [FromRoute] Guid positionId,
        [FromServices] AddPositionHandler handler,
        CancellationToken cancellationToken = default)
    {
        var command = new AddPositionCommand(id, positionId);

        return await handler.Handle(command, cancellationToken);
    }

    [HttpDelete("{id}/positions/{positionId}")]
    public async Task<EndpointResult<Guid>> RemovePosition(
       [FromRoute] Guid id,
       [FromRoute] Guid positionId,
       [FromServices] RemovePositionHandler handler,
       CancellationToken cancellationToken = default)
    {
        var command = new RemovePositionCommand(id, positionId);

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