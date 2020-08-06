using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenericDataStructures;
using MAZE.Api.Contracts;
using MAZE.Events;
using CharacterId = System.Int32;
using GameId = System.String;
using ObstacleId = System.Int32;
using PathId = System.Int32;

namespace MAZE.Api
{
    public class CharacterService
    {
        private readonly GameRepository _gameRepository;
        private readonly EventRepository _eventRepository;
        private readonly GameEventService _gameEventService;
        private readonly AvailableMovementsFactory _availableMovementsFactory;
        private readonly AvailableObstaclesToClearFactory _availableObstaclesToClearFactory;

        public CharacterService(
            GameRepository gameRepository,
            EventRepository eventRepository,
            GameEventService gameEventService,
            AvailableMovementsFactory availableMovementsFactory,
            AvailableObstaclesToClearFactory availableObstaclesToClearFactory)
        {
            _gameRepository = gameRepository;
            _eventRepository = eventRepository;
            _gameEventService = gameEventService;
            _availableMovementsFactory = availableMovementsFactory;
            _availableObstaclesToClearFactory = availableObstaclesToClearFactory;
        }

        public async Task<Result<IEnumerable<Character>, ReadGameError>> GetCharactersAsync(GameId gameId)
        {
            var result = await _gameRepository.GetGameAsync(gameId);
            return result.Map(
                game =>
                {
                    var characters = game.World.Characters.Select(character => CreateCharacter(character, game.World));

                    return new Result<IEnumerable<Character>, ReadGameError>(characters);
                },
                readGameError => readGameError);
        }

        public async Task<Result<Character, ReadCharacterError>> GetCharacterAsync(GameId gameId, CharacterId characterId)
        {
            var result = await _gameRepository.GetGameAsync(gameId);
            return result.Map<Result<Character, ReadCharacterError>>(
                game =>
                {
                    var character = game.World.Characters.SingleOrDefault(characterCandidate => characterCandidate.Id == characterId);

                    if (character == null)
                    {
                        return ReadCharacterError.CharacterNotFound;
                    }

                    return CreateCharacter(character, game.World);
                },
                readGameError => ConvertToReadCharacterError(readGameError));
        }

        public async Task<Result<Character, ExecuteActionError>> ExecuteActionAsync(GameId gameId, CharacterId characterId, Union<Move, UsePortal, ClearObstacle> action)
        {
            var result = await _gameRepository.GetGameAndVersionAsync(gameId);
            return await result.Map(
                async gameAndVersion =>
                {
                    var (game, version) = gameAndVersion;

                    var character = game.World.Characters.SingleOrDefault(characterCandidate => characterCandidate.Id == characterId);

                    if (character == null)
                    {
                        return ExecuteActionError.CharacterNotFound;
                    }

                    var createEventResult = action.Map(
                        move => Move(game, character, move.ActionName, move.NumberOfPathsToTravel),
                        usePortal => UsePortal(game, character, usePortal.PortalPath),
                        clearObstacle => ClearObstacle(game, character, clearObstacle.Obstacle));

                    return await createEventResult.Map<Task<Result<Character, ExecuteActionError>>>(
                        async @event =>
                        {
                            await _eventRepository.AddEventAsync(game.Id, @event, version);

                            var changedResources = @event.ApplyAndGetModifiedResources(game);

                            var changedResourceNames = ChangedResourcesResolver.GetResourceNames(changedResources);

                            await _gameEventService.NotifyWorldUpdatedAsync(game.Id, changedResourceNames.ToArray());

                            return CreateCharacter(character, game.World);
                        },
                        error => Task.FromResult(new Result<Character, ExecuteActionError>(error)));
                },
                readGameError => Task.FromResult(new Result<Character, ExecuteActionError>(ConvertToMoveCharacterError(readGameError))));
        }

        private static CharacterClass Convert(Models.CharacterClass characterClass)
        {
            return characterClass switch
            {
                Models.CharacterClass.Mage => CharacterClass.Mage,
                Models.CharacterClass.Rogue => CharacterClass.Rogue,
                Models.CharacterClass.Warrior => CharacterClass.Warrior,
                Models.CharacterClass.Cleric => CharacterClass.Cleric,
                _ => throw new ArgumentOutOfRangeException(nameof(characterClass), characterClass, null)
            };
        }

