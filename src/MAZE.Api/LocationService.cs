using System;
using System.Collections.Generic;
using MAZE.Api.Models;

namespace MAZE.Api
{
    public class LocationService
    {
        private readonly LocationRepository _locationRepository;

        public LocationService(LocationRepository locationRepository)
        {
            _locationRepository = locationRepository;
        }

        public IEnumerable<Location> GetDiscoveredLocations()
        {
            return _locationRepository.GetDiscoveredLocations();
        }
    }
}
