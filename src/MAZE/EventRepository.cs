using System.Collections.Generic;
using GenericDataStructures;
using MAZE.Events;
using GameId = System.String;

namespace MAZE
{
    public class EventRepository
    {
        private readonly Dictionary<GameId, List<Union<WorldCreated, CharacterAdded>>> _events = new Dictionary<GameId, List<Union<WorldCreated, CharacterAdded>>>();

        public Result<IEnumerable<Union<WorldCreated, CharacterAdded>>, ReadEventsError> GetEvents(GameId gameId)
        {
            if (!_events.ContainsKey(gameId))
            {
                return ReadEventsError.GameNotFound;
            }

            return _events[gameId];
        }

        public void AddEvent(GameId gameId, Union<WorldCreated, CharacterAdded> @event)
        {
            if (!_events.ContainsKey(gameId))
            {
                _events.Add(gameId, new List<Union<WorldCreated, CharacterAdded>>());
            }

            _events[gameId].Add(@event);
        }
    }
}
