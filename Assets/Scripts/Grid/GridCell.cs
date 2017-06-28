using System.Collections;
using System.Collections.Generic;

public class GridCell
{
    public GridCellRenderer Renderer { get; set; }
    public DungeonBlock Occupant { get; set; }
    public Vector2Int Coords { get; protected set; }

    public GridCell(int x, int y)
    {
        Coords = new Vector2Int(x, y);
    }

    public void SetWalls(EDirection newWalls)
    {
        Occupant.SetWalls(newWalls);

		if (Renderer != null)
            Renderer.SetWalls(Occupant.Walls);
    }

    public void BreakWall(EDirection direction)
    {
        Occupant.BreakWall(direction);

		if (Renderer != null)
            Renderer.SetWalls(Occupant.Walls);
    }
}
