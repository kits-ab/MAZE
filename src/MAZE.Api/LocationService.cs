using System.Collections.Generic;
using System.Linq;
using GenericDataStructures;
using GameId = System.String;
using Location = MAZE.Api.Contracts.Location;

namespace MAZE.Api
{
    public class LocationService
    {
        private readonly GameRepository _gameRepository;

        public LocationService(GameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public Result<IEnumerable<Location>, ReadGameError> GetDiscoveredLocations(GameId gameId)
        {
            var result = _gameRepository.GetGame(gameId);

            return result.Map(
                game =>
                {
                    var discoveredLocations = game.World.Locations
                        .Where(location => location.IsDiscovered)
                        .Select(Convert);

                    return new Result<IEnumerable<Location>, ReadGameError>(discoveredLocations);
                },
                readGameError => readGameError);
        }

        private static Location Convert(Models.Location location)
        {
            return new Location(location.Id);
        }
    }
}
