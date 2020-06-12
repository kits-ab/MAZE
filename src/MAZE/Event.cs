using MAZE.Models;

namespace MAZE
{
    public abstract class Event
    {
        public abstract void ApplyToGame(Game game);
    }
}
