using System.Collections.Generic;
using MAZE.Api.Models;

namespace MAZE.Api
{
    public class PathRepository
    {
        public IEnumerable<Path> GetAll()
        {
            return new[]
            {
                new Path(0, 0, 4, PathType.South),
                new Path(1, 1, 2, PathType.East),
                new Path(2, 1, 5, PathType.South),
                new Path(3, 2, 1, PathType.West),
                new Path(4, 2, 3, PathType.East),
                new Path(5, 3, 2, PathType.West),
                new Path(6, 3, 7, PathType.South),
                new Path(7, 4, 0, PathType.North),
                new Path(8, 4, 8, PathType.South),
                new Path(9, 5, 1, PathType.North),
                new Path(10, 5, 6, PathType.East),
                new Path(11, 5, 9, PathType.South),
                new Path(12, 6, 5, PathType.West),
                new Path(13, 6, 10, PathType.South),
                new Path(14, 7, 3, PathType.North),
                new Path(15, 7, 11, PathType.South),
                new Path(16, 8, 4, PathType.North),
                new Path(17, 8, 9, PathType.East),
                new Path(18, 9, 8, PathType.West),
                new Path(19, 9, 10, PathType.East),
                new Path(20, 10, 9, PathType.West),
                new Path(21, 10, 6, PathType.North),
                new Path(22, 11, 7, PathType.North),
                new Path(23, 11, 14, PathType.Portal),
                new Path(24, 12, 8, PathType.North),
                new Path(25, 12, 13, PathType.East),
                new Path(26, 13, 12, PathType.West),
                new Path(27, 13, 14, PathType.East),
                new Path(28, 14, 13, PathType.West),
                new Path(29, 14, 11, PathType.Portal),
            };
        }
    }
}
