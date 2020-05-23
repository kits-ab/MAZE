using Microsoft.AspNetCore.Mvc;
using GameId = System.String;

namespace MAZE.Api.Controllers
{
    [Route("game/{gameId}/[controller]")]
    [ApiController]
    public class PathsController : ControllerBase
    {
        private readonly PathService _pathService;

        public PathsController(PathService pathService)
        {
            _pathService = pathService;
        }

        [HttpGet]
        public IActionResult Get(GameId gameId)
        {
            return Ok(_pathService.GetDiscoveredPaths(gameId));
        }
    }
}
