using System;
using System.Collections.Generic;
using System.Linq;
using GenericDataStructures;
using MAZE.Api.Contracts;
using GameId = System.String;

namespace MAZE.Api
{
    public class PathService
    {
        private readonly GameService _gameService;

        public PathService(GameService gameService)
        {
            _gameService = gameService;
        }

        public Result<IEnumerable<Path>, ReadGameError> GetDiscoveredPaths(GameId gameId)
        {
            var result = _gameService.GetGame(gameId);

            return result.Map(
                game =>
                {
                    var visiblePaths = GetVisiblePaths(game);

                    return new Result<IEnumerable<Path>, ReadGameError>(visiblePaths);
                },
                readGameError => readGameError);
        }

        private static IEnumerable<Path> GetVisiblePaths(Models.Game game)
        {
            var discoveredLocationIds = game.World.Locations
                .Where(location => location.IsDiscovered)
                .Select(location => location.Id)
                .ToHashSet();

            return game.World.Paths
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
