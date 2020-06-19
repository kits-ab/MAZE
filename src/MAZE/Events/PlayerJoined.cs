using System.Linq;
using MAZE.Models;

namespace MAZE.Events
{
    public class PlayerJoined : Event
    {
        public PlayerJoined(string newPlayerName)
        {
            NewPlayerName = newPlayerName;
        }

        public string NewPlayerName { get; }

        public override void ApplyToGame(Game game)
        {
            var takenPlayerIds = game.Players.Select(player => player.Id).ToHashSet();
            var newPlayerId = 0;
            while (takenPlayerIds.Contains(newPlayerId))
            {
                newPlayerId++;
            }

            game.Players.Add(new Player(newPlayerId, NewPlayerName));
        }
    }
}
