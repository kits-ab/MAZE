using LocationId = System.Int32;

namespace MAZE.Api.Contracts
{
    public class Move : IAction
    {
        public const string Name = "move";

        public Move(LocationId location)
        {
            Location = location;
        }

        public string ActionName => Name;

        public LocationId Location { get; }
    }
}
