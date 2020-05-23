using System;
using System.Linq;
using MAZE.Events;
using MAZE.Models;
using GameId = System.String;

namespace MAZE
{
    public class GameService
    {
        private readonly EventRepository _eventService;

        public GameService(EventRepository eventService)
        {
            _eventService = eventService;
        }

        public GameId CreateGame(World world)
        {
            var newGameId = Guid.NewGuid().ToString();

            var gameCreatedEvent = new GameCreated(world);

            _eventService.AddEvent(newGameId, gameCreatedEvent);

            return newGameId;
        }

        public Game GetGame(GameId id)
        {
            var gameEvents = _eventService.GetEvents(id);

            var gameCreatedEvent = gameEvents.First();

            return new Game(id, gameCreatedEvent.World);
        }
    }
}
