using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenericDataStructures;
using MAZE.Api.Contracts;
using GameId = System.String;

namespace MAZE.Api
{
    public class PathService
    {
        private readonly GameRepository _gameService;

        public PathService(GameRepository gameService)
        {
            _gameService = gameService;
        }

        public async Task<Result<IEnumerable<Path>, ReadGameError>> GetPathsAsync(GameId gameId)
        {
            var result = await _gameService.GetGameAsync(gameId);

            return result.Map(
                game =>
                {
                    var discoveredPaths = game.World.Paths
                        .Where(path => path.IsDiscovered)
                        .Select(Convert);

                    return new Result<IEnumerable<Path>, ReadGameError>(discoveredPaths);
                },
                readGameError => readGameError);
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
