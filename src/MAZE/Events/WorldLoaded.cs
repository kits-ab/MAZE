using System;
using System.Collections.Generic;
using System.Linq;
using MAZE.Models;
using WorldId = System.String;

namespace MAZE.Events
{
    public class WorldLoaded : Event
    {
        public WorldLoaded(WorldId id, IEnumerable<Location> locations, IEnumerable<Path> paths, IEnumerable<Obstacle> obstacles)
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

        public override void ApplyToWorld(World world)
        {
            if (world.Id != null)
            {
                throw new InvalidOperationException("Cannot reload a world");
            }

            world.Id = Id;
            world.Locations.AddRange(Locations);
            world.Paths.AddRange(Paths);
            world.Obstacles.AddRange(Obstacles);
        }
    }
}
