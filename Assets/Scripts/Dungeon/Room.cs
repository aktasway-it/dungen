using System.Collections;
using System.Collections.Generic;

public class Room
{
    public int ID { get; private set; }
    public Vector2Int Origin { get; private set; }
    public Vector2Int Size { get; private set; }
    private List<DungeonCell> _gridCells;
	public int DoorCount { get; private set; }

    public Room(int id, Vector2Int origin, Vector2Int size) 
    {
        ID = id;
		Origin = origin;
		Size = size;
        _gridCells = new List<DungeonCell>();
	}

    public void AddCell(DungeonCell cell)
    {
        if(!_gridCells.Contains(cell))
            _gridCells.Add(cell);
    }

    public void AddDoor()
    {
        DoorCount++;
    }

    public List<DungeonCell> GetBounds()
	{
        List<DungeonCell> bounds = new List<DungeonCell>();

        for (int i = 0; i < _gridCells.Count; ++i)
        {
            bool isLeftEdge = _gridCells[i].Coords.X - Origin.X == 0;
			bool isRightEdge = _gridCells[i].Coords.X - (Origin.X + Size.X) == 0;

            bool isTopEdge = _gridCells[i].Coords.Y - (Origin.Y + Size.Y) == 0;
            bool isBottomEdge = _gridCells[i].Coords.Y - Origin.Y == 0;

			if (!isLeftEdge && !isRightEdge && !isTopEdge && !isBottomEdge)
				continue;

            bounds.Add(_gridCells[i]);
        }

		return bounds;
	}
}
