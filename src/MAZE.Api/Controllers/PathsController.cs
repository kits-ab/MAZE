using System;
using Microsoft.AspNetCore.Mvc;
using GameId = System.String;

namespace MAZE.Api.Controllers
{
    [Route("games/{gameId}/[controller]")]
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
            var result = _pathService.GetDiscoveredPaths(gameId);

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
