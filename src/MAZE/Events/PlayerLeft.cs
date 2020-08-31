using System.Collections.Generic;
using System.Linq;
using GenericDataStructures;
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

        public override IEnumerable<Union<Player, Character, Location, Obstacle, Path>> ApplyAndGetModifiedResources(Game game)
        {
            var playerToRemove = game.Players.Single(player => player.Id == PlayerId);
            game.Players.Remove(playerToRemove);

            // Reassign actions to existing players
            if (game.Players.Any())
            {
                var actionSets = ActionSetsFactory.GetActionSets(game.Players.Count);
                ActionSetAssigner.Assign(actionSets, game.Players, game.RandomSeed);
            }

            yield return playerToRemove;
        }
    }
}
