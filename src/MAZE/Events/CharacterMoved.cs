using System.Collections.Generic;
using System.Linq;
using GenericDataStructures;
using MAZE.Models;
using CharacterId = System.Int32;
using LocationId = System.Int32;

namespace MAZE.Events
{
    public class CharacterMoved : Event
    {
        public CharacterMoved(CharacterId characterId, LocationId newLocationId)
        {
            CharacterId = characterId;
            NewLocationId = newLocationId;
        }

        public int CharacterId { get; }

        public int NewLocationId { get; }

        public override IEnumerable<Union<Player, Character, Location, Obstacle, Path>> ApplyToGame(Game game)
        {
            var characterToMove = game.World.Characters.Single(character => character.Id == CharacterId);
            characterToMove.LocationId = NewLocationId;
            yield return characterToMove;
            var newLocation = game.World.Locations.Single(location => location.Id == NewLocationId);
            if (!newLocation.IsDiscovered)
            {
                foreach (var discoveredResource in Explorer.Discover(NewLocationId, game.World))
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
