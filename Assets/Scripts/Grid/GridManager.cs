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
    private int _minRoomsDistance;

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
        Job.Create(GenerateCorridors());
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

            Vector2Int roomDistanceVector = new Vector2Int(_minRoomsDistance, _minRoomsDistance);

            if(BoxCheck(cell.Coords - roomDistanceVector, roomSize.X + _minRoomsDistance * 2, roomSize.Y + _minRoomsDistance * 2))
            {
                RoomBlock room = new RoomBlock(roomId);

                for (int x = cell.Coords.X; x <= cell.Coords.X + roomSize.X; ++x)
				{
                    for (int y = cell.Coords.Y; y <= cell.Coords.Y + roomSize.Y; ++y)
					{
                        EDirection walls = EDirection.None;
                        bool isLeftEdge = x - cell.Coords.X == 0;
                        bool isRightEdge = x - (cell.Coords.X + roomSize.X) == 0;

						bool isTopEdge = y - (cell.Coords.Y + roomSize.Y) == 0;
                        bool isBottomEdge = y - cell.Coords.Y == 0;

                        if (isLeftEdge)
                            walls |= EDirection.West;
                        else if (isRightEdge)
                            walls |= EDirection.East;

                        if (isTopEdge)
                            walls |= EDirection.North;
                        else if (isBottomEdge)
                            walls |= EDirection.South;

                        _grid[x, y].SetWalls(walls);
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

    public IEnumerator GenerateCorridors()
    {
        GridCell currentCell = null;
        EDirection[,] visited = new EDirection[_width, _height];

		// select a random starting cell
		while (currentCell == null)
        {
            var randomCell = GetRandomCell();
            if (randomCell.Occupant == null)
                currentCell = randomCell;
        }

        // mark walls and rooms as already visited
        for (int x = 0; x < _width; ++x)
        {
            for (int y = 0; y < _width; ++y)
            {
                visited[x, y] = _grid[x, y].Occupant != null ? _grid[x, y].Walls : EDirection.All;
            }
        }

        // mark starting cell as visited
        currentCell.Occupant = new CorridorBlock(0);
        currentCell.Renderer.SetColor(Color.magenta);

		Stack<GridCell> cellStack = new Stack<GridCell>();
        cellStack.Push(currentCell);

        Color debugColor = new Color(RandomUtility.Range(0.0f, 1.0f), RandomUtility.Range(0.0f, 1.0f), RandomUtility.Range(0.0f, 1.0f));

        int corridorID = 1;

		List<Vector2Int> neighbors = new List<Vector2Int>();
		
        while(cellStack.Count > 0)
        {
            neighbors.Clear();

            for (int x = currentCell.Coords.X - 1; x <= currentCell.Coords.X + 1; ++x)
            {
                for (int y = currentCell.Coords.Y - 1; y <= currentCell.Coords.Y + 1; ++y)
                {
                    if (((x - currentCell.Coords.X) != 0 && (y - currentCell.Coords.Y) != 0) || (x - currentCell.Coords.X) == 0 && (y - currentCell.Coords.Y) == 0)
                        continue;
                    
                    if (IsValidCoordinate(x, y) && visited[x, y] == EDirection.All)
                        neighbors.Add(new Vector2Int(x, y));
                }
            }

            if(neighbors.Count == 0)
            {
                currentCell = cellStack.Pop();
            }
            else
            {
                var randomNeighbourCoords = neighbors.GetRandomElement();
                Vector2Int dirVector = currentCell.Coords - randomNeighbourCoords;
                currentCell.BreakWall(DirectionFromVector(dirVector * -1));
                currentCell = _grid[randomNeighbourCoords.X, randomNeighbourCoords.Y];
                currentCell.BreakWall(DirectionFromVector(dirVector));
                currentCell.Occupant = new CorridorBlock(corridorID);
                currentCell.Renderer.SetColor(debugColor);

                visited[randomNeighbourCoords.X, randomNeighbourCoords.Y] = currentCell.Walls;
				cellStack.Push(currentCell);
                corridorID++;

				yield return null;
			}
        }
	}

    private bool BoxCheck(Vector2Int topLeftCoords, int width, int height)
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

    private EDirection DirectionFromVector(Vector2Int vector)
    {
        if (vector.X != 0)
            return vector.X > 0 ? EDirection.East : EDirection.West;
        else if (vector.Y != 0)
            return vector.Y > 0 ? EDirection.North : EDirection.South;
        else
            return EDirection.All;
	}

    public GridCell GetRandomCell()
    {
        return _grid[RandomUtility.Range(0, _width), RandomUtility.Range(0, _height)];
    }

    public GridCell[,] GetAdjacentCells(Vector2Int coords)
    {
        GridCell[,] neighbours = new GridCell[3, 3];
        for (int x = coords.X - 1; x <= coords.X + 1; ++x)
        {
            for (int y = coords.Y - 1; y <= coords.Y + 1; ++y)
            {
                if (!IsValidCoordinate(x, y) || x == coords.X && y == coords.Y)
                    continue;

                neighbours[x - (coords.X - 1), y - (coords.Y - 1)] = _grid[x, y];
            }
        }

        return neighbours;
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
