using System.Collections.Generic;
using System.Linq;
using GenericDataStructures;
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

        public override IEnumerable<Union<Player, Character, Location, Obstacle, Path>> ApplyAndGetModifiedResources(Game game)
        {
            var takenPlayerIds = game.Players.Select(player => player.Id).ToHashSet();
            var newPlayerId = 0;
            while (takenPlayerIds.Contains(newPlayerId))
            {
                newPlayerId++;
            }

            var newPlayer = new Player(newPlayerId, NewPlayerName);
            game.Players.Add(newPlayer);
            yield return newPlayer;
        }
    }
}
