using MAZE.Models;

namespace MAZE.Events
{
    public class WorldCreated
    {
        public WorldCreated(World world)
        {
            World = world;
        }

        public World World { get; }
    }
}
