using System.Collections.Generic;
using GenericDataStructures;
using MAZE.Models;

namespace MAZE
{
    public abstract class Event
    {
        public abstract IEnumerable<Union<Player, Character, Location, Obstacle, Path>> ApplyToGame(Game game);
    }
}
