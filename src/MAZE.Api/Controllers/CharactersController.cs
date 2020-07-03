using System;
using System.Linq;
using System.Threading.Tasks;
using GenericDataStructures;
using MAZE.Api.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using CharacterId = System.Int32;
using GameId = System.String;

namespace MAZE.Api.Controllers
{
    [Route("games/{gameId}/[controller]")]
    [ApiController]
    public class CharactersController : ControllerBase
    {
        private readonly CharacterService _characterService;
        private readonly ObstacleService _obstacleService;

        public CharactersController(CharacterService characterService, ObstacleService obstacleService)
        {
            _characterService = characterService;
            _obstacleService = obstacleService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(GameId gameId)
        {
            var result = await _characterService.GetCharactersAsync(gameId);

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

        [HttpGet("{characterId}")]
        public async Task<IActionResult> Get(GameId gameId, CharacterId characterId)
        {
            var result = await _characterService.GetCharacterAsync(gameId, characterId);

            return result.Map<IActionResult>(
                Ok,
                readGameError =>
                {
                    return readGameError switch
                    {
                        ReadCharacterError.GameNotFound => NotFound("Game not found"),
                        ReadCharacterError.CharacterNotFound => NotFound("Character not found"),
                        _ => throw new ArgumentOutOfRangeException(nameof(readGameError), readGameError, null)
                    };
                });
        }

        [HttpPatch("{characterId}")]
        [Authorize]
        public async Task<IActionResult> Patch(GameId gameId, CharacterId characterId, JsonPatchDocument<Character> patch)
        {
            if (patch.Operations.Count != 1)
            {
                return BadRequest("Only one modification is currently supported");
            }

            var operation = patch.Operations.Single();

            if (operation.OperationType == OperationType.Add &&
                operation.path.Equals("ExecutedActions", StringComparison.InvariantCultureIgnoreCase))
            {
                if (!(operation.value is JObject actionJObject))
                {
                    return BadRequest("Invalid operation value");
                }

                if (actionJObject.TryGetValue("actionName", out var actionNameToken))
                {
                    var actionName = actionNameToken.Value<string>();
                    Union<Move, ClearObstacle> action;
                    switch (actionName)
                    {
                        case Move.Name:
                            var move = actionJObject.ToObject<Move>();
                            if (move == null)
                            {
                                return BadRequest("Invalid move data");
                            }

                            action = move;
                            break;

                        case ClearObstacle.Name:
                            var clearObstacle = actionJObject.ToObject<ClearObstacle>();
                            if (clearObstacle == null)
                            {
                                return BadRequest("Invalid clear obstacle data");
                            }

                            action = clearObstacle;
                            break;

                        default:
                            return BadRequest($"{actionName} is not an supported action");
                    }

                    var actionResult = await _characterService.ExecuteActionAsync(gameId, characterId, action);

                    return actionResult.Map(
                        Ok,
                        CreateErrorResponse);
                }
                else
                {
                    return BadRequest("No action name provided");
                }
            }
            else
            {
                return BadRequest("Unsupported operation");
            }
        }

        private IActionResult CreateErrorResponse(ExecuteActionError error)
        {
            return error switch
            {
                ExecuteActionError.GameNotFound => NotFound("Game not found"),
                ExecuteActionError.CharacterNotFound => NotFound("Character not found"),
                ExecuteActionError.NotAnAvailableAction => BadRequest("Not an available action"),
                _ => throw new ArgumentOutOfRangeException(nameof(error), error, null)
            };
        }
    }
}
