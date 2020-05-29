using System;
using Microsoft.AspNetCore.Mvc;
using GameId = System.String;

namespace MAZE.Api.Controllers
{
    [Route("games/{gameId}/[controller]")]
    [ApiController]
    public class ObstaclesController : ControllerBase
    {
        private readonly ObstacleService _obstacleService;

        public ObstaclesController(ObstacleService obstacleService)
        {
            _obstacleService = obstacleService;
        }

        [HttpGet]
        public IActionResult Get(GameId gameId)
        {
            var result = _obstacleService.GetObstacles(gameId);

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
