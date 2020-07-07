using System.Collections.Generic;
using System.Threading.Tasks;
using GenericDataStructures;
using MAZE.Models;
using GameId = System.String;

namespace MAZE
{
    public class GameRepository
    {
        private readonly EventRepository _eventRepository;

        public GameRepository(EventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<Result<(Game Game, long Version), ReadGameError>> GetGameAndVersionAsync(GameId gameId)
        {
            var result = await _eventRepository.GetEventsAndVersionAsync(gameId);

            return result.Map<Result<(Game Game, long Version), ReadGameError>>(
                eventsAndVersion =>
                {
                    var (events, version) = eventsAndVersion;
                    return (CreateGame(gameId, events), version);
                },
                readGameError => readGameError);
        }

        public async Task<Result<Game, ReadGameError>> GetGameAsync(GameId gameId)
        {
            var result = await _eventRepository.GetEventsAsync(gameId);

            return result.Map<Result<Game, ReadGameError>>(
                events => CreateGame(gameId, events),
                readGameError => readGameError);
        }

        private static Game CreateGame(GameId gameId, IEnumerable<Event> events)
        {
            var game = new Game(gameId);

            foreach (var @event in events)
            {
                @event.Apply(game);
            }

            return game;
        }
    }
}
