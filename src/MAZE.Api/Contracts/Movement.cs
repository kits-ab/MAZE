using LocationId = System.Int32;

namespace MAZE.Api.Contracts
{
    public class Movement
    {
        public Movement(LocationId locationId, int numberOfPathsToTravel, PathType type)
        {
            Location = locationId;
            NumberOfPathsToTravel = numberOfPathsToTravel;
            Type = type;
        }

        public LocationId Location { get; }

        public int NumberOfPathsToTravel { get; }

        public PathType Type { get; }
    }
}
