using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GameId = System.Int32;
using ObstacleId = System.Int32;

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

        [HttpDelete("{obstacleId}")]
        public async Task<IActionResult> Delete(GameId gameId, ObstacleId obstacleId)
        {
            var result = await _obstacleService.RemoveObstacleAsync(gameId, obstacleId);

            return result.Map<IActionResult>(
                NoContent,
                error =>
                {
                    return error switch
                    {
                        RemoveObstacleError.GameNotFound => NotFound("Game not found"),
                        RemoveObstacleError.ObstacleNotFound => NotFound("Obstacle not found"),
                        _ => throw new ArgumentOutOfRangeException(nameof(error), error, null)
                    };
                });
        }
    }
}
