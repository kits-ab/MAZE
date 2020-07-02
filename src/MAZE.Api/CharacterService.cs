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

namespace MAZE.Api
{
    public class CharacterService
    {
        private readonly GameRepository _gameRepository;
        private readonly EventRepository _eventRepository;
        private readonly GameEventService _gameEventService;
        private readonly AvailableMovementsFactory _availableMovementsFactory;

        public CharacterService(GameRepository gameRepository, EventRepository eventRepository, GameEventService gameEventService, AvailableMovementsFactory availableMovementsFactory)
        {
            _gameRepository = gameRepository;
            _eventRepository = eventRepository;
            _gameEventService = gameEventService;
            _availableMovementsFactory = availableMovementsFactory;
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

        public async Task<Result<Character, MoveCharacterError>> MoveCharacterAsync(GameId gameId, CharacterId characterId, LocationId newLocationId)
        {
            var result = await _gameRepository.GetGameAndVersionAsync(gameId);
            return await result.Map(
                async gameAndVersion =>
                {
                    var (game, version) = gameAndVersion;
                    var character = game.World.Characters.SingleOrDefault(characterCandidate => characterCandidate.Id == characterId);

                    if (character == null)
                    {
                        return MoveCharacterError.CharacterNotFound;
                    }

                    var availableMovements =
                        _availableMovementsFactory.GetAvailableMovements(character.LocationId, game.World);
                    if (availableMovements.All(movement => movement.Location != newLocationId))
                    {
                        return MoveCharacterError.NotAnAvailableMovement;
                    }

                    var characterMoved = new CharacterMoved(characterId, newLocationId);

                    await _eventRepository.AddEventAsync(gameId, characterMoved, version);

                    var changedResources = characterMoved.ApplyToGame(game);

                    var changedResourceNames = ChangedResourcesResolver.GetResourceNames(changedResources);

                    await _gameEventService.NotifyWorldUpdatedAsync(gameId, changedResourceNames.ToArray());

                    return CreateCharacter(character, game.World);
                },
                readGameError => Task.FromResult(new Result<Character, MoveCharacterError>(ConvertToMoveCharacterError(readGameError))));
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

        private static MoveCharacterError ConvertToMoveCharacterError(ReadGameError error)
        {
            return error switch
            {
                ReadGameError.NotFound => MoveCharacterError.GameNotFound,
                _ => throw new ArgumentOutOfRangeException(nameof(error), error, null)
            };
        }

        private Character CreateCharacter(Models.Character character, Models.World world)
        {
            var availableMovements = _availableMovementsFactory.GetAvailableMovements(character.LocationId, world);
            return new Character(character.Id, character.LocationId, Convert(character.Class), availableMovements);
        }
    }
}
