using System.Collections.Generic;
using System.Linq;
using CharacterId = System.Int32;
using LocationId = System.Int32;

namespace MAZE.Api.Contracts
{
    public class Character
    {
        public Character(CharacterId id, LocationId location, CharacterClass characterClass, IEnumerable<IAction> availableActions)
        {
            Id = id;
            Location = location;
            CharacterClass = characterClass;
            AvailableActions = availableActions.ToList();
        }

        public CharacterId Id { get; }

        public LocationId Location { get; }

        public CharacterClass CharacterClass { get; }

        public List<IAction> AvailableActions { get; }
    }
}
