using MAZE.Models;

namespace MAZE.Events
{
    public class GameCreated
    {
        public GameCreated(World world)
        {
            World = world;
        }

        public World World { get; }
    }
}
