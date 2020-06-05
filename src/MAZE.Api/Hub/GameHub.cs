using System.Threading.Tasks;
using MAZE.Api.Contracts;
using Microsoft.AspNetCore.SignalR;

namespace MAZE.Api.Hub
{
    public class GameHub : Microsoft.AspNetCore.SignalR.Hub
    {
        public override Task OnConnectedAsync()
        {
            if (Context.GetHttpContext().Request.Query.TryGetValue("gameId", out var gameIdValues))
            {
                foreach (var gameId in gameIdValues)
                {
                    Groups.AddToGroupAsync(Context.ConnectionId, gameId);
                }
            }
            else
            {
                Context.Abort();
            }

            Clients.Caller.SendAsync(nameof(WorldUpdated), new WorldUpdated("locations", "paths", "characters"));

            return base.OnConnectedAsync();
        }
    }
}
