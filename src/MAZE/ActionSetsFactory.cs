using System.Collections.Generic;
using MAZE.Models;

namespace MAZE
{
    public static class ActionSetsFactory
    {
        public static IEnumerable<IEnumerable<ActionName>> GetActionSets(int numberOfPlayers)
        {
            switch (numberOfPlayers)
            {
                case 1:
                    yield return new[]
                    {
                        ActionName.MoveWest,
                        ActionName.MoveEast,
                        ActionName.MoveNorth,
                        ActionName.MoveSouth,
                        ActionName.UsePortal,
                        ActionName.ClearObstacle,
                        ActionName.Teleport,
                        ActionName.Disarm,
                        ActionName.Smash,
                        ActionName.Heal,
                    };
                    break;

                case 2:
                    yield return new[]
                    {
                        ActionName.MoveWest,
                        ActionName.MoveSouth,
                        ActionName.UsePortal,
                        ActionName.ClearObstacle,
                        ActionName.Smash,
                    };
                    yield return new[]
                    {
                        ActionName.MoveEast,
                        ActionName.MoveNorth,
                        ActionName.Teleport,
                        ActionName.Disarm,
                        ActionName.Heal,
                    };
                    break;

                case 3:
                    yield return new[]
                    {
                        ActionName.MoveWest,
                        ActionName.Teleport,
                        ActionName.Disarm,
                    };
                    yield return new[]
                    {
                        ActionName.MoveEast,
                        ActionName.MoveNorth,
                        ActionName.Smash,
                        ActionName.Heal,
                    };
                    yield return new[]
                    {
                        ActionName.MoveSouth,
                        ActionName.UsePortal,
                        ActionName.ClearObstacle,
                    };
                    break;

                case 4:
                    yield return new[]
                    {
                        ActionName.MoveWest,
                        ActionName.Teleport,
                        ActionName.Disarm,
                    };
                    yield return new[]
                    {
                        ActionName.MoveEast,
                        ActionName.Smash,
                    };
                    yield return new[]
                    {
                        ActionName.MoveNorth,
                        ActionName.ClearObstacle,
                        ActionName.Heal,
                    };
                    yield return new[]
                    {
                        ActionName.MoveSouth,
                        ActionName.UsePortal,
                    };
                    break;

                case 5:
                    yield return new[]
                    {
                        ActionName.MoveWest,
                        ActionName.Teleport,
                    };
                    yield return new[]
                    {
                        ActionName.MoveEast,
                        ActionName.Smash,
                    };
                    yield return new[]
                    {
                        ActionName.MoveNorth,
                        ActionName.ClearObstacle,
                        ActionName.Heal,
                    };
                    yield return new[]
                    {
                        ActionName.MoveWest,
                        ActionName.Disarm,
                    };
                    yield return new[]
                    {
                        ActionName.MoveSouth,
                        ActionName.UsePortal,
                    };
                    break;

                case 6:
                    yield return new[]
                    {
                        ActionName.MoveWest,
                        ActionName.Teleport,
                    };
                    yield return new[]
                    {
                        ActionName.MoveEast,
                        ActionName.Smash,
                    };
                    yield return new[]
                    {
                        ActionName.MoveNorth,
                        ActionName.Heal,
                    };
                    yield return new[]
                    {
                        ActionName.MoveWest,
                        ActionName.Disarm,
                    };
                    yield return new[]
                    {
                        ActionName.MoveEast,
                        ActionName.ClearObstacle,
                    };
                    yield return new[]
                    {
                        ActionName.MoveSouth,
                        ActionName.UsePortal,
                    };
                    break;

                case 7:
                    yield return new[]
                    {
                        ActionName.MoveWest,
                        ActionName.Teleport,
                    };
                    yield return new[]
                    {
                        ActionName.MoveEast,
                        ActionName.Smash,
                    };
                    yield return new[]
                    {
                        ActionName.MoveNorth,
                        ActionName.Heal,
                    };
                    yield return new[]
                    {
                        ActionName.MoveWest,
                        ActionName.Disarm,
                    };
                    yield return new[]
                    {
                        ActionName.MoveEast,
                        ActionName.ClearObstacle,
                    };
                    yield return new[]
                    {
                        ActionName.MoveNorth,
                        ActionName.Teleport,
                    };
                    yield return new[]
                    {
                        ActionName.MoveSouth,
                        ActionName.UsePortal,
                    };
                    break;

                case 8:
                    yield return new[]
                    {
                        ActionName.MoveWest,
                        ActionName.Teleport,
                    };
                    yield return new[]
                    {
                        ActionName.MoveEast,
                        ActionName.Smash,
                    };
                    yield return new[]
                    {
                        ActionName.MoveNorth,
                        ActionName.Heal,
                    };
                    yield return new[]
                    {
                        ActionName.MoveSouth,
                        ActionName.UsePortal,
                    };
                    yield return new[]
                    {
                        ActionName.MoveWest,
                        ActionName.Disarm,
                    };
                    yield return new[]
                    {
                        ActionName.MoveEast,
                        ActionName.ClearObstacle,
                    };
                    yield return new[]
                    {
                        ActionName.MoveNorth,
                        ActionName.Teleport,
                    };
                    yield return new[]
                    {
                        ActionName.MoveSouth,
                        ActionName.Smash,
                    };
                    break;
            }
        }
    }
}
