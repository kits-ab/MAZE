using System.Collections.Generic;
using MAZE.Api.Models;

namespace MAZE.Api
{
    public class LocationRepository
    {
        public IEnumerable<Location> GetDiscoveredLocations()
        {
            for (int locationId = 0; locationId < 15; locationId++)
            {
                yield return new Location(locationId);
            }
        }
    }
}
