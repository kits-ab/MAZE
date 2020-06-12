using MAZE.Models;

namespace MAZE.Events
{
    public class PlayerJoined : Event
    {
        public PlayerJoined(Player newPlayer)
        {
            NewPlayer = newPlayer;
        }

        public Player NewPlayer { get; }

        public override void ApplyToGame(Game game)
        {
            game.Players.Add(NewPlayer);
        }
    }
}
