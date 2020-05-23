using System.Collections.Generic;
using System.Drawing;
using System.IO;
using MAZE.Models;
using Path = MAZE.Models.Path;
using WorldId = System.String;

namespace MAZE.Api
{
    public class WorldSerializer
    {
        private readonly int _locationColor = Color.White.ToArgb();
        private readonly int _pathColor = Color.White.ToArgb();

        public World Deserialize(WorldId worldId, Stream worldStream)
        {
            var locationCounter = 0;
            var locations = new Dictionary<(int X, int Y), Location>();

            var pathCounter = 0;
            var paths = new List<Path>();

            using (var image = new Bitmap(worldStream))
            {
                // Read locations
                for (var y = 1; y < image.Height; y += 2)
                {
                    for (var x = 1; x < image.Width; x += 2)
                    {
                        var color = image.GetPixel(x, y);
                        if (color.ToArgb() == _locationColor)
                        {
                            locations.Add((x, y), new Location(locationCounter++, true));
                        }
                    }
                }

                // Read paths
                foreach (var locationInformation in locations)
                {
                    void TryAddPath(int xOffset, int yOffset, PathType pathType)
                    {
                        var (locationX, locationY) = locationInformation.Key;
                        var pathCandidateColor = image.GetPixel(locationX + xOffset, locationY + yOffset);
                        if (pathCandidateColor.ToArgb() == _pathColor && locations.TryGetValue((locationX + (xOffset * 2), locationY + (yOffset * 2)), out var neighborLocation))
                        {
                            paths.Add(new Path(pathCounter++, locationInformation.Value.Id, neighborLocation.Id, pathType));
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

            return new World(worldId, locations.Values, paths);
        }
    }
}
