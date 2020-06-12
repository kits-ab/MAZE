using System;
using Microsoft.AspNetCore.Mvc;
using GameId = System.Int32;

namespace MAZE.Api.Controllers
{
    [Route("games/{gameId}/[controller]")]
    [ApiController]
    public class PlayersController : ControllerBase
    {
        private readonly GameService _gameService;

        public PlayersController(GameService gameService)
        {
            _gameService = gameService;
        }

        [HttpGet]
        public IActionResult Get(GameId gameId)
        {
            var result = _gameService.GetPlayers(gameId);

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
