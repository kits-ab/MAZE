using System.Linq;
using MAZE.Models;
using PlayerId = System.Int32;

namespace MAZE.Events
{
    public class PlayerLeft : Event
    {
        public PlayerLeft(PlayerId playerId)
        {
            PlayerId = playerId;
        }

        public PlayerId PlayerId { get; }

        public override void ApplyToGame(Game game)
        {
            var playerToRemove = game.Players.Single(player => player.Id == PlayerId);
            game.Players.Remove(playerToRemove);
        }
    }
}
