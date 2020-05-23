using System.Collections.Generic;
using System.Linq;
using WorldId = System.String;

namespace MAZE.Models
{
    public class World
    {
        public World(WorldId id, IEnumerable<Location> locations, IEnumerable<Path> paths)
        {
            Id = id;
            Paths = paths.ToList();
            Locations = locations.ToList();
        }

        public WorldId Id { get; }

        public List<Location> Locations { get; }

        public List<Path> Paths { get; }
    }
}
