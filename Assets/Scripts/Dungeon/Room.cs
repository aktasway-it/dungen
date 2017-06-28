using System.Collections;
using System.Collections.Generic;

public class Room
{
    public int DoorCount { get; private set; }
	private readonly Vector2Int _topLeftCorner;
	private readonly Vector2Int _size;
    private Dictionary<int, RoomBlock> _blocks;

	public Room(int id, Vector2Int topLeftCorner, Vector2Int size) 
    {
		_topLeftCorner = topLeftCorner;
		_size = size;
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

		for (int x = _topLeftCorner.X; x <= _topLeftCorner.X + _size.X; ++x)
		{
			for (int y = _topLeftCorner.Y; y <= _topLeftCorner.Y + _size.Y; ++y)
			{
				bool isLeftEdge = x - _topLeftCorner.X == 0;
                bool isRightEdge = x - (_topLeftCorner.X + _size.X) == 0;

                bool isTopEdge = y - (_topLeftCorner.Y + _size.Y) == 0;
				bool isBottomEdge = y - _topLeftCorner.Y == 0;

                if(!isLeftEdge && !isRightEdge && !isTopEdge && !isBottomEdge)
					continue;

                int roomBlockId = GridManager.Instance.IdFromPosition(x, y);
                bounds.Add(roomBlockId, _blocks[roomBlockId]);
			}
		}

		return bounds;
	}
}
