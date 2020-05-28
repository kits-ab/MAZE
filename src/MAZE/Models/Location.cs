using LocationId = System.Int32;

namespace MAZE.Models
{
    public class Location
    {
        public Location(LocationId id)
        {
            Id = id;
        }

        public LocationId Id { get; }

        public bool IsDiscovered { get; set; }
    }
}
