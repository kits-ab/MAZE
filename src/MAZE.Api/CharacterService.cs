using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenericDataStructures;
using MAZE.Api.Contracts;
using MAZE.Events;
using CharacterId = System.Int32;
using GameId = System.String;
using LocationId = System.Int32;
using ObstacleId = System.Int32;

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

        public async Task<Result<Character, ExecuteActionError>> ExecuteActionAsync(GameId gameId, CharacterId characterId, Union<Move, ClearObstacle> action)
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
                        move => MoveCharacter(game, version, character, move.Location),
                        clearObstacle => ClearObstacle(game, version, character, clearObstacle.Obstacle));

                    return await createEventResult.Map<Task<Result<Character, ExecuteActionError>>>(
                        async @event =>
                        {
                            await _eventRepository.AddEventAsync(game.Id, @event, version);

                            var changedResources = @event.ApplyToGame(game);

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

        private Result<Event, ExecuteActionError> MoveCharacter(Models.Game game, long version, Models.Character character, LocationId newLocationId)
        {
            var availableMovements =
                _availableMovementsFactory.GetAvailableMovements(character.LocationId, game.World);
            if (availableMovements.All(movement => movement.Location != newLocationId))
            {
                return ExecuteActionError.NotAnAvailableAction;
            }

            return new CharacterMoved(character.Id, newLocationId);
        }

        private Result<Event, ExecuteActionError> ClearObstacle(Models.Game game, long version, Models.Character character, ObstacleId obstacleId)
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
            var availableMovements = _availableMovementsFactory.GetAvailableMovements(character.LocationId, world);

            var obstacleTypeCharacterCanClear = GetObstacleTypeCharacterCanClear(character);
            var availableObstacleRemovals = _availableObstaclesToClearFactory.GetAvailableObstaclesToClear(character.LocationId, obstacleTypeCharacterCanClear, world);

            var availableActions = availableMovements.Cast<IAction>().Concat(availableObstacleRemovals);

            return new Character(character.Id, character.LocationId, Convert(character.Class), availableActions);
        }
    }
}
