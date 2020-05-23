using System;
using System.Collections.Generic;
using System.Linq;
using MAZE.Api.Contracts;
using GameId = System.String;
using Path = MAZE.Api.Contracts.Path;

namespace MAZE.Api
{
    public class PathService
    {
        private readonly GameService _gameService;
        private readonly PathRepository _pathRepository;

        public PathService(GameService gameService, PathRepository pathRepository)
        {
            _gameService = gameService;
            _pathRepository = pathRepository;
        }

        public IEnumerable<Path> GetDiscoveredPaths(GameId gameId)
        {
            var gameWorld = _gameService.GetGame(gameId).World;

            var discoveredLocationIds = gameWorld.Locations
                .Where(location => location.IsDiscovered)
                .Select(location => location.Id)
                .ToHashSet();

            return gameWorld.Paths
                .Where(path =>
                    discoveredLocationIds.Contains(path.From) || discoveredLocationIds.Contains(path.To))
                .Select(Convert);
        }

        private static Path Convert(Models.Path path)
        {
            return new Path(path.Id, path.From, path.To, Convert(path.Type));
        }

        private static PathType Convert(Models.PathType pathType)
        {
            return pathType switch
            {
                Models.PathType.West => PathType.West,
                Models.PathType.East => PathType.East,
                Models.PathType.North => PathType.North,
                Models.PathType.South => PathType.South,
                Models.PathType.Portal => PathType.Portal,
                _ => throw new ArgumentOutOfRangeException(nameof(pathType), pathType, null)
            };
        }
    }
}
