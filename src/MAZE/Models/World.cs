using System.Collections.Generic;
using WorldId = System.String;

namespace MAZE.Models
{
    public class World
    {
        public WorldId? Id { get; set; }

        public List<Location> Locations { get; } = new List<Location>();

        public List<Path> Paths { get; } = new List<Path>();

        public List<Obstacle> Obstacles { get; } = new List<Obstacle>();

        public List<Character> Characters { get; } = new List<Character>();
    }
}
