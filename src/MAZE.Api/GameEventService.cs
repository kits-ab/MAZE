using System.Linq;
using System.Threading.Tasks;
using MAZE.Api.Contracts;
using MAZE.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using GameId = System.String;

namespace MAZE.Api
{
    public class GameEventService
    {
        private readonly IHubContext<GameHub> _hubContext;

        public GameEventService(IHubContext<GameHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyWorldUpdatedAsync(GameId gameId, params string[] changedResources)
        {
            // Characters are always sent due to their available actions are not resolved after each event
            var potentiallyChangedResources = changedResources.Contains("characters")
                ? changedResources
                : changedResources.Concat(new[] { "characters" });
            await _hubContext.Clients.Groups(gameId).SendAsync(nameof(WorldUpdated), new WorldUpdated(potentiallyChangedResources));
        }
    }
}
