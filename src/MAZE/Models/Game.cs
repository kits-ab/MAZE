using GameId = System.String;

namespace MAZE.Models
{
    public class Game
    {
        public Game(GameId id, World world)
        {
            Id = id;
            World = world;
        }

        public GameId Id { get; }

        public World World { get; }
    }
}
