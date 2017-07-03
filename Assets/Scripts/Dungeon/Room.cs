using System.Collections;
using System.Collections.Generic;

public class Room
{
    public int ID { get; private set; }
    public Vector2Int Origin { get; private set; }
    public Vector2Int Size { get; private set; }
    private Dictionary<int, GridCell> _gridCells;
	public int DoorCount { get; private set; }

    public Room(int id, Vector2Int origin, Vector2Int size) 
    {
        ID = id;
		Origin = origin;
		Size = size;
        _gridCells = new Dictionary<int, GridCell>();
	}

    public void AddBlock(int cellId, GridCell block)
    {
        if(!_gridCells.ContainsKey(cellId))
            _gridCells.Add(cellId, block);
    }

    public void AddDoor()
    {
        DoorCount++;
    }

    public Dictionary<int, GridCell> GetBounds()
	{
        Dictionary<int, GridCell> bounds = new Dictionary<int, GridCell>();

		for (int x = Origin.X; x <= Origin.X + Size.X; ++x)
		{
			for (int y = Origin.Y; y <= Origin.Y + Size.Y; ++y)
			{
				bool isLeftEdge = x - Origin.X == 0;
                bool isRightEdge = x - (Origin.X + Size.X) == 0;

                bool isTopEdge = y - (Origin.Y + Size.Y) == 0;
				bool isBottomEdge = y - Origin.Y == 0;

                if(!isLeftEdge && !isRightEdge && !isTopEdge && !isBottomEdge)
					continue;

                int roomBlockId = GridManager.Instance.IdFromPosition(x, y);
                bounds.Add(roomBlockId, _gridCells[roomBlockId]);
			}
		}

		return bounds;
	}
}
