using CharacterId = System.Int32;
using LocationId = System.Int32;

namespace MAZE.Api.Contracts
{
    public class Character
    {
        public Character(CharacterId id, LocationId location, CharacterClass characterClass)
        {
            Id = id;
            Location = location;
            CharacterClass = characterClass;
        }

        public CharacterId Id { get; }

        public LocationId Location { get; }

        public CharacterClass CharacterClass { get; }
    }
}
