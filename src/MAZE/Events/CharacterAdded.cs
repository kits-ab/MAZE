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

        public override void ApplyToWorld(World world)
        {
            world.Characters.Add(Character);
            if (!world.Locations.Single(location => location.Id == Character.LocationId).IsDiscovered)
            {
                world.DiscoverLocation(Character.LocationId);
            }
        }
    }
}
