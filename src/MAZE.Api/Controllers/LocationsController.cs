using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GameId = System.String;

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
        public async Task<IActionResult> Get(GameId gameId)
        {
            var result = await _locationsService.GetDiscoveredLocationsAsync(gameId);

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
