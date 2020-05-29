using System.Collections.Generic;
using System.Linq;
using ObstacleId = System.Int32;
using PathId = System.Int32;

namespace MAZE.Models
{
    public class Obstacle
    {
        public Obstacle(ObstacleId id, ObstacleType type, IEnumerable<PathId> blockedPathIds)
        {
            Type = type;
            Id = id;
            BlockedPathIds = blockedPathIds.ToList();
        }

        public ObstacleId Id { get; }

        public ObstacleType Type { get; }

        public List<int> BlockedPathIds { get; }

        public bool IsDiscovered { get; set; }
    }
}
