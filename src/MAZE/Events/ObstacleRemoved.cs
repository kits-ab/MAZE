using System.Linq;
using MAZE.Models;
using ObstacleId = System.Int32;

namespace MAZE.Events
{
    public class ObstacleRemoved : Event
    {
        public ObstacleRemoved(ObstacleId obstacleId)
        {
            ObstacleId = obstacleId;
        }

        public ObstacleId ObstacleId { get; }

        public override void ApplyToWorld(World world)
        {
            var obstacleToRemove = world.Obstacles.Single(obstacle => obstacle.Id == ObstacleId);
            world.Obstacles.Remove(obstacleToRemove);

            foreach (var unblockedPathId in obstacleToRemove.BlockedPathIds)
            {
                var unblockedPath = world.Paths.Single(path => path.Id == unblockedPathId);
                var fromLocation = world.Locations.Single(location => location.Id == unblockedPath.From);
                var toLocation = world.Locations.Single(location => location.Id == unblockedPath.To);

                if (!fromLocation.IsDiscovered)
                {
                    world.DiscoverLocation(fromLocation.Id);
                }

                if (!toLocation.IsDiscovered)
                {
                    world.DiscoverLocation(toLocation.Id);
                }
            }
        }
    }
}
