using UnityEngine;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class GridManager : SingletonBehavior<GridManager> 
{
	public int Width
	{
		get
		{
			return _width;
		}
	}

	public int Height
	{
		get
		{
			return _height;
		}
	}

	[SerializeField]
	private int _width;

	[SerializeField]
	private int _height;

	[SerializeField][Range(0.1f, 5.0f)]
	private float _tileSize = 1.0f;

	[SerializeField]
	private float _spacing = 0.1f;

    [SerializeField]
    private MinMax _roomWidth;

	[SerializeField]
	private MinMax _roomHeight;

	[SerializeField]
	private int _maxAttempts;

	[SerializeField]
	private int _seed;

	[SerializeField]
	private bool _useRandomSeed;

    [SerializeField]
    private bool _useDebugVisualization;

    [SerializeField]
    private GridCellRenderer _cellVisualPrefab = null;

    private GridCell[,] _grid = null;
    private Dictionary<int, RoomBlock> _rooms;

    private void Start()
    {
        Create();
    }

    public void Create () 
	{
		if(_useRandomSeed)
			_seed = (int) (DateTime.Now.Ticks % 100000);
		
		RandomUtility.Create(_seed);
        GenerateGrid();
        GenerateRooms();
	}

	private void GenerateGrid()
	{
        _grid = new GridCell[_width, _height];

		Vector3 initialPos = new Vector3(-_width * _tileSize * 0.5f, -_height * _tileSize * 0.5f, 0);
		initialPos.x -= _spacing * (_width - 1) * 0.5f;
		initialPos.y -= _spacing * (_height - 1) * 0.5f;

        for (int x = 0; x < _width; ++x)
        {
            for (int y = 0; y < _width; ++y)
            {
				GridCell cell = new GridCell(x, y);
				_grid[x, y] = cell;

                if(_useDebugVisualization)
                {
					Vector3 spawnPos = new Vector3(x * (_tileSize + _spacing) + _tileSize * 0.5f, y * (_tileSize + _spacing) + _tileSize * 0.5f, 0);
                    GridCellRenderer cellRenderer = Instantiate<GridCellRenderer>(_cellVisualPrefab, initialPos + spawnPos, Quaternion.identity);
					cellRenderer.transform.localScale = Vector3.one * _tileSize;
					cellRenderer.name = string.Format("Tile_{0}x{1}", x, y);
					cellRenderer.transform.parent = transform;

                    cell.Renderer = cellRenderer;
                }
            }
        }
	}

	/*public bool IsMapFullyConnected(bool[,] obstacleMap, int currentObstacleCount)
	{
		int obstacleMapWidth = obstacleMap.GetLength(0);
		int obstacleMapHeight = obstacleMap.GetLength(1);

		bool[,] visitedMapNodes = new bool[obstacleMapWidth, obstacleMapHeight];

		Queue<Vector2Int> visitedNodeQueue = new Queue<Vector2Int>();
		visitedNodeQueue.Enqueue (_startTile.Coords);

		visitedMapNodes [_startTile.Coords.x, _startTile.Coords.y] = true;

		int accessibleTileCount = 1;

		while (visitedNodeQueue.Count > 0) 
		{
			Vector2Int currentNode = visitedNodeQueue.Dequeue();

			for (int x = -1; x <= 1; x ++) 
			{
				for (int y = -1; y <= 1; y ++) 
				{
					int neighbourX = currentNode.x + x;
					int neighbourY = currentNode.y + y;

					if (x == 0 || y == 0) 
					{
						if (neighbourX >= 0 && neighbourX < obstacleMapWidth && neighbourY >= 0 && neighbourY < obstacleMapHeight) 
						{
							if (!visitedMapNodes[neighbourX,neighbourY] && !obstacleMap[neighbourX,neighbourY]) 
							{
								visitedMapNodes[neighbourX,neighbourY] = true;
								visitedNodeQueue.Enqueue(new Vector2Int(neighbourX,neighbourY));
								accessibleTileCount ++;
							}
						}
					}
				}
			}
		}

		int targetAccessibleTileCount = (int)(_freeTiles.Count - currentObstacleCount);
		return targetAccessibleTileCount == accessibleTileCount;
	}*/

    public void GenerateRooms()
    {
        _rooms = new Dictionary<int, RoomBlock>();

        int attempts = 0;
        int roomId = 0;
        while(attempts < _maxAttempts)
        {
            GridCell cell = GetRandomCell();
            Vector2Int roomSize = new Vector2Int(RandomUtility.Range(_roomWidth.Min, _roomWidth.Max + 1), RandomUtility.Range(_roomHeight.Min, _roomHeight.Max + 1));
            if(BoxCheck(cell.Coords, roomSize.X, roomSize.Y))
            {
                RoomBlock room = new RoomBlock(roomId);

                for (int x = cell.Coords.X; x < cell.Coords.X + roomSize.X; ++x)
				{
                    for (int y = cell.Coords.Y; y < cell.Coords.Y + roomSize.Y; ++y)
					{
                        _grid[x, y].Occupant = room;
                        _grid[x, y].Renderer.SetColor(Color.cyan);
					}
				}

				roomId++;
                _rooms.Add(roomId, room);
			}

			attempts++;
        }
    }

    public bool BoxCheck(Vector2Int topLeftCoords, int width, int height)
    {
        for (int x = topLeftCoords.X; x < topLeftCoords.X + width; ++x)
        {
            for (int y = topLeftCoords.Y; y < topLeftCoords.Y + height; ++y)
            {
                if (!IsValidCoordinate(x, y) || _grid[x, y].Occupant != null)
                    return false;
            }
        }

        return true;
    }

    public GridCell GetRandomCell()
    {
        return _grid[RandomUtility.Range(0, _width), RandomUtility.Range(0, _height)];
    }

	public bool IsValidCoordinate(Vector2Int coords)
	{
		return IsValidCoordinate(coords.X, coords.Y);
	}

	public bool IsValidCoordinate(int x, int y)
	{
		return x >= 0 && x < _width && y >= 0 && y < _height;
	}
}
