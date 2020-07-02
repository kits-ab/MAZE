using System.Collections.Generic;
using System.Linq;
using GenericDataStructures;
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

        public override IEnumerable<Union<Player, Character, Location, Obstacle, Path>> ApplyToGame(Game game)
        {
            var obstacleToRemove = game.World.Obstacles.Single(obstacle => obstacle.Id == ObstacleId);
            game.World.Obstacles.Remove(obstacleToRemove);
            yield return obstacleToRemove;

            foreach (var unblockedPathId in obstacleToRemove.BlockedPathIds)
            {
                var unblockedPath = game.World.Paths.Single(path => path.Id == unblockedPathId);
                var fromLocation = game.World.Locations.Single(location => location.Id == unblockedPath.From);
                var toLocation = game.World.Locations.Single(location => location.Id == unblockedPath.To);

                if (!fromLocation.IsDiscovered)
                {
                    foreach (var discoveredResource in Explorer.Discover(fromLocation.Id, game.World))
                    {
                        yield return discoveredResource.Map<Union<Player, Character, Location, Obstacle, Path>>(
                            location => location,
                            obstacle => obstacle,
                            path => path);
                    }
                }

                if (!toLocation.IsDiscovered)
                {
                    foreach (var discoveredResource in Explorer.Discover(toLocation.Id, game.World))
                    {
                        yield return discoveredResource.Map<Union<Player, Character, Location, Obstacle, Path>>(
                            location => location,
                            obstacle => obstacle,
                            path => path);
                    }
                }
            }
        }
    }
}
