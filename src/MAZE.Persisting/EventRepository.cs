using System;
using System.Collections.Generic;
using System.Linq;
using GenericDataStructures;
using Newtonsoft.Json;
using GameId = System.Int32;

namespace MAZE
{
    public class EventRepository
    {
        private readonly Dictionary<GameId, List<Event>> _events = new Dictionary<GameId, List<Event>>();

        public Result<IEnumerable<Event>, ReadGameError> GetEvents(GameId gameId)
        {
            if (!_events.ContainsKey(gameId))
            {
                return ReadGameError.NotFound;
            }

            return GetEventClones(gameId).ToList();
        }

        public void AddEvent(GameId gameId, Event @event)
        {
            if (!_events.ContainsKey(gameId))
            {
                _events.Add(gameId, new List<Event>());
            }

            _events[gameId].Add(@event);
        }

        private IEnumerable<Event> GetEventClones(GameId gameId)
        {
            // Clone events to replicate the behavior of a database instead of allowing to modify stored data
            foreach (var @event in _events[gameId].ToList())
            {
                var json = JsonConvert.SerializeObject(@event);
                var clonedEvent = JsonConvert.DeserializeObject(json, @event.GetType());
                if (clonedEvent == null)
                {
                    throw new InvalidOperationException("Failed to clone event");
                }

                yield return (Event)clonedEvent;
            }
        }
    }
}
