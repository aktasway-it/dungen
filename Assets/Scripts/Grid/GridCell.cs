using System.Collections;
using System.Collections.Generic;

public class GridCell
{
    public EDirection Walls { get; protected set; }
    public GridCellRenderer Renderer { get; set; }
    public DungeonBlock Occupant { get; set; }
    public Vector2Int Coords { get; protected set; }

    public GridCell(int x, int y)
    {
        Coords = new Vector2Int(x, y);
        Walls = EDirection.All;
    }

    public void SetWalls(EDirection newWalls)
    {
        if (newWalls == Walls)
			return;

        Walls = newWalls;
		if (Renderer != null)
			Renderer.SetWalls(Walls);
    }

    public void BreakWall(EDirection direction)
    {
        var newWalls = Walls &~ direction;

        if (newWalls == Walls)
            return;
        
        Walls &= ~direction;
		if (Renderer != null)
            Renderer.SetWalls(Walls);
    }
}
