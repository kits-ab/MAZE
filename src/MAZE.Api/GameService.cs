using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenericDataStructures;
using MAZE.Api.Contracts;
using MAZE.Events;
using Microsoft.Extensions.Hosting;
using GameId = System.String;
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

        public async Task<Result<Game, NewGameError>> NewGameAsync(WorldId worldId)
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

            var newGameId = Guid.NewGuid().ToString();

            await _eventRepository.AddEventsAsync(newGameId, gameCreationEvents);

            var result = await _gameRepository.GetGameAsync(newGameId);

            if (result.TryGetSuccessValue(out var game))
            {
                return Convert(game);
            }
            else
            {
                throw new InvalidOperationException("Game should been created");
            }
        }

        public async Task<Result<Game, ReadGameError>> GetGameAsync(GameId id)
        {
            var result = await _gameRepository.GetGameAsync(id);

            return result.Map<Result<Game, ReadGameError>>(
                game => Convert(game),
                readGameError => readGameError);
        }

        public async Task<Result<Player, ReadGameError>> JoinGameAsync(GameId gameId, string playerName)
        {
            var result = await _gameRepository.GetGameAndVersionAsync(gameId);
            return await result.Map<Task<Result<Player, ReadGameError>>>(
                async gameAndVersion =>
                {
                    var (game, version) = gameAndVersion;
                    var playerJoined = new PlayerJoined(playerName);
                    var changedResources = playerJoined.ApplyToGame(game);
                    await _eventRepository.AddEventAsync(gameId, playerJoined, version);

                    return Convert(game.Players.Last());
                },
                readGameError => Task.FromResult(new Result<Player, ReadGameError>(readGameError)));
        }

        public async Task<Result<IEnumerable<Player>, ReadGameError>> GetPlayersAsync(GameId gameId)
        {
            var result = await _gameRepository.GetGameAsync(gameId);
            return result.Map(
                game =>
                {
                    var players = game.Players.Select(Convert);

                    return new Result<IEnumerable<Contracts.Player>, ReadGameError>(players);
                },
                readGameError => readGameError);
        }

        public async Task<VoidResult<LeaveGameError>> LeaveGameAsync(GameId gameId, PlayerId playerId)
        {
            var result = await _gameRepository.GetGameAndVersionAsync(gameId);

            return await result.Map(
                async gameAndVersion =>
                {
                    var (game, version) = gameAndVersion;
                    if (game.Players.All(player => player.Id != playerId))
                    {
                        return LeaveGameError.PlayerNotFound;
                    }

                    var playerLeft = new PlayerLeft(playerId);
                    await _eventRepository.AddEventAsync(gameId, playerLeft, version);
                    return VoidResult<LeaveGameError>.Success;
                },
                readGameError =>
                {
                    return readGameError switch
                    {
                        ReadGameError.NotFound => Task.FromResult(new VoidResult<LeaveGameError>(LeaveGameError.GameNotFound)),
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
