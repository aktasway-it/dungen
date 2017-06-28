using System.Collections;
using System.Collections.Generic;

public class Room
{
    public int ID { get; private set; }
    public Vector2Int Origin { get; private set; }
    public Vector2Int Size { get; private set; }
    private Dictionary<int, RoomBlock> _blocks;
	public int DoorCount { get; private set; }

    public Room(int id, Vector2Int origin, Vector2Int size) 
    {
        ID = id;
		Origin = origin;
		Size = size;
        _blocks = new Dictionary<int, RoomBlock>();
	}

    public void AddBlock(int cellId, RoomBlock block)
    {
        if(!_blocks.ContainsKey(cellId))
            _blocks.Add(cellId, block);
    }

    public void AddDoor()
    {
        DoorCount++;
    }

    public Dictionary<int, RoomBlock> GetBounds()
	{
        Dictionary<int, RoomBlock> bounds = new Dictionary<int, RoomBlock>();

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
                bounds.Add(roomBlockId, _blocks[roomBlockId]);
			}
		}

		return bounds;
	}
}
