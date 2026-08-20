using CSharpFunctionalExtensions;
using DirectoryService.Application.Features.Positions.Delete;
using DirectoryService.Application.Features.Positions.Rename;
using DirectoryService.Application.Positions.Create;
using DirectoryService.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;
using SharedService.Framework.EndpointResults;
using SharedService.SharedKernel;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
[Route("api/positions")]
public class PositionsController : Controller
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
    public async Task<EndpointResult<Guid>> Rename(
        [FromRoute] Guid id,
        [FromBody] RenamePositionRequest request,
        [FromServices] RenamePositionHandler handler,
        CancellationToken cancellationToken = default)
    {
        return await handler.Handle(new RenamePositionCommand(id, request.Name), cancellationToken);
    }

    [HttpDelete("{id}")]
    public async Task<EndpointResult<Guid>> Delete(
        [FromRoute] Guid id,
        [FromServices] DeletePositionHandler handler,
        CancellationToken cancellationToken = default)
    {
        return await handler.Handle(new DeletePositionCommand(id), cancellationToken);
    }

    [HttpPost]
    public async Task<EndpointResult<Guid>> Create(
        [FromServices] CreatePositionHandler handler,
        [FromBody] CreatePositionRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreatePositionCommand(
            request.Name,
            request.Description,
            request.Departments);

        return await handler.Handle(command, cancellationToken);
    }
}