using System.Collections.Generic;
using System.Linq;
using MAZE.Models;
using LocationId = System.Int32;

namespace MAZE
{
    public static class AvailablePathsFactory
    {
        public static IEnumerable<IEnumerable<Path>> GetAvailablePaths(LocationId atLocationId, World world)
        {
            var blockedPathIds = world.Obstacles.SelectMany(obstacle => obstacle.BlockedPathIds).ToHashSet();
            var blockedLocations = world.Characters.Select(character => character.LocationId).ToHashSet();

            var pathsFromOriginalLocation = world.Paths.Where(path => path.From == atLocationId && path.IsDiscovered && !blockedPathIds.Contains(path.Id) && !blockedLocations.Contains(path.To)).ToList();

            // Add portals
            foreach (var portalPath in pathsFromOriginalLocation.Where(path => path.Type == PathType.Portal))
            {
                if (!blockedLocations.Contains(portalPath.To))
                {
                    yield return new[] { portalPath };
                }
            }

            // Add directional movement
            var directionalPathTypes = new[]
            {
                PathType.West,
                PathType.East,
                PathType.North,
                PathType.South,
            };

            foreach (var direction in directionalPathTypes)
            {
                foreach (var initialPath in pathsFromOriginalLocation.Where(path => path.Type == direction))
                {
                    var paths = new List<Path>();
                    var nextPath = initialPath;
                    while (nextPath != null)
                    {
                        paths.Add(nextPath);
                        nextPath = world.Paths.SingleOrDefault(pathCandidate =>
                            pathCandidate.From == nextPath.To &&
                            pathCandidate.Type == direction &&
                            pathCandidate.IsDiscovered &&
                            !blockedPathIds.Contains(pathCandidate.Id) &&
                            !blockedLocations.Contains(pathCandidate.To));
                    }

                    if (paths.Any())
                    {
                        yield return paths;
                    }
                }
            }
        }
    }
}
