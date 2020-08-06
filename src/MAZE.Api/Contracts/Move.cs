using System;

namespace MAZE.Api.Contracts
{
    public class Move : IAction
    {
        public Move(ActionName actionName, int numberOfPathsToTravel)
        {
            if (actionName != ActionName.MoveWest &&
                actionName != ActionName.MoveEast &&
                actionName != ActionName.MoveNorth &&
                actionName != ActionName.MoveSouth)
            {
                throw new ArgumentException("A move action need to be of movement action type");
            }

            ActionName = actionName;
            NumberOfPathsToTravel = numberOfPathsToTravel;
        }

        public ActionName ActionName { get; }

        public int NumberOfPathsToTravel { get; }
    }
}
