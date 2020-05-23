using System.Collections.Generic;
using MAZE.Events;
using GameId = System.String;

namespace MAZE
{
    public class EventRepository
    {
        private readonly Dictionary<GameId, List<GameCreated>> _events = new Dictionary<GameId, List<GameCreated>>();

        public IEnumerable<GameCreated> GetEvents(GameId gameId)
        {
            return _events[gameId];
        }

        public void AddEvent(GameId gameId, GameCreated @event)
        {
            if (!_events.ContainsKey(gameId))
            {
                _events.Add(gameId, new List<GameCreated>());
            }

            _events[gameId].Add(@event);
        }
    }
}
