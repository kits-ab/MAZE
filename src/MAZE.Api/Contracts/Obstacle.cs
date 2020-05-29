using System.Collections.Generic;
using System.Linq;
using LocationId = System.Int32;
using ObstacleId = System.Int32;

namespace MAZE.Api.Contracts
{
    public class Obstacle
    {
        public Obstacle(ObstacleId id, ObstacleType type, IEnumerable<LocationId> blockedPaths)
        {
            Id = id;
            Type = type;
            BlockedPaths = blockedPaths.ToList();
        }

        public ObstacleId Id { get; }

        public ObstacleType Type { get; }

        public List<LocationId> BlockedPaths { get; }
    }
}
