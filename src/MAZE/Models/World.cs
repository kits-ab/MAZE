using System;
using System.Collections.Generic;
using System.Linq;
using LocationId = System.Int32;
using WorldId = System.String;

namespace MAZE.Models
{
    public class World
    {
        public WorldId? Id { get; set; }

        public List<Location> Locations { get; } = new List<Location>();

        public List<Path> Paths { get; } = new List<Path>();

        public List<Obstacle> Obstacles { get; } = new List<Obstacle>();

        public List<Character> Characters { get; } = new List<Character>();

        public void DiscoverLocation(LocationId locationId)
        {
            var location = Locations.Single(locationCandidate => locationCandidate.Id == locationId);
            if (location.IsDiscovered)
            {
                throw new ArgumentException($"Location {locationId} is already discovered");
            }

            location.IsDiscovered = true;
            foreach (var pathToDiscover in Paths.Where(path => !path.IsDiscovered && (path.From == locationId || path.To == locationId)))
            {
                pathToDiscover.IsDiscovered = true;
                foreach (var obstacleToDiscover in Obstacles.Where(obstacle =>
                    !obstacle.IsDiscovered && obstacle.BlockedPathIds.Contains(pathToDiscover.Id)))
                {
                    obstacleToDiscover.IsDiscovered = true;
                }
            }

            foreach (var neighborLocationIdToDiscover in Paths.Where(path => path.From == locationId && !Obstacles.Any(obstacle => obstacle.BlockedPathIds.Contains(path.Id))).Select(path => path.To))
            {
                if (!Locations.Single(neighborLocationCandidate => neighborLocationCandidate.Id == neighborLocationIdToDiscover).IsDiscovered)
                {
                    DiscoverLocation(neighborLocationIdToDiscover);
                }
            }
        }
    }
}
