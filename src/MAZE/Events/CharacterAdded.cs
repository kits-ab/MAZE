using System.Linq;
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

        public override void ApplyToGame(Game game)
        {
            game.World.Characters.Add(Character);
            if (!game.World.Locations.Single(location => location.Id == Character.LocationId).IsDiscovered)
            {
                Explorer.Discover(Character.LocationId, game.World);
            }
        }
    }
}
