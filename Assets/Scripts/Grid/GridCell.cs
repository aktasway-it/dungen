using System.Collections;
using System.Collections.Generic;

public class GridCell
{
	public int ID { get; private set; }

	public GridCellRenderer Renderer { get; set; }
    public DungeonBlock Occupant { get; set; }
    public Vector2Int Coords { get; protected set; }

    public GridCell(int id, int x, int y)
    {
        ID = id;
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
