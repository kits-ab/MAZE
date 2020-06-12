﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MAZE.Api.Contracts;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using GameId = System.Int32;
using PlayerId = System.Int32;

namespace MAZE.Api.Hubs
{
    public class GameHub : Hub
    {
        private const string JoinedGamesKey = "joined game ids";

        private readonly GameService _gameService;
        private readonly ILogger<GameHub> _logger;

        public GameHub(GameService gameService, ILogger<GameHub> logger)
        {
            _gameService = gameService;
            _logger = logger;
        }

        public override Task OnConnectedAsync()
        {
            if (Context.GetHttpContext().Request.Query.TryGetValue("gameId", out var gameIdsStringValues))
            {
                string? playerName = null;
                if (Context.GetHttpContext().Request.Query.TryGetValue("playerName", out var playerNames))
                {
                    playerName = playerNames.First();
                }

                var joinedGames = new List<JoinedGame>();
                foreach (var gameIdStringValue in gameIdsStringValues)
                {
                    var gameId = int.Parse(gameIdStringValue);
                    Groups.AddToGroupAsync(Context.ConnectionId, gameIdStringValue);
                    if (playerName != null)
                    {
                        var result = _gameService.JoinGame(gameId, playerName);
                        if (result.TryGetSuccessValue(out var player))
                        {
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

            Clients.Caller.SendAsync(nameof(WorldUpdated), new WorldUpdated("locations", "paths", "characters"));

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            // Leave joined games
            if (Context.Items.ContainsKey(JoinedGamesKey))
            {
                foreach (var joinedGame in (List<JoinedGame>)Context.Items[JoinedGamesKey])
                {
                    var result = _gameService.LeaveGame(joinedGame.GameId, joinedGame.PlayerId);
                    if (!result.IsSuccess)
                    {
                        _logger.LogWarning("Failed to leave a game the player joined");
                    }
                }
            }

            return base.OnDisconnectedAsync(exception);
        }

        private class JoinedGame
        {
            public JoinedGame(int gameId, PlayerId playerId)
            {
                GameId = gameId;
                PlayerId = playerId;
            }

            public GameId GameId { get; }

            public PlayerId PlayerId { get; }
        }
    }
}
