using System.Collections.Generic;
using System.Drawing;
using System.IO;
using GenericDataStructures;
using MAZE.Events;
using MAZE.Models;
using Path = MAZE.Models.Path;
using WorldId = System.String;

namespace MAZE.Api
{
    public class WorldSerializer
    {
        private readonly int _locationColor = Color.White.ToArgb();
        private readonly int _wizardStartLocationColor = Color.FromArgb(255, 128, 128, 255).ToArgb();
        private readonly int _rogueStartLocationColor = Color.FromArgb(255, 128, 255, 128).ToArgb();
        private readonly int _warriorStartLocationColor = Color.FromArgb(255, 255, 128, 128).ToArgb();
        private readonly int _clericStartLocationColor = Color.FromArgb(255, 255, 255, 128).ToArgb();
        private readonly int _pathColor = Color.FromArgb(255, 200, 200, 200).ToArgb();
        private readonly int _wizardBlockedPathColor = Color.FromArgb(255, 0, 0, 255).ToArgb();
        private readonly int _rogueBlockedPathColor = Color.FromArgb(255, 0, 255, 0).ToArgb();
        private readonly int _warriorBlockedPathColor = Color.FromArgb(255, 255, 0, 0).ToArgb();
        private readonly int _clericBlockedPathColor = Color.FromArgb(255, 255, 255, 0).ToArgb();

        public IEnumerable<Union<WorldCreated, CharacterAdded>> Deserialize(WorldId worldId, Stream worldStream)
        {
            var locationCounter = 0;
            var locations = new Dictionary<(int X, int Y), Location>();

            var pathCounter = 0;
            var paths = new List<Path>();

            var obstacles = new List<Obstacle>();

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
                            || color == _wizardStartLocationColor
                            || color == _rogueStartLocationColor
                            || color == _warriorStartLocationColor
                            || color == _clericStartLocationColor)
                        {
                            var location = new Location(locationCounter++);
                            locations.Add((x, y), location);

                            if (color == _wizardStartLocationColor)
                            {
                                var character = new Character(characterCounter++, CharacterClass.Wizard, location.Id);
                                characters.Add(character);
                            }
                        }
                    }
                }

                // Read paths and obstacles
                foreach (var locationInformation in locations)
                {
                    void TryAddPath(int xOffset, int yOffset, PathType pathType)
                    {
                        var (locationX, locationY) = locationInformation.Key;
                        var pathCandidateColor = image.GetPixel(locationX + xOffset, locationY + yOffset).ToArgb();
                        if ((pathCandidateColor == _pathColor ||
                             pathCandidateColor == _wizardBlockedPathColor ||
                             pathCandidateColor == _rogueBlockedPathColor ||
                             pathCandidateColor == _warriorBlockedPathColor ||
                             pathCandidateColor == _clericBlockedPathColor)
                            && locations.TryGetValue((locationX + (xOffset * 2), locationY + (yOffset * 2)), out var neighborLocation))
                        {
                            var pathId = pathCounter++;
                            paths.Add(new Path(pathId, locationInformation.Value.Id, neighborLocation.Id, pathType));
                            if (pathCandidateColor == _wizardBlockedPathColor)
                            {
                                obstacles.Add(new Obstacle(ObstacleType.ForceField, pathId));
                            }

                            if (pathCandidateColor == _rogueBlockedPathColor)
                            {
                                obstacles.Add(new Obstacle(ObstacleType.Lock, pathId));
                            }

                            if (pathCandidateColor == _warriorBlockedPathColor)
                            {
                                obstacles.Add(new Obstacle(ObstacleType.Stone, pathId));
                            }

                            if (pathCandidateColor == _clericBlockedPathColor)
                            {
                                obstacles.Add(new Obstacle(ObstacleType.Ghost, pathId));
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

            var world = new World(worldId, locations.Values, paths, obstacles);
            yield return new WorldCreated(world);

            foreach (var character in characters)
            {
                yield return new CharacterAdded(character);
            }
        }
    }
}
