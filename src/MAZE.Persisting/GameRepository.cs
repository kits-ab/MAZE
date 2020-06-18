using GenericDataStructures;
using MAZE.Models;
using GameId = System.Int32;

namespace MAZE
{
    public class GameRepository
    {
        private static int _gameCounter;

        private readonly EventRepository _eventRepository;

        public GameRepository(EventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public GameId CreateGame()
        {
            var newGameId = _gameCounter;
            _gameCounter++;

            return newGameId;
        }

        public Result<Game, ReadGameError> GetGame(GameId id)
        {
            var result = _eventRepository.GetEvents(id);

            return result.Map<Result<Game, ReadGameError>>(
                events =>
                {
                    var game = new Game(id);

                    foreach (var @event in events)
                    {
                        @event.ApplyToGame(game);
                    }

                    return game;
                },
                readGameError => readGameError);
        }
    }
}
