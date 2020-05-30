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
        private readonly EventService _eventService;

        public ObstacleService(GameRepository gameRepository, EventRepository eventRepository, EventService eventService)
        {
            _gameRepository = gameRepository;
            _eventRepository = eventRepository;
            _eventService = eventService;
        }

        public Result<IEnumerable<Obstacle>, ReadGameError> GetObstacles(GameId gameId)
        {
            var result = _gameRepository.GetGame(gameId);
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

        public async Task<VoidResult<ReadGameError>> RemoveObstacleAsync(string gameId, ObstacleId obstacleId)
        {
            var result = _gameRepository.GetGame(gameId);
            return await result.Map(
                async game =>
                {
                    _eventRepository.AddEvent(gameId, new ObstacleRemoved(obstacleId));

                    await _eventService.NotifyWorldUpdatedAsync(gameId, "locations", "paths", "obstacles");

                    return VoidResult<ReadGameError>.Success;
                },
                readGameError => Task.FromResult(new VoidResult<ReadGameError>(readGameError)));
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
    }
}
