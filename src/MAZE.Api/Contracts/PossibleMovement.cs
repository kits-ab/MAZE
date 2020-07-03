using LocationId = System.Int32;

namespace MAZE.Api.Contracts
{
    public class PossibleMovement : Move
    {
        public PossibleMovement(LocationId location, int numberOfPathsToTravel, PathType type)
            : base(location)
        {
            NumberOfPathsToTravel = numberOfPathsToTravel;
            Type = type;
        }

        public int NumberOfPathsToTravel { get; }

        public PathType Type { get; }
    }
}
