﻿using System;
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
        private readonly GameEventService _gameEventService;

        public GameService(
            IHostEnvironment environment,
            WorldSerializer worldSerializer,
            GameRepository gameRepository,
            EventRepository eventRepository,
            GameEventService gameEventService)
        {
            _environment = environment;
            _worldSerializer = worldSerializer;
            _gameRepository = gameRepository;
            _eventRepository = eventRepository;
            _gameEventService = gameEventService;
        }

        public async Task<Result<Game, NewGameError>> NewGameAsync(WorldId worldId)
        {
            var worldFilePath = System.IO.Path.Combine(_environment.ContentRootPath, "Worlds", $"{worldId}.png");

            if (!System.IO.File.Exists(worldFilePath))
            {
                return NewGameError.WorldNotFound;
            }

            List<Event> worldCreationEvents;
            await using (var worldStream = System.IO.File.OpenRead(worldFilePath))
            {
                worldCreationEvents = _worldSerializer.Deserialize(worldId, worldStream).ToList();
            }

            var newGameId = Guid.NewGuid().ToString();

            var gameCreationEvents = worldCreationEvents.Prepend(new RandomSeedSet(new Random().Next()));

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

        public async Task<Result<Player, JoinGameError>> JoinGameAsync(GameId gameId, string playerName)
        {
            var result = await _gameRepository.GetGameAndVersionAsync(gameId);
            return await result.Map<Task<Result<Player, JoinGameError>>>(
                async gameAndVersion =>
                {
                    var (game, version) = gameAndVersion;

                    if (game.Players.Count >= Models.Game.MaxNumberOfPlayers)
                    {
                        return JoinGameError.GameFull;
                    }

                    var playerJoined = new PlayerJoined(playerName);
                    await _eventRepository.AddEventAsync(gameId, playerJoined, version);

                    var changedResources = playerJoined.ApplyAndGetModifiedResources(game);
                    var changedResourceNames = ChangedResourcesResolver.GetResourceNames(changedResources);

                    await _gameEventService.NotifyWorldUpdatedAsync(game.Id, changedResourceNames.ToArray());

                    return Convert(game.Players.Last());
                },
                readGameError =>
                {
                    return readGameError switch
                    {
                        ReadGameError.NotFound => Task.FromResult(new Result<Player, JoinGameError>(JoinGameError.GameNotFound)),
                        _ => throw new ArgumentOutOfRangeException(nameof(readGameError), readGameError, null)
                    };
                });
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

                    var changedResources = playerLeft.ApplyAndGetModifiedResources(game);
                    var changedResourceNames = ChangedResourcesResolver.GetResourceNames(changedResources);

                    await _gameEventService.NotifyWorldUpdatedAsync(game.Id, changedResourceNames.ToArray());

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

        private static Game Convert(Models.Game game)
        {
            return new Game(game.World.Id)
            {
                Id = game.Id,
            };
        }

        private static Player Convert(Models.Player player)
        {
            return new Player(player.Id, player.Name, player.Actions.Select(Convert));
        }

        private static ActionName Convert(Models.ActionName actionName)
        {
            return actionName switch
            {
                Models.ActionName.MoveWest => ActionName.MoveWest,
                Models.ActionName.MoveEast => ActionName.MoveEast,
                Models.ActionName.MoveNorth => ActionName.MoveNorth,
                Models.ActionName.MoveSouth => ActionName.MoveSouth,
                Models.ActionName.UsePortal => ActionName.UsePortal,
                Models.ActionName.ClearObstacle => ActionName.ClearObstacle,
                Models.ActionName.Teleport => ActionName.Teleport,
                Models.ActionName.Disarm => ActionName.Disarm,
                Models.ActionName.Smash => ActionName.Smash,
                Models.ActionName.Heal => ActionName.Heal,
                _ => throw new ArgumentOutOfRangeException(nameof(actionName), actionName, null)
            };
        }
    }
}
