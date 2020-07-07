using System.Collections.Generic;
using System.Linq;
using GenericDataStructures;
using MAZE.Models;

namespace MAZE
{
    public abstract class Event
    {
        public abstract IEnumerable<Union<Player, Character, Location, Obstacle, Path>> ApplyAndGetModifiedResources(Game game);

        public void Apply(Game game)
        {
            _ = ApplyAndGetModifiedResources(game).ToList();
        }
    }
}
