using System;
using System.Collections.Generic;
using MAZE.Api.Contracts;
using LocationId = System.Int32;

namespace MAZE.Api
{
    public class AvailableMovementsFactory
    {
        public IEnumerable<Movement> GetAvailableMovements(LocationId atLocationId, Models.World world)
        {
            foreach (var paths in AvailablePathsFactory.GetAvailablePaths(atLocationId, world))
            {
                var pathDistance = 1;
                foreach (var path in paths)
                {
                    yield return new Movement(path.To, pathDistance, Convert(path.Type));
                    pathDistance++;
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
