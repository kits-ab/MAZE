using LocationId = System.Int32;

namespace MAZE.Api.Contracts
{
    public class Location
    {
        public Location(LocationId id)
        {
            Id = id;
        }

        public LocationId Id { get; }
    }
}
