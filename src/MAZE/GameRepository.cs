using System;
using System.Linq;
using GenericDataStructures;
using MAZE.Events;
using MAZE.Models;
using GameId = System.String;

namespace MAZE
{
    public class GameRepository
    {
        private static int _gameCounter = 0;

        private readonly EventRepository _eventRepository;

        public GameRepository(EventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public GameId CreateGame()
        {
            var newGameId = _gameCounter.ToString();
            _gameCounter++;

            return newGameId;
        }

        public Result<Game, ReadGameError> GetGame(GameId id)
        {
            var result = _eventRepository.GetEvents(id);

            return result.Map<Result<Game, ReadGameError>>(
                gameEventsToRead =>
                {
                    var gameEvents = gameEventsToRead.ToList();
                    var worldCreatedEvent = gameEvents.First();
                    var world = worldCreatedEvent.Map(
                        worldCreated => worldCreated.World,
                        _ => throw new InvalidOperationException("First event should be world creation"));

                    foreach (var @event in gameEvents.Skip(1))
                    {
                        @event.Switch(
                            _ => throw new InvalidOperationException("Cannot created world more than once"),
                            characterAdded =>
                            {
                                world.Characters.Add(characterAdded.Character);
                                world.DiscoverLocation(characterAdded.Character.LocationId);
                            });
                    }

                    return new Game(id, world);
                },
                readEventsError =>
                {
                    return readEventsError switch
                    {
                        ReadEventsError.GameNotFound => ReadGameError.NotFound,
                        _ => throw new ArgumentOutOfRangeException(nameof(readEventsError), readEventsError, null)
                    };
                });
        }
    }
}
