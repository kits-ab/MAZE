using System;
using System.Linq;
using GenericDataStructures;
using MAZE.Events;
using MAZE.Models;
using GameId = System.String;

namespace MAZE
{
    public class GameService
    {
        private static int _gameCounter = 0;

        private readonly EventRepository _eventRepository;

        public GameService(EventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public GameId CreateGame(World world)
        {
            var newGameId = _gameCounter.ToString();
            _gameCounter++;

            var gameCreatedEvent = new GameCreated(world);

            _eventRepository.AddEvent(newGameId, gameCreatedEvent);

            return newGameId;
        }

        public Result<Game, ReadGameError> GetGame(GameId id)
        {
            var result = _eventRepository.GetEvents(id);

            return result.Map<Result<Game, ReadGameError>>(
                gameEvents =>
                {
                    var gameCreatedEvent = gameEvents.First();

                    return new Game(id, gameCreatedEvent.World);
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
