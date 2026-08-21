using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Features.Locations.Delete;
using DirectoryService.Application.Features.Locations.GetById;
using DirectoryService.Application.Locations.Create;
using DirectoryService.Contracts.Requests;
using DirectoryService.Contracts.Responses;
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
    public async Task<EndpointResult<GetLocationResponse>> GetById(
        [FromRoute] Guid id,
        [FromServices] GetLocationByIdHandler handler,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLocationByIdQuery(id);

        return await handler.Handle(query, cancellationToken);
    }

    [HttpGet("top")]
    public EndpointResult GetTop(CancellationToken cancellationToken = default)
    {
        return Result.Success<Error>();
    }

    [HttpPatch("{id}")]
    public async Task<EndpointResult<Guid>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateLocationRequest request,
        [FromServices] UpdateLocationHandler handler,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateLocationCommand(
            id,
            request.Name,
            request.City,
            request.District,
            request.Street,
            request.Structure,
            request.TimeZone);

        return await handler.Handle(command, cancellationToken);
    }

    [HttpDelete("{id}")]
    public async Task<EndpointResult<Guid>> Delete(
        [FromRoute] Guid id,
        [FromServices] DeleteLocationHandler handler,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteLocationCommand(id);

        return await handler.Handle(command, cancellationToken);
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