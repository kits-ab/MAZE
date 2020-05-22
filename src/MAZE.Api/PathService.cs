using System.Collections.Generic;
using System.Linq;
using MAZE.Api.Models;

namespace MAZE.Api
{
    public class PathService
    {
        private readonly LocationRepository _locationRepository;
        private readonly PathRepository _pathRepository;

        public PathService(LocationRepository locationRepository, PathRepository pathRepository)
        {
            _locationRepository = locationRepository;
            _pathRepository = pathRepository;
        }

        public IEnumerable<Path> GetDiscoveredPaths()
        {
            var discoveredLocationIds = _locationRepository.GetDiscoveredLocations()
                .Select(location => location.Id)
                .ToHashSet();

            return _pathRepository.GetAll()
                .Where(path =>
                    discoveredLocationIds.Contains(path.From) || discoveredLocationIds.Contains(path.To));
        }
    }
}
