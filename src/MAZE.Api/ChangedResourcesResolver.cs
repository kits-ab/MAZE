using System.Collections.Generic;
using System.Linq;
using GenericDataStructures;
using MAZE.Models;

namespace MAZE.Api
{
    public class ChangedResourcesResolver
    {
        public static IEnumerable<string> GetResourceNames(IEnumerable<Union<Player, Character, Location, Obstacle, Path>> changedResources)
        {
            return changedResources.Select(resource =>
                    resource.Map(
                        _ => "players",
                        _ => "characters",
                        _ => "locations",
                        _ => "obstacles",
                        _ => "paths"))
                .Distinct();
        }
    }
}
