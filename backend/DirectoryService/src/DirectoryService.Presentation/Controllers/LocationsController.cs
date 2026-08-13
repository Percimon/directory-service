using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations.Create;
using DirectoryService.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;
using SharedService.Framework.EndpointResults;
using SharedService.SharedKernel;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
[Route("api/locations")]
public class LocationsController : Controller
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
        [FromBody] UpdateLocationRequest request,
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
        [FromServices] CreateLocationHandler handler,
        [FromBody] CreateLocationRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateLocationCommand(
            request.Name,
            request.City,
            request.District,
            request.Street,
            request.Structure,
            request.TimeZone);

        return await handler.Handle(command, cancellationToken);
    }
}