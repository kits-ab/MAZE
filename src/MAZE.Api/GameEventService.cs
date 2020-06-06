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

        public async Task NotifyWorldUpdatedAsync(GameId gameId, params string[] potentiallyChangedResources)
        {
            await _hubContext.Clients.Groups(gameId).SendAsync(nameof(WorldUpdated), new WorldUpdated(potentiallyChangedResources));
        }
    }
}
