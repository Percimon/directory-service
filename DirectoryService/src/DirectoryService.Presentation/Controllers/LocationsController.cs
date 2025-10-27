using DirectoryService.Application.Locations.Create;
using DirectoryService.Contracts;
using DirectoryService.Presentation.EndpointResults;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers
{
    [ApiController]
    [Route("api/locations")]
    public class LocationsController : Controller
    {
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
}