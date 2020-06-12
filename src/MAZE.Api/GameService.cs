using System;
using System.Collections.Generic;
using System.Linq;
using GenericDataStructures;
using MAZE.Events;
using Microsoft.Extensions.Hosting;
using GameId = System.Int32;
using PlayerId = System.Int32;
using WorldId = System.String;

namespace MAZE.Api
{
    public class GameService
    {
        private readonly IHostEnvironment _environment;
        private readonly WorldSerializer _worldSerializer;
        private readonly GameRepository _gameRepository;
        private readonly EventRepository _eventRepository;

        public GameService(IHostEnvironment environment, WorldSerializer worldSerializer, GameRepository gameRepository, EventRepository eventRepository)
        {
            _environment = environment;
            _worldSerializer = worldSerializer;
            _gameRepository = gameRepository;
            _eventRepository = eventRepository;
        }

        public Result<Contracts.Game, NewGameError> NewGame(WorldId worldId)
        {
            var worldFilePath = System.IO.Path.Combine(_environment.ContentRootPath, "Worlds", $"{worldId}.png");

            if (!System.IO.File.Exists(worldFilePath))
            {
                return NewGameError.WorldNotFound;
            }

            List<Event> gameCreationEvents;
            using (var worldStream = System.IO.File.OpenRead(worldFilePath))
            {
                gameCreationEvents = _worldSerializer.Deserialize(worldId, worldStream).ToList();
            }

            var newGameId = _gameRepository.CreateGame();

            foreach (var @event in gameCreationEvents)
            {
                _eventRepository.AddEvent(newGameId, @event);
            }

            var result = _gameRepository.GetGame(newGameId);

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
            var result = _gameRepository.GetGame(id);

            return result.Map<Result<Contracts.Game, ReadGameError>>(
                game => Convert(game),
                readGameError => readGameError);
        }

        public Result<Contracts.Player, ReadGameError> JoinGame(GameId gameId, string playerName)
        {
            var joinGameEvents = _eventRepository.GetEvents(gameId);
            return joinGameEvents.Map<Result<Contracts.Player, ReadGameError>>(
                gameEvents =>
                {
                    var numberOfJoinGameEvents = gameEvents.Count(gameEvent => gameEvent is PlayerJoined);
                    var newPlayer = new Models.Player(numberOfJoinGameEvents, playerName);
                    var playerJoined = new PlayerJoined(newPlayer);
                    _eventRepository.AddEvent(gameId, playerJoined);
                    return Convert(newPlayer);
                },
                readGameError => readGameError);
        }

        public Result<IEnumerable<Contracts.Player>, ReadGameError> GetPlayers(GameId gameId)
        {
            var result = _gameRepository.GetGame(gameId);
            return result.Map(
                game =>
                {
                    var players = game.Players.Select(Convert);

                    return new Result<IEnumerable<Contracts.Player>, ReadGameError>(players);
                },
                readGameError => readGameError);
        }

        public VoidResult<LeaveGameError> LeaveGame(GameId gameId, PlayerId playerId)
        {
            var result = _gameRepository.GetGame(gameId);

            return result.Map(
                game =>
                {
                    if (game.Players.All(player => player.Id != playerId))
                    {
                        return LeaveGameError.PlayerNotFound;
                    }

                    var playerLeft = new PlayerLeft(playerId);
                    _eventRepository.AddEvent(gameId, playerLeft);
                    return VoidResult<LeaveGameError>.Success;
                },
                readGameError =>
                {
                    return readGameError switch
                    {
                        ReadGameError.NotFound => LeaveGameError.GameNotFound,
                        _ => throw new ArgumentOutOfRangeException(nameof(readGameError), readGameError, null)
                    };
                });
        }

        private static Contracts.Game Convert(Models.Game game)
        {
            return new Contracts.Game(game.World.Id)
            {
                Id = game.Id,
            };
        }

        private static Contracts.Player Convert(Models.Player player)
        {
            return new Contracts.Player(player.Id, player.Name);
        }
    }
}
