using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task<Result<IEnumerable<Location>, ReadGameError>> GetDiscoveredLocationsAsync(GameId gameId)
        {
            var result = await _gameRepository.GetGameAsync(gameId);

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