        private static ReadCharacterError ConvertToReadCharacterError(ReadGameError error)
        {
            return error switch
            {
                ReadGameError.NotFound => ReadCharacterError.GameNotFound,
                _ => throw new ArgumentOutOfRangeException(nameof(error), error, null)
            };
        }

        private static ExecuteActionError ConvertToMoveCharacterError(ReadGameError error)
        {
            return error switch
            {
                ReadGameError.NotFound => ExecuteActionError.GameNotFound,
                _ => throw new ArgumentOutOfRangeException(nameof(error), error, null)
            };
        }

        private static Models.ObstacleType GetObstacleTypeCharacterCanClear(Models.Character character)
        {
            return character.Class switch
            {
                Models.CharacterClass.Mage => Models.ObstacleType.ForceField,
                Models.CharacterClass.Rogue => Models.ObstacleType.Lock,
                Models.CharacterClass.Warrior => Models.ObstacleType.Stone,
                Models.CharacterClass.Cleric => Models.ObstacleType.Ghost,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static Models.PathType Convert(ActionName actionName)
        {
            return actionName switch
            {
                ActionName.MoveWest => Models.PathType.West,
                ActionName.MoveEast => Models.PathType.East,
                ActionName.MoveNorth => Models.PathType.North,
                ActionName.MoveSouth => Models.PathType.South,
                _ => throw new ArgumentOutOfRangeException(nameof(actionName), actionName, null)
            };
        }

        private Result<Event, ExecuteActionError> Move(Models.Game game, Models.Character character, ActionName actionName, int numberOfPathsToMove)
        {
            var availableMovements =
                _availableMovementsFactory.GetAvailableMovementActions(character.LocationId, game.World);
            if (!availableMovements.Any(movement => movement.ActionName == actionName && movement.NumberOfPathsToTravel == numberOfPathsToMove))
            {
                return ExecuteActionError.NotAnAvailableAction;
            }

            var pathType = Convert(actionName);

            var newLocationId = character.LocationId;

            for (var remainingPathsToTraverse = numberOfPathsToMove; remainingPathsToTraverse > 0; remainingPathsToTraverse--)
            {
                newLocationId = game.World.Paths.Single(path => path.Type == pathType && path.From == newLocationId).To;
            }

            return new CharacterMoved(character.Id, newLocationId);
        }

        private Result<Event, ExecuteActionError> UsePortal(Models.Game game, Models.Character character, PathId pathId)
        {
            var availablePortals =
                _availableMovementsFactory.GetAvailablePortalActions(character.LocationId, game.World);
            if (availablePortals.All(usePortal => usePortal.PortalPath != pathId))
            {
                return ExecuteActionError.NotAnAvailableAction;
            }

            var newLocationId = game.World.Paths.Single(path => path.Id == pathId).To;

            return new CharacterMoved(character.Id, newLocationId);
        }

        private Result<Event, ExecuteActionError> ClearObstacle(Models.Game game, Models.Character character, ObstacleId obstacleId)
        {
            var obstacleTypeCharacterCanClear = GetObstacleTypeCharacterCanClear(character);
            var availableObstacleRemovals = _availableObstaclesToClearFactory.GetAvailableObstaclesToClear(character.LocationId, obstacleTypeCharacterCanClear, game.World);

            if (availableObstacleRemovals.All(obstacleRemoval => obstacleRemoval.Obstacle != obstacleId))
            {
                return ExecuteActionError.NotAnAvailableAction;
            }

            var obstacleCleared = new ObstacleCleared(obstacleId);

            return obstacleCleared;
        }

        private Character CreateCharacter(Models.Character character, Models.World world)
        {
            var movementActions = _availableMovementsFactory.GetAvailableMovementActions(character.LocationId, world);
            var usePortalActions = _availableMovementsFactory.GetAvailablePortalActions(character.LocationId, world);

            var obstacleTypeCharacterCanClear = GetObstacleTypeCharacterCanClear(character);
            var obstacleRemovalActions = _availableObstaclesToClearFactory.GetAvailableObstaclesToClear(character.LocationId, obstacleTypeCharacterCanClear, world);

            var availableActions = movementActions.Cast<IAction>()
                .Concat(usePortalActions)
                .Concat(obstacleRemovalActions);

            return new Character(character.Id, character.LocationId, Convert(character.Class), availableActions);
        }
    }
}
