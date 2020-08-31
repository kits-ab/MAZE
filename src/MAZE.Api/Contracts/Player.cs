using System.Collections.Generic;
using System.Linq;
using PlayerId = System.Int32;

namespace MAZE.Api.Contracts
{
    public class Player
    {
        public Player(int id, string name, IEnumerable<ActionName> actions)
        {
            Id = id;
            Name = name;
            Actions = actions.ToList();
        }

        public PlayerId Id { get; }

        public string Name { get; }

        public List<ActionName> Actions { get; }
    }
}
