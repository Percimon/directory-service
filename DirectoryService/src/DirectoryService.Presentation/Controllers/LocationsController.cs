using DirectoryService.Application.Locations.Create;
using DirectoryService.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers
{
    [ApiController]
    [Route("api/locations")]
    public class LocationsController : Controller
    {
        [HttpPost]
        public async Task<ActionResult<Guid>> Create(
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

            var result = await handler.Handle(command, cancellationToken);

            //TO DO: result и envelope в следующем задании

            if (result.IsFailure)
            {
                return BadRequest();
            }

            return Ok();
        }
    }
}