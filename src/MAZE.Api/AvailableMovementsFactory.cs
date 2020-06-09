using System;
using System.Collections.Generic;
using System.Linq;
using MAZE.Api.Contracts;
using LocationId = System.Int32;

namespace MAZE.Api
{
    public class AvailableMovementsFactory
    {
        public IEnumerable<Movement> GetAvailableMovements(LocationId atLocationId, Models.World world)
        {
            var blockedPathIds = world.Obstacles.SelectMany(obstacle => obstacle.BlockedPathIds).ToHashSet();
            var blockedLocations = world.Characters.Select(character => character.LocationId).ToHashSet();

            var pathsFromOriginalLocation = world.Paths.Where(path => path.From == atLocationId && path.IsDiscovered && !blockedPathIds.Contains(path.Id)).ToList();

            // Add portals
            foreach (var portalPath in pathsFromOriginalLocation.Where(path => path.Type == Models.PathType.Portal))
            {
                if (!blockedLocations.Contains(portalPath.To))
                {
                    yield return new Movement(portalPath.To, 1, PathType.Portal);
                }
            }

            // Add directional movement
            var directionalPathTypes = new[]
            {
                Models.PathType.West,
                Models.PathType.East,
                Models.PathType.North,
                Models.PathType.South,
            };

            foreach (var direction in directionalPathTypes)
            {
                foreach (var path in pathsFromOriginalLocation.Where(pathCandidate => pathCandidate.Type == direction))
                {
                    int? nextLocationId = path.To;
                    var pathsTraversed = 1;
                    while (nextLocationId != null)
                    {
                        yield return new Movement(nextLocationId.Value, pathsTraversed++, Convert(direction));
                        nextLocationId = world.Paths.Where(pathCandidate =>
                                pathCandidate.From == nextLocationId.Value &&
                                pathCandidate.Type == direction &&
                                pathCandidate.IsDiscovered &&
                                !blockedPathIds.Contains(pathCandidate.Id) &&
                                !blockedLocations.Contains(pathCandidate.To))
                            .Select(pathCandidate => (int?)pathCandidate.To)
                            .SingleOrDefault();
                    }
                }
            }
        }

        private static PathType Convert(Models.PathType pathType)
        {
            return pathType switch
            {
                Models.PathType.West => PathType.West,
                Models.PathType.East => PathType.East,
                Models.PathType.North => PathType.North,
                Models.PathType.South => PathType.South,
                Models.PathType.Portal => PathType.Portal,
                _ => throw new ArgumentOutOfRangeException(nameof(pathType), pathType, null)
            };
        }
    }
}
