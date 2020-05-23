using System.Collections.Generic;
using System.Linq;

namespace MAZE.Models
{
    public class World
    {
        public World(IEnumerable<Location> locations, IEnumerable<Path> paths)
        {
            Paths = paths.ToList();
            Locations = locations.ToList();
        }

        public List<Location> Locations { get; }

        public List<Path> Paths { get; }
    }
}
