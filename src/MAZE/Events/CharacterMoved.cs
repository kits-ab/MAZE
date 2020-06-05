using System.Linq;
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

        public override void ApplyToWorld(World world)
        {
            var characterToMove = world.Characters.Single(character => character.Id == CharacterId);
            characterToMove.LocationId = NewLocationId;
        }
    }
}
