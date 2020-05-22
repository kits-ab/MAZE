using LocationId = System.Int32;
using PathId = System.Int32;

namespace MAZE.Api.Models
{
    public class Path
    {
        public Path(PathId id, LocationId from, LocationId to, PathType type)
        {
            Id = id;
            From = from;
            To = to;
            Type = type;
        }

        public PathId Id { get; }

        public LocationId From { get; }

        public LocationId To { get; }

        public PathType Type { get; }
    }
}
