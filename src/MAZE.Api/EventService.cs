using System.Threading.Tasks;
using MAZE.Api.Contracts;
using MAZE.Api.Hub;
using Microsoft.AspNetCore.SignalR;
using GameId = System.String;

namespace MAZE.Api
{
    public class EventService
    {
        private readonly IHubContext<GameHub> _hubContext;

        public EventService(IHubContext<GameHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyWorldUpdatedAsync(GameId gameId, params string[] potentiallyChangedResources)
        {
            await _hubContext.Clients.Groups(gameId).SendAsync("WorldUpdated", new WorldUpdated(potentiallyChangedResources));
        }
    }
}
