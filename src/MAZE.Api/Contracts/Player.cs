using PlayerId = System.Int32;

namespace MAZE.Api.Contracts
{
    public class Player
    {
        public Player(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public PlayerId Id { get; }

        public string Name { get; }
    }
}
