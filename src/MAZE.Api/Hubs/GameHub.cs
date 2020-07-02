using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MAZE.Api.Contracts;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using GameId = System.String;
using PlayerId = System.Int32;

namespace MAZE.Api.Hubs
{
    public class GameHub : Hub
    {
        private const string JoinedGamesKey = "joined game ids";

        private readonly GameService _gameService;
        private readonly TokenFactory _tokenFactory;
        private readonly ILogger<GameHub> _logger;

        public GameHub(GameService gameService, TokenFactory tokenFactory, ILogger<GameHub> logger)
        {
            _gameService = gameService;
            _tokenFactory = tokenFactory;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            if (Context.GetHttpContext().Request.Query.TryGetValue("gameId", out var gameIds))
            {
                string? playerName = null;
                if (Context.GetHttpContext().Request.Query.TryGetValue("playerName", out var playerNames))
                {
                    playerName = playerNames.First();
                }

                var joinedGames = new List<JoinedGame>();
                foreach (var gameId in gameIds)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
                    if (playerName != null)
                    {
                        var result = await _gameService.JoinGameAsync(gameId, playerName);
                        if (result.TryGetSuccessValue(out var player))
                        {
                            var token = _tokenFactory.CreateJwtToken(gameId, player.Id);
                            await Clients.Caller.SendAsync(nameof(NewToken), new NewToken(token));
                            joinedGames.Add(new JoinedGame(gameId, player.Id));
                        }
                        else
                        {
                            Context.Abort();
                        }
                    }
                }

                if (joinedGames.Any())
                {
                    Context.Items.Add(JoinedGamesKey, joinedGames);
                }
            }
            else
            {
                Context.Abort();
            }

            await Clients.Caller.SendAsync(nameof(WorldUpdated), new WorldUpdated("locations", "paths", "characters", "obstacles", "players"));
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // Leave joined games
            if (Context.Items.ContainsKey(JoinedGamesKey))
            {
                foreach (var joinedGame in (List<JoinedGame>)Context.Items[JoinedGamesKey])
                {
                    var result = await _gameService.LeaveGameAsync(joinedGame.GameId, joinedGame.PlayerId);
                    if (!result.IsSuccess)
                    {
                        _logger.LogWarning("Failed to leave a game the player joined");
                    }
                }
            }
        }

        private class JoinedGame
        {
            public JoinedGame(GameId gameId, PlayerId playerId)
            {
                GameId = gameId;
                PlayerId = playerId;
            }

            public GameId GameId { get; }

            public PlayerId PlayerId { get; }
        }
    }
}
