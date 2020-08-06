using ObstacleId = System.Int32;

namespace MAZE.Api.Contracts
{
    public class ClearObstacle : IAction
    {
        public const ActionName Name = ActionName.ClearObstacle;

        public ClearObstacle(ObstacleId obstacle)
        {
            Obstacle = obstacle;
        }

        public ActionName ActionName => Name;

        public ObstacleId Obstacle { get; }
    }
}
