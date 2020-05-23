using System;
using MAZE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Game = MAZE.Api.Contracts.Game;
using GameId = System.String;

namespace MAZE.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private readonly GameService _gameService;
        private readonly WorldSerializer _worldSerializer;
        private readonly IHostEnvironment _hostEnvironment;

        public GamesController(GameService gameService, WorldSerializer worldSerializer, IHostEnvironment hostEnvironment)
        {
            _gameService = gameService;
            _worldSerializer = worldSerializer;
            _hostEnvironment = hostEnvironment;
        }

        [HttpPost]
        public IActionResult Post(Game game)
        {
            var worldFilePath = System.IO.Path.Combine(_hostEnvironment.ContentRootPath, "Worlds", $"{game.World}.png");

            if (!System.IO.File.Exists(worldFilePath))
            {
                return NotFound($"World {game.World} not found");
            }

            World world;
            using (var worldStream = System.IO.File.OpenRead(worldFilePath))
            {
                world = _worldSerializer.Deserialize(game.World, worldStream);
            }

            var newGameId = _gameService.CreateGame(world);

            var result = _gameService.GetGame(newGameId);

            if (result.TryGetSuccessValue(out var newGame))
            {
                return CreatedAtAction("Get", new { gameId = newGameId }, Convert(newGame));
            }
            else
            {
                throw new InvalidOperationException("Game should been created");
            }
        }

        [HttpGet("{gameId}")]
        public IActionResult Get(GameId gameId)
        {
            var result = _gameService.GetGame(gameId);

            return result.Map<IActionResult>(
                game => Ok(Convert(game)),
                readGameError =>
                {
                    return readGameError switch
                    {
                        ReadGameError.NotFound => NotFound(),
                        _ => throw new ArgumentOutOfRangeException(nameof(readGameError), readGameError, null)
                    };
                });
        }

        private static Game Convert(Models.Game game)
        {
            return new Game(game.World.Id)
            {
                Id = game.Id,
            };
        }
    }
}
