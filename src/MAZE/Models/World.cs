using System.Collections.Generic;
using System.Linq;
using LocationId = System.Int32;
using WorldId = System.String;

namespace MAZE.Models
{
    public class World
    {
        public World(WorldId id, IEnumerable<Location> locations, IEnumerable<Path> paths, IEnumerable<Obstacle> obstacles)
        {
            Id = id;
            Paths = paths.ToList();
            Locations = locations.ToList();
            Obstacles = obstacles.ToList();
        }

        public WorldId Id { get; }

        public List<Location> Locations { get; }

        public List<Path> Paths { get; }

        public List<Obstacle> Obstacles { get; }

        public List<Character> Characters { get; } = new List<Character>();

        public void DiscoverLocation(LocationId locationId)
        {
            Locations.Single(location => location.Id == locationId).IsDiscovered = true;
            foreach (var neighborLocationIdToDiscover in Paths.Where(path => path.From == locationId && !Obstacles.Any(obstacle => obstacle.BlockedPathIds.Contains(path.Id))).Select(path => path.To))
            {
                if (!Locations.Single(location => location.Id == neighborLocationIdToDiscover).IsDiscovered)
                {
                    DiscoverLocation(neighborLocationIdToDiscover);
                }
            }
        }
    }
}
