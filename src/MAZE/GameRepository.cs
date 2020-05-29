using System;
using GenericDataStructures;
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
                events =>
                {
                    var world = new World();

                    foreach (var @event in events)
                    {
                        @event.ApplyToWorld(world);
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
