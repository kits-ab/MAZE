using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenericDataStructures;
using MAZE.Api.Contracts;
using MAZE.Events;
using GameId = System.String;
using ObstacleId = System.Int32;

namespace MAZE.Api
{
    public class ObstacleService
    {
        private readonly GameRepository _gameRepository;
        private readonly EventRepository _eventRepository;
        private readonly GameEventService _eventService;

        public ObstacleService(GameRepository gameRepository, EventRepository eventRepository, GameEventService eventService)
        {
            _gameRepository = gameRepository;
            _eventRepository = eventRepository;
            _eventService = eventService;
        }

        public async Task<Result<IEnumerable<Obstacle>, ReadGameError>> GetObstaclesAsync(GameId gameId)
        {
            var result = await _gameRepository.GetGameAsync(gameId);
            return result.Map(
                game =>
                {
                    var visibleObstacles = game.World.Obstacles
                        .Where(obstacle => obstacle.IsDiscovered)
                        .Select(Convert);

                    return new Result<IEnumerable<Obstacle>, ReadGameError>(visibleObstacles);
                },
                readGameError => readGameError);
        }

        public async Task<VoidResult<RemoveObstacleError>> RemoveObstacleAsync(GameId gameId, ObstacleId obstacleId)
        {
            var result = await _gameRepository.GetGameAndVersionAsync(gameId);
            return await result.Map(
                async gameAndVersion =>
                {
                    var (game, version) = gameAndVersion;
                    if (!game.World.Obstacles.Exists(obstacle => obstacle.Id == obstacleId))
                    {
                        return RemoveObstacleError.ObstacleNotFound;
                    }

                    var obstacleRemoved = new ObstacleRemoved(obstacleId);

                    await _eventRepository.AddEventAsync(gameId, obstacleRemoved, version);

                    var changedResources = obstacleRemoved.ApplyToGame(game);

                    var changedResourceNames = ChangedResourcesResolver.GetResourceNames(changedResources);

                    await _eventService.NotifyWorldUpdatedAsync(gameId, changedResourceNames.ToArray());

                    return VoidResult<RemoveObstacleError>.Success;
                },
                readGameError => Task.FromResult(new VoidResult<RemoveObstacleError>(Convert(readGameError))));
        }

        private static Obstacle Convert(Models.Obstacle obstacle)
        {
            return new Obstacle(obstacle.Id, Convert(obstacle.Type), obstacle.BlockedPathIds);
        }

        private static ObstacleType Convert(Models.ObstacleType obstacleType)
        {
            return obstacleType switch
            {
                Models.ObstacleType.ForceField => ObstacleType.ForceField,
                Models.ObstacleType.Lock => ObstacleType.Lock,
                Models.ObstacleType.Stone => ObstacleType.Stone,
                Models.ObstacleType.Ghost => ObstacleType.Ghost,
                _ => throw new ArgumentOutOfRangeException(nameof(obstacleType), obstacleType, null)
            };
        }

        private static RemoveObstacleError Convert(ReadGameError error)
        {
            return error switch
            {
                ReadGameError.NotFound => RemoveObstacleError.GameNotFound,
                _ => throw new ArgumentOutOfRangeException(nameof(error), error, null)
            };
        }
    }
}
