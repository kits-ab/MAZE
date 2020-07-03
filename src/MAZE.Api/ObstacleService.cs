using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenericDataStructures;
using MAZE.Api.Contracts;
using GameId = System.String;

namespace MAZE.Api
{
    public class ObstacleService
    {
        private readonly GameRepository _gameRepository;

        public ObstacleService(GameRepository gameRepository)
        {
            _gameRepository = gameRepository;
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
