using PathId = System.Int32;

namespace MAZE.Api.Contracts
{
    public class UsePortal : IAction
    {
        public const ActionName Name = ActionName.UsePortal;

        public UsePortal(PathId portalPath)
        {
            PortalPath = portalPath;
        }

        public ActionName ActionName => Name;

        public PathId PortalPath { get; }
    }
}
