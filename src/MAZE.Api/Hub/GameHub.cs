using System.Threading.Tasks;
using GameId = System.String;

namespace MAZE.Api.Hub
{
    public class GameHub : Microsoft.AspNetCore.SignalR.Hub
    {
        public async Task Subscribe(GameId gameId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }
    }
}
