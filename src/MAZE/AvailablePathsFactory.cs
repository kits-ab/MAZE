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
                foreach (var path in pathsFromOriginalLocation.Where(pathCandidate => pathCandidate.Type == direction))
                {
                    var paths = new List<Path>();
                    int? nextLocationId = path.To;
                    while (nextLocationId != null)
                    {
                        paths.Add(path);
                        nextLocationId = world.Paths.Where(pathCandidate =>
                                pathCandidate.From == nextLocationId.Value &&
                                pathCandidate.Type == direction &&
                                pathCandidate.IsDiscovered &&
                                !blockedPathIds.Contains(pathCandidate.Id) &&
                                !blockedLocations.Contains(pathCandidate.To))
                            .Select(pathCandidate => (int?)pathCandidate.To)
                            .SingleOrDefault();
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
