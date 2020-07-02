using System.Collections.Generic;
using System.Linq;
using GenericDataStructures;
using MAZE.Models;

namespace MAZE.Events
{
    public class CharacterAdded : Event
    {
        public CharacterAdded(Character character)
        {
            Character = character;
        }

        public Character Character { get; }

        public override IEnumerable<Union<Player, Character, Location, Obstacle, Path>> ApplyToGame(Game game)
        {
            game.World.Characters.Add(Character);
            yield return Character;
            if (!game.World.Locations.Single(location => location.Id == Character.LocationId).IsDiscovered)
            {
                foreach (var discoveredResource in Explorer.Discover(Character.LocationId, game.World))
                {
                    yield return discoveredResource.Map<Union<Player, Character, Location, Obstacle, Path>>(
                        location => location,
                        obstacle => obstacle,
                        path => path);
                }
            }
        }
    }
}
