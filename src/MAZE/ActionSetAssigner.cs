using System.Collections.Generic;
using System.Linq;
using MAZE.Models;

namespace MAZE
{
    public static class ActionSetAssigner
    {
        public static void Assign(
            IEnumerable<IEnumerable<ActionName>> actionSets,
            List<Player> players,
            int randomSeed)
        {
            var startOffset = randomSeed % players.Count;

            var playersToAssignActionsTo = players
                .Skip(startOffset)
                .Concat(players.Take(startOffset))
                .ToList();

            var playerIndexToAssignTo = 0;
            foreach (var actionSet in actionSets)
            {
                var player = playersToAssignActionsTo[playerIndexToAssignTo++];
                player.Actions.Clear();
                player.Actions.AddRange(actionSet);
            }
        }
    }
}
