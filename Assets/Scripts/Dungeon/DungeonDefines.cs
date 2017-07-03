public enum ECellType
{
    Empty,
    Corridor,
    Room
}

public enum EEdgeType
{
    None,
    Wall,
    Door
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