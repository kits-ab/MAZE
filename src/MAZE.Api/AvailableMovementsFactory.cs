using System.Collections.Generic;
using MAZE.Api.Contracts;
using LocationId = System.Int32;

namespace MAZE.Api
{
    public class AvailableMovementsFactory
    {
        public IEnumerable<Movement> GetAvailableMovements(LocationId atLocationId, Models.World world)
        {
            yield return new Movement(0, 1, PathType.East);
            yield return new Movement(3, 5, PathType.East);
        }
    }
}
