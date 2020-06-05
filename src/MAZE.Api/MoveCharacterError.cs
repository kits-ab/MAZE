namespace MAZE.Api
{
    public enum MoveCharacterError
    {
        GameNotFound,
        CharacterNotFound,
        LocationNotFound,
        NoPathBetweenLocations,
        PathNotInAStraightLine,
        BlockedByObstacle,
        BlockedByCharacter,
    }
}
