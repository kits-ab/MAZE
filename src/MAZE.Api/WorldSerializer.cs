using System.Collections.Generic;
using System.Drawing;
using System.IO;
using MAZE.Events;
using MAZE.Models;
using Path = MAZE.Models.Path;
using PathId = System.Int32;
using WorldId = System.String;

namespace MAZE.Api
{
    public class WorldSerializer
    {
        private readonly int _locationColor = Color.White.ToArgb();
        private readonly int _mageStartLocationColor = Color.FromArgb(255, 128, 128, 255).ToArgb();
        private readonly int _rogueStartLocationColor = Color.FromArgb(255, 128, 255, 128).ToArgb();
        private readonly int _warriorStartLocationColor = Color.FromArgb(255, 255, 128, 128).ToArgb();
        private readonly int _clericStartLocationColor = Color.FromArgb(255, 255, 255, 128).ToArgb();
        private readonly int _pathColor = Color.FromArgb(255, 200, 200, 200).ToArgb();
        private readonly int _mageBlockedPathColor = Color.FromArgb(255, 0, 0, 255).ToArgb();
        private readonly int _rogueBlockedPathColor = Color.FromArgb(255, 0, 255, 0).ToArgb();
        private readonly int _warriorBlockedPathColor = Color.FromArgb(255, 255, 0, 0).ToArgb();
        private readonly int _clericBlockedPathColor = Color.FromArgb(255, 255, 255, 0).ToArgb();

        public IEnumerable<Event> Deserialize(WorldId worldId, Stream worldStream)
        {
            var locationCounter = 0;
            var locations = new Dictionary<(int X, int Y), Location>();

            var pathCounter = 0;
            var paths = new List<Path>();

            var pathIdsBlockedByObstacle = new Dictionary<(int X, int Y, ObstacleType ObstacleType), List<PathId>>();

            var characterCounter = 0;
            var characters = new List<Character>();

            using (var image = new Bitmap(worldStream))
            {
                // Read locations and characters
                for (var y = 1; y < image.Height; y += 2)
                {
                    for (var x = 1; x < image.Width; x += 2)
                    {
                        var color = image.GetPixel(x, y).ToArgb();
                        if (color == _locationColor
                            || color == _mageStartLocationColor
                            || color == _rogueStartLocationColor
                            || color == _warriorStartLocationColor
                            || color == _clericStartLocationColor)
                        {
                            var location = new Location(locationCounter++);
                            locations.Add((x, y), location);

                            CharacterClass? characterClassToCreate = null;

                            if (color == _mageStartLocationColor)
                            {
                                characterClassToCreate = CharacterClass.Mage;
                            }

                            if (color == _rogueStartLocationColor)
                            {
                                characterClassToCreate = CharacterClass.Rogue;
                            }

                            if (color == _warriorStartLocationColor)
                            {
                                characterClassToCreate = CharacterClass.Warrior;
                            }

                            if (color == _clericStartLocationColor)
                            {
                                characterClassToCreate = CharacterClass.Cleric;
                            }

                            if (characterClassToCreate.HasValue)
                            {
                                var character = new Character(characterCounter++, characterClassToCreate.Value, location.Id);
                                characters.Add(character);
                            }
                        }
                    }
                }

                // Read paths and obstacles
                foreach (var locationInformation in locations)
                {
                    void AddBlockedPoint(PathId pathId, int pathX, int pathY, ObstacleType obstacleType)
                    {
                        var key = (x: pathX, y: pathY, obstacleType);
                        if (!pathIdsBlockedByObstacle.ContainsKey(key))
                        {
                            pathIdsBlockedByObstacle.Add(key, new List<int>());
                        }

                        pathIdsBlockedByObstacle[key].Add(pathId);
                    }

                    void TryAddPath(int xOffset, int yOffset, PathType pathType)
                    {
                        var (locationX, locationY) = locationInformation.Key;
                        var pathX = locationX + xOffset;
                        var pathY = locationY + yOffset;
                        var pathCandidateColor = image.GetPixel(pathX, pathY).ToArgb();
                        if ((pathCandidateColor == _pathColor ||
                             pathCandidateColor == _mageBlockedPathColor ||
                             pathCandidateColor == _rogueBlockedPathColor ||
                             pathCandidateColor == _warriorBlockedPathColor ||
                             pathCandidateColor == _clericBlockedPathColor)
                            && locations.TryGetValue((locationX + (xOffset * 2), locationY + (yOffset * 2)), out var neighborLocation))
                        {
                            var pathId = pathCounter++;
                            paths.Add(new Path(pathId, locationInformation.Value.Id, neighborLocation.Id, pathType));
                            if (pathCandidateColor == _mageBlockedPathColor)
                            {
                                AddBlockedPoint(pathId, pathX, pathY, ObstacleType.ForceField);
                            }

                            if (pathCandidateColor == _rogueBlockedPathColor)
                            {
                                AddBlockedPoint(pathId, pathX, pathY, ObstacleType.Lock);
                            }

                            if (pathCandidateColor == _warriorBlockedPathColor)
                            {
                                AddBlockedPoint(pathId, pathX, pathY, ObstacleType.Stone);
                            }

                            if (pathCandidateColor == _clericBlockedPathColor)
                            {
                                AddBlockedPoint(pathId, pathX, pathY, ObstacleType.Ghost);
                            }
                        }
                    }

                    TryAddPath(-1, 0, PathType.West);
                    TryAddPath(1, 0, PathType.East);
                    TryAddPath(0, -1, PathType.North);
                    TryAddPath(0, 1, PathType.South);

                    TryAddPath(-1, -1, PathType.Portal);
                    TryAddPath(1, -1, PathType.Portal);
                    TryAddPath(-1, 1, PathType.Portal);
                    TryAddPath(1, 1, PathType.Portal);
                }
            }

            var obstacleCounter = 0;
            var obstacles = new List<Obstacle>();
            foreach (var obstacleInformation in pathIdsBlockedByObstacle)
            {
                obstacles.Add(new Obstacle(obstacleCounter++, obstacleInformation.Key.ObstacleType, obstacleInformation.Value));
            }

            yield return new WorldLoaded(worldId, locations.Values, paths, obstacles);

            foreach (var character in characters)
            {
                yield return new CharacterAdded(character);
            }
        }
    }
}
