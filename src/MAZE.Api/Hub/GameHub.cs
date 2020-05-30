using System.Threading.Tasks;
using MAZE.Api.Contracts;
using Microsoft.AspNetCore.SignalR;
using GameId = System.String;

namespace MAZE.Api.Hub
{
    public class GameHub : Microsoft.AspNetCore.SignalR.Hub
    {
        public async Task Subscribe(GameId gameId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
        }

        public async Task Notify(GameId gameId, WorldUpdated worldUpdated)
        {
            await Clients.Group(gameId).SendAsync("WorldUpdated", worldUpdated);
        }
    }
}
