using System;
using Microsoft.AspNetCore.Mvc;
using Game = MAZE.Api.Contracts.Game;
using GameId = System.Int32;

namespace MAZE.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private readonly GameService _gameService;

        public GamesController(GameService gameService)
        {
            _gameService = gameService;
        }

        [HttpPost]
        public IActionResult Post(Game game)
        {
            if (game.World == null)
            {
                return BadRequest("A world is required when creating a game");
            }

            var result = _gameService.NewGame(game.World);

            return result.Map<IActionResult>(
                newGame =>
                {
                    return CreatedAtAction("Get", new { gameId = newGame.Id }, newGame);
                },
                newGameError =>
                {
                    return newGameError switch
                    {
                        NewGameError.WorldNotFound => NotFound($"World {game.World} not found"),
                        _ => throw new ArgumentOutOfRangeException(nameof(newGameError), newGameError, null)
                    };
                });
        }

        [HttpGet("{gameId}")]
        public IActionResult Get(GameId gameId)
        {
            var result = _gameService.GetGame(gameId);

            return result.Map<IActionResult>(
                Ok,
                readGameError =>
                {
                    return readGameError switch
                    {
                        ReadGameError.NotFound => NotFound(),
                        _ => throw new ArgumentOutOfRangeException(nameof(readGameError), readGameError, null)
                    };
                });
        }
    }
}
