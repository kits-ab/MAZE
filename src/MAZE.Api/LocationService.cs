using System.Collections.Generic;
using System.Linq;
using GameId = System.String;
using Location = MAZE.Api.Contracts.Location;

namespace MAZE.Api
{
    public class LocationService
    {
        private readonly GameService _gameService;

        public LocationService(GameService gameService)
        {
            _gameService = gameService;
        }

        public IEnumerable<Location> GetDiscoveredLocations(GameId gameId)
        {
            return _gameService.GetGame(gameId).World.Locations
                .Where(location => location.IsDiscovered)
                .Select(Convert);
        }

        private static Location Convert(Models.Location location)
        {
            return new Location(location.Id);
        }
    }
}
