using CSharpFunctionalExtensions;
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
    public EndpointResult Update(
        [FromRoute] Guid id,
        [FromBody] UpdatePositionRequest request,
        CancellationToken cancellationToken = default)
    {
        return Result.Success<Error>();
    }

    [HttpDelete("{id}")]
    public EndpointResult Delete([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        return Result.Success<Error>();
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