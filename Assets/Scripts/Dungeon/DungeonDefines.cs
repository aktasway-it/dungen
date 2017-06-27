public enum EDungeonBlockType
{
    Wall,
    Corridor,
    Room
}

[System.Flags]
public enum EDirection
{
    None = 0,
    North = 1,
    South = 2,
    East = 4,
    West = 8,
    All = 15
}