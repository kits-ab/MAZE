using System.Collections.Generic;
using GenericDataStructures;
using MAZE.Models;

namespace MAZE.Events
{
    public class RandomSeedSet : Event
    {
        public RandomSeedSet(int randomSeed)
        {
            RandomSeed = randomSeed;
        }

        public int RandomSeed { get; }

        public override IEnumerable<Union<Player, Character, Location, Obstacle, Path>> ApplyAndGetModifiedResources(Game game)
        {
            game.RandomSeed = RandomSeed;

            return new Union<Player, Character, Location, Obstacle, Path>[] { };
        }
    }
}
