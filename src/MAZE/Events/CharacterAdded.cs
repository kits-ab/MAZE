using MAZE.Models;

namespace MAZE.Events
{
    public class CharacterAdded
    {
        public CharacterAdded(Character character)
        {
            Character = character;
        }

        public Character Character { get; }
    }
}
