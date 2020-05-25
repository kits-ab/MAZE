using System;
using GenericDataStructures;
using Microsoft.Extensions.Hosting;
using GameId = System.String;
using WorldId = System.String;

namespace MAZE.Api
{
    public class GameService
    {
        private readonly IHostEnvironment _environment;
        private readonly WorldSerializer _worldSerializer;
        private readonly GameRepository _gameService;

        public GameService(IHostEnvironment environment, WorldSerializer worldSerializer, GameRepository gameService)
        {
            _environment = environment;
            _worldSerializer = worldSerializer;
            _gameService = gameService;
        }

        public Result<Contracts.Game, NewGameError> NewGame(WorldId worldId)
        {
            var worldFilePath = System.IO.Path.Combine(_environment.ContentRootPath, "Worlds", $"{worldId}.png");

            if (!System.IO.File.Exists(worldFilePath))
            {
                return NewGameError.WorldNotFound;
            }

            Models.World world;
            using (var worldStream = System.IO.File.OpenRead(worldFilePath))
            {
                world = _worldSerializer.Deserialize(worldId, worldStream);
            }

            var newGameId = _gameService.CreateGame(world);

            var result = _gameService.GetGame(newGameId);

            if (result.TryGetSuccessValue(out var newGame))
            {
                return Convert(newGame);
            }
            else
            {
                throw new InvalidOperationException("Game should been created");
            }
        }

        public Result<Contracts.Game, ReadGameError> GetGame(GameId id)
        {
            var result = _gameService.GetGame(id);

            return result.Map<Result<Contracts.Game, ReadGameError>>(
                game => Convert(game),
                readGameError => readGameError);
        }

        private static Contracts.Game Convert(Models.Game game)
        {
            return new Contracts.Game(game.World.Id)
            {
                Id = game.Id,
            };
        }
    }
}
