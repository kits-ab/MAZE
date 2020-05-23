using LocationId = System.Int32;

namespace MAZE.Models
{
    public class Location
    {
        public Location(LocationId id, bool isDiscovered)
        {
            Id = id;
            IsDiscovered = isDiscovered;
        }

        public LocationId Id { get; }

        public bool IsDiscovered { get; }
    }
}
