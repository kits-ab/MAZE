using ObstacleId = System.Int32;

namespace MAZE.Api.Contracts
{
    public class ClearObstacle : IAction
    {
        public const string Name = "clearObstacle";

        public ClearObstacle(ObstacleId obstacle)
        {
            Obstacle = obstacle;
        }

        public string ActionName => Name;

        public ObstacleId Obstacle { get; }
    }
}
