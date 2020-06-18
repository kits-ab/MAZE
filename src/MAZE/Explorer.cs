using System;
using System.Linq;
using MAZE.Models;
using LocationId = System.Int32;

namespace MAZE
{
    public static class Explorer
    {
        public static void Discover(LocationId locationId, World world)
        {
            var location = world.Locations.Single(locationCandidate => locationCandidate.Id == locationId);
            if (location.IsDiscovered)
            {
                throw new ArgumentException($"Location {locationId} is already discovered");
            }

            location.IsDiscovered = true;
            foreach (var pathToDiscover in world.Paths.Where(path => !path.IsDiscovered && (path.From == locationId || path.To == locationId)))
            {
                pathToDiscover.IsDiscovered = true;
                foreach (var obstacleToDiscover in world.Obstacles.Where(obstacle =>
                    !obstacle.IsDiscovered && obstacle.BlockedPathIds.Contains(pathToDiscover.Id)))
                {
                    obstacleToDiscover.IsDiscovered = true;
                }
            }

            foreach (var neighborLocationIdToDiscover in world.Paths.Where(path => path.From == locationId && !world.Obstacles.Any(obstacle => obstacle.BlockedPathIds.Contains(path.Id))).Select(path => path.To))
            {
                if (!world.Locations.Single(neighborLocationCandidate => neighborLocationCandidate.Id == neighborLocationIdToDiscover).IsDiscovered)
                {
                    Discover(neighborLocationIdToDiscover, world);
                }
            }
        }
    }
}
