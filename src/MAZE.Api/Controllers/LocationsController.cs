using System;
using Microsoft.AspNetCore.Mvc;
using GameId = System.Int32;

namespace MAZE.Api.Controllers
{
    [Route("games/{gameId}/[controller]")]
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
            var result = _locationsService.GetDiscoveredLocations(gameId);

            return result.Map<IActionResult>(
                Ok,
                readGameError =>
                {
                    return readGameError switch
                    {
                        ReadGameError.NotFound => NotFound("Game not found"),
                        _ => throw new ArgumentOutOfRangeException(nameof(readGameError), readGameError, null)
                    };
                });
        }
    }
}
