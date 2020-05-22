using Microsoft.AspNetCore.Mvc;
using GameId = System.String;

namespace MAZE.Api.Controllers
{
    [Route("game/{gameId}/[controller]")]
    [ApiController]
    public class LocationsController : ControllerBase
    {
        private readonly LocationService _locationsService;

        public LocationsController(LocationService locationsService)
        {
            _locationsService = locationsService;
        }

        [HttpGet]
        public IActionResult Get(GameId gameId)
        {
            return Ok(_locationsService.GetDiscoveredLocations());
        }
    }
}
