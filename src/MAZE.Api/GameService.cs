using System;
using System.Collections.Generic;
using System.Linq;
using GenericDataStructures;
using MAZE.Events;
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
        private readonly EventRepository _eventRepository;

        public GameService(IHostEnvironment environment, WorldSerializer worldSerializer, GameRepository gameService, EventRepository eventRepository)
        {
            _environment = environment;
            _worldSerializer = worldSerializer;
            _gameService = gameService;
            _eventRepository = eventRepository;
        }

        public Result<Contracts.Game, NewGameError> NewGame(WorldId worldId)
        {
            var worldFilePath = System.IO.Path.Combine(_environment.ContentRootPath, "Worlds", $"{worldId}.png");

            if (!System.IO.File.Exists(worldFilePath))
            {
                return NewGameError.WorldNotFound;
            }

            List<Union<WorldCreated, CharacterAdded>> gameCreationEvents;
            using (var worldStream = System.IO.File.OpenRead(worldFilePath))
            {
                gameCreationEvents = _worldSerializer.Deserialize(worldId, worldStream).ToList();
            }

            var newGameId = _gameService.CreateGame();

            foreach (var @event in gameCreationEvents)
            {
                _eventRepository.AddEvent(newGameId, @event);
            }

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
