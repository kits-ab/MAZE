﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GameId = System.String;
using ObstacleId = System.Int32;

namespace MAZE.Api.Controllers
{
    [Route("games/{gameId}/[controller]")]
    [ApiController]
    public class ObstaclesController : ControllerBase
    {
        private readonly ObstacleService _obstacleService;

        public ObstaclesController(ObstacleService obstacleService)
        {
            _obstacleService = obstacleService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(GameId gameId)
        {
            var result = await _obstacleService.GetObstaclesAsync(gameId);

            return result.Map<IActionResult>(
                Ok,
                readGameError =>
                {
                    return readGameError switch
                    {
                        ReadGameError.NotFound => NotFound("Game not found"),
                        _ => throw new ArgumentOutOfRangeException(nameof(readGameError), readGameError, null)
                    };
                });
        }
    }
}
