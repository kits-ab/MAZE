using System.Collections.Generic;
using GameId = System.String;

namespace MAZE.Models
{
    public class Game
    {
        public const int MaxNumberOfPlayers = 8;

        public Game(GameId id)
        {
            Id = id;
        }

        public GameId Id { get; }

        public int RandomSeed { get; set; }

        public World World { get; } = new World();

        public List<Player> Players { get; } = new List<Player>();
    }
}
