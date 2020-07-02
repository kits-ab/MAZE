using System;
using System.Collections.Generic;
using System.Linq;
using GenericDataStructures;
using MAZE.Models;
using LocationId = System.Int32;

namespace MAZE
{
    public static class Explorer
    {
        public static IEnumerable<Union<Location, Obstacle, Path>> Discover(LocationId locationId, World world)
        {
            var location = world.Locations.Single(locationCandidate => locationCandidate.Id == locationId);
            if (location.IsDiscovered)
            {
                throw new ArgumentException($"Location {locationId} is already discovered");
            }

            location.IsDiscovered = true;
            yield return location;
            foreach (var pathToDiscover in world.Paths.Where(path => !path.IsDiscovered && (path.From == locationId || path.To == locationId)))
            {
                pathToDiscover.IsDiscovered = true;
                yield return pathToDiscover;
                foreach (var obstacleToDiscover in world.Obstacles.Where(obstacle =>
                    !obstacle.IsDiscovered && obstacle.BlockedPathIds.Contains(pathToDiscover.Id)))
                {
                    obstacleToDiscover.IsDiscovered = true;
                    yield return obstacleToDiscover;
                }
            }

            foreach (var neighborLocationIdToDiscover in world.Paths.Where(path => path.From == locationId && path.Type != PathType.Portal && !world.Obstacles.Any(obstacle => obstacle.BlockedPathIds.Contains(path.Id))).Select(path => path.To))
            {
                if (!world.Locations.Single(neighborLocationCandidate => neighborLocationCandidate.Id == neighborLocationIdToDiscover).IsDiscovered)
                {
                    foreach (var discoveredResource in Discover(neighborLocationIdToDiscover, world))
                    {
                        yield return discoveredResource;
                    }
                }
            }
        }
    }
}
