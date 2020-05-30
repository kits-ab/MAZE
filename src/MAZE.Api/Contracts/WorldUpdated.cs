using System.Collections.Generic;
using System.Linq;

namespace MAZE.Api.Contracts
{
    public class WorldUpdated
    {
        public WorldUpdated(params string[] potentiallyChangedResources)
        {
            PotentiallyChangedResources = potentiallyChangedResources.ToList();
        }

        public IEnumerable<string> PotentiallyChangedResources { get; }
    }
}
