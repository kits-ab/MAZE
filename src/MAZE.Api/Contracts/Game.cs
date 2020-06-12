using System.ComponentModel.DataAnnotations;
using GameId = System.Int32;
using WorldId = System.String;

namespace MAZE.Api.Contracts
{
    public class Game
    {
        public Game(WorldId? world)
        {
            World = world;
        }

        public GameId? Id { get; set; }

        [RegularExpression("^[a-zA-Z]+$")]
        public WorldId? World { get; set; }
    }
}
