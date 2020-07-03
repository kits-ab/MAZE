using LocationId = System.Int32;

namespace MAZE.Api.Contracts
{
    public class Movement : IAction
    {
        public Movement(LocationId locationId, int numberOfPathsToTravel, PathType type)
        {
            Location = locationId;
            NumberOfPathsToTravel = numberOfPathsToTravel;
            Type = type;
        }

        public string ActionName => "move";

        public LocationId Location { get; }

        public int NumberOfPathsToTravel { get; }

        public PathType Type { get; }
    }
}
