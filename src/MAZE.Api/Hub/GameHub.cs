using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using GameId = System.String;

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

            return base.OnConnectedAsync();
        }
    }
}
