using System;
using System.Collections.Generic;
using System.Linq;
using MAZE.Api.Contracts;
using LocationId = System.Int32;

namespace MAZE.Api
{
    public class AvailableObstaclesToClearFactory
    {
        public IEnumerable<ClearObstacle> GetAvailableObstaclesToClear(LocationId atLocationId, Models.ObstacleType obstacleType, Models.World world)
        {
            var connectedPathIds = world.Paths
                .Where(path => path.From == atLocationId)
                .Select(path => path.Id);

            return world.Obstacles
                .Where(obstacle => obstacle.Type == obstacleType && connectedPathIds.Any(pathId => obstacle.BlockedPathIds.Contains(pathId)))
                .Select(obstacle => new ClearObstacle(obstacle.Id));
        }
    }
}
