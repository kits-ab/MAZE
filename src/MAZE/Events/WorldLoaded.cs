using System;
using System.Collections.Generic;
using System.Linq;
using GenericDataStructures;
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

        public override IEnumerable<Union<Player, Character, Location, Obstacle, Path>> ApplyToGame(Game game)
        {
            if (game.World.Id != null)
            {
                throw new InvalidOperationException("Cannot reload a world");
            }

            game.World.Id = Id;

            game.World.Locations.AddRange(Locations);
            foreach (var location in Locations)
            {
                yield return location;
            }

            game.World.Paths.AddRange(Paths);
            foreach (var path in Paths)
            {
                yield return path;
            }

            game.World.Obstacles.AddRange(Obstacles);
            foreach (var obstacle in Obstacles)
            {
                yield return obstacle;
            }
        }
    }
}
