using PlayerId = System.Int32;

namespace MAZE.Models
{
    public class Player
    {
        public Player(PlayerId id, string name)
        {
            Id = id;
            Name = name;
        }

        public PlayerId Id { get; }

        public string Name { get; }
    }
}
