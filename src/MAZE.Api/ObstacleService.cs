using System;
using System.Collections.Generic;
using System.Linq;
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

        public ObstacleService(GameRepository gameRepository, EventRepository eventRepository)
        {
            _gameRepository = gameRepository;
            _eventRepository = eventRepository;
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

        public VoidResult<ReadGameError> RemoveObstacle(string gameId, ObstacleId obstacleId)
        {
            var result = _gameRepository.GetGame(gameId);
            return result.Map(
                game =>
                {
                    _eventRepository.AddEvent(gameId, new ObstacleRemoved(obstacleId));

                    return VoidResult<ReadGameError>.Success;
                },
                readGameError => readGameError);
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
