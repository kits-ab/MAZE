using System.Collections.Generic;
using GenericDataStructures;
using GameId = System.String;

namespace MAZE
{
    public class EventRepository
    {
        private readonly Dictionary<GameId, List<Event>> _events = new Dictionary<GameId, List<Event>>();

        public Result<IEnumerable<Event>, ReadEventsError> GetEvents(GameId gameId)
        {
            if (!_events.ContainsKey(gameId))
            {
                return ReadEventsError.GameNotFound;
            }

            return _events[gameId];
        }

        public void AddEvent(GameId gameId, Event @event)
        {
            if (!_events.ContainsKey(gameId))
            {
                _events.Add(gameId, new List<Event>());
            }

            _events[gameId].Add(@event);
        }
    }
}
