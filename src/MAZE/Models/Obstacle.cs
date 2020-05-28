using System.Collections.Generic;
using System.Linq;
using PathId = System.Int32;

namespace MAZE.Models
{
    public class Obstacle
    {
        public Obstacle(ObstacleType type, params PathId[] blockedPathIds)
        {
            Type = type;
            BlockedPathIds = blockedPathIds.ToList();
        }

        public ObstacleType Type { get; }

        public List<int> BlockedPathIds { get; }
    }
}
