using System;
using System.Collections.Generic;
using MAZE.Api.Contracts;
using LocationId = System.Int32;

namespace MAZE.Api
{
    public class AvailableMovementsFactory
    {
        public IEnumerable<Move> GetAvailableMovementActions(LocationId atLocationId, Models.World world)
        {
            foreach (var paths in AvailablePathsFactory.GetAvailablePaths(atLocationId, world))
            {
                var pathDistance = 1;
                foreach (var path in paths)
                {
                    if (path.Type == Models.PathType.West ||
                        path.Type == Models.PathType.East ||
                        path.Type == Models.PathType.North ||
                        path.Type == Models.PathType.South)
                    {
                        yield return new Move(Convert(path.Type), pathDistance);
                    }

                    pathDistance++;
                }
            }
        }

        public IEnumerable<UsePortal> GetAvailablePortalActions(LocationId atLocationId, Models.World world)
        {
            foreach (var paths in AvailablePathsFactory.GetAvailablePaths(atLocationId, world))
            {
                foreach (var path in paths)
                {
                    if (path.Type == Models.PathType.Portal)
                    {
                        yield return new UsePortal(path.Id);
                    }
                }
            }
        }

        private static ActionName Convert(Models.PathType pathType)
        {
            return pathType switch
            {
                Models.PathType.West => ActionName.MoveWest,
                Models.PathType.East => ActionName.MoveEast,
                Models.PathType.North => ActionName.MoveNorth,
                Models.PathType.South => ActionName.MoveSouth,
                _ => throw new ArgumentOutOfRangeException(nameof(pathType), pathType, null)
            };
        }
    }
}
