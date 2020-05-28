using CharacterId = System.Int32;
using LocationId = System.Int32;

namespace MAZE.Models
{
    public class Character
    {
        public Character(CharacterId id, CharacterClass @class, LocationId locationId)
        {
            Id = id;
            Class = @class;
            LocationId = locationId;
        }

        public CharacterId Id { get; }

        public CharacterClass Class { get; }

        public LocationId LocationId { get; }
    }
}
