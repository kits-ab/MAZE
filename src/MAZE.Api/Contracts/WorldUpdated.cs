using System.Collections.Generic;
using System.Linq;

namespace MAZE.Api.Contracts
{
    public class WorldUpdated
    {
        public WorldUpdated(IEnumerable<string> potentiallyChangedResources)
        {
            PotentiallyChangedResources = potentiallyChangedResources.ToList();
        }

        public WorldUpdated(params string[] potentiallyChangedResources)
        {
            PotentiallyChangedResources = potentiallyChangedResources.ToList();
        }

        public List<string> PotentiallyChangedResources { get; }
    }
}
