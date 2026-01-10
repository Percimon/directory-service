using DirectoryService.Application.Locations.Create;
using DirectoryService.Application.Positions.Create;
using DirectoryService.Contracts.Requests;
using DirectoryService.Presentation.EndpointResults;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
[Route("api/positions")]
public class PositionsController : Controller
{
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