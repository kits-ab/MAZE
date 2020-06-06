using System;
using System.Linq;
using System.Threading.Tasks;
using MAZE.Api.Contracts;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using CharacterId = System.Int32;
using GameId = System.String;
using LocationId = System.Int32;

namespace MAZE.Api.Controllers
{
    [Route("games/{gameId}/[controller]")]
    [ApiController]
    public class CharactersController : ControllerBase
    {
        private readonly CharacterService _characterService;

        public CharactersController(CharacterService characterService)
        {
            _characterService = characterService;
        }

        [HttpGet]
        public IActionResult Get(GameId gameId)
        {
            var result = _characterService.GetCharacters(gameId);

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
        public IActionResult Get(GameId gameId, CharacterId characterId)
        {
            var result = _characterService.GetCharacter(gameId, characterId);

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
        public async Task<IActionResult> Patch(GameId gameId, CharacterId characterId, JsonPatchDocument<Character> patch)
        {
            if (patch.Operations.Count != 1)
            {
                return BadRequest("Only one modification is currently supported");
            }

            var operation = patch.Operations.Single();

            if (operation.OperationType == OperationType.Replace &&
                operation.path.Equals(nameof(Character.Location), StringComparison.InvariantCultureIgnoreCase))
            {
                LocationId locationId;
                try
                {
                    locationId = Convert.ToInt32(operation.value);
                }
                catch
                {
                    return BadRequest("Invalid move to location");
                }

                var moveResult = await _characterService.MoveCharacterAsync(gameId, characterId, locationId);

                return moveResult.Map(
                    () =>
                    {
                        var result = _characterService.GetCharacter(gameId, characterId);
                        return result.Map<IActionResult>(
                            Ok,
                            _ => Conflict("Character was unavailable after movement"));
                    },
                    moveCharacterError =>
                    {
                        return moveCharacterError switch
                        {
                            MoveCharacterError.GameNotFound => NotFound("Game not found"),
                            MoveCharacterError.CharacterNotFound => NotFound("Character not found"),
                            MoveCharacterError.LocationNotFound => NotFound("Location not found"),
                            MoveCharacterError.NoPathBetweenLocations => BadRequest("No path to new location"),
                            MoveCharacterError.PathNotInAStraightLine => BadRequest("No straight path to new location"),
                            MoveCharacterError.BlockedByObstacle => BadRequest("Path blocked by obstacle"),
                            MoveCharacterError.BlockedByCharacter => BadRequest("Path blocked by character"),
                            _ => throw new ArgumentOutOfRangeException(nameof(moveCharacterError), moveCharacterError, null)
                        };
                    });
            }
            else
            {
                return BadRequest("Unsupported operation");
            }
        }
    }
}
