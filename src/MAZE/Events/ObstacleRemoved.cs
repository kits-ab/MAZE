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

        public override void ApplyToGame(Game game)
        {
            var obstacleToRemove = game.World.Obstacles.Single(obstacle => obstacle.Id == ObstacleId);
            game.World.Obstacles.Remove(obstacleToRemove);

            foreach (var unblockedPathId in obstacleToRemove.BlockedPathIds)
            {
                var unblockedPath = game.World.Paths.Single(path => path.Id == unblockedPathId);
                var fromLocation = game.World.Locations.Single(location => location.Id == unblockedPath.From);
                var toLocation = game.World.Locations.Single(location => location.Id == unblockedPath.To);

                if (!fromLocation.IsDiscovered)
                {
                    game.World.DiscoverLocation(fromLocation.Id);
                }

                if (!toLocation.IsDiscovered)
                {
                    game.World.DiscoverLocation(toLocation.Id);
                }
            }
        }
    }
}
