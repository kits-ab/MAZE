using LocationId = System.Int32;

namespace MAZE.Api.Models
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
