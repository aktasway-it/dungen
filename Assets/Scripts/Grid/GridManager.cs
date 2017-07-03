﻿using UnityEngine;

using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

using Debug = UnityEngine.Debug;

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
    private MinMax _roomSize;

    [SerializeField, Range(0f, 1f)]
    private float _sparseness;

    [SerializeField, Range(1, 10)]
    private int _maxDoorsPerRoom = 2;

	[SerializeField]
	private int _maxAttempts;

    [SerializeField]
    private bool _removeDeadEnds;

	[SerializeField]
	private int _seed;

	[SerializeField]
	private bool _useRandomSeed;

    [SerializeField]
    private bool _useDebugVisualization;

	[SerializeField]
	private bool _showAnimatedGeneration;

    [SerializeField]
    private GridCellRenderer _cellVisualPrefab = null;

    private int _freeCells;
    private GridCell[,] _grid = null;
	private GridCell _startTile = null;
    private Dictionary<int, Room> _rooms;

    private void Start()
    {
        Job.Create(Create());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
    }

    public IEnumerator Create () 
	{
        Stopwatch sw = new Stopwatch();

        sw.Start();
		if(_useRandomSeed)
			_seed = (int) (DateTime.Now.Ticks % 100000);
		
		RandomUtility.Create(_seed);
        GenerateGrid();
        yield return Job.Create(GenerateRooms(), false).StartAsRoutine();

        yield return Job.Create(GenerateCorridors(), false).StartAsRoutine();
		ConnectRooms();

        if(_removeDeadEnds)
            yield return Job.Create(RemoveDeadEnds(), false).StartAsRoutine();

        sw.Stop();;
        Debug.Log("Generation time: " + sw.ElapsedMilliseconds + "ms");
	}

	private void GenerateGrid()
	{
        _grid = new GridCell[_width, _height];
        _freeCells = _width * _height;

		Vector3 initialPos = new Vector3(-_width * _tileSize * 0.5f, -_height * _tileSize * 0.5f, 0);
		initialPos.x -= _spacing * (_width - 1) * 0.5f;
		initialPos.y -= _spacing * (_height - 1) * 0.5f;

        for (int x = 0; x < _width; ++x)
        {
            for (int y = 0; y < _width; ++y)
            {
                int cellId = IdFromPosition(x, y);
                GridCell cell = new GridCell(cellId, ECellType.Empty, x, y);
				_grid[x, y] = cell;

                if(_useDebugVisualization)
                {
					Vector3 spawnPos = new Vector3(x * (_tileSize + _spacing) + _tileSize * 0.5f, y * (_tileSize + _spacing) + _tileSize * 0.5f, 0);
                    GridCellRenderer cellRenderer = Instantiate<GridCellRenderer>(_cellVisualPrefab, initialPos + spawnPos, Quaternion.identity);
					cellRenderer.transform.localScale = Vector3.one * _tileSize;
                    cellRenderer.name = string.Format("Tile_{0}x{1} ({2})", x, y, cellId);
					cellRenderer.transform.parent = transform;

                    cell.Renderer = cellRenderer;
                }
            }
        }
	}

    private IEnumerator GenerateRooms()
    {
        _rooms = new Dictionary<int, Room>();

        int attempts = 0;
        int roomId = 0;
        int minRoomDistance = (_roomSize.Min + _roomSize.Max) / 2;
        minRoomDistance = Mathf.Max(1, Mathf.RoundToInt(minRoomDistance * _sparseness));

		while(attempts < _maxAttempts)
        {
            GridCell cell = GetRandomCell();
            Vector2Int roomSize = new Vector2Int(RandomUtility.Range(_roomSize.Min, _roomSize.Max + 1), RandomUtility.Range(_roomSize.Min, _roomSize.Max + 1));

            Vector2Int roomDistanceVector = new Vector2Int(minRoomDistance, minRoomDistance);

            if(BoxCheck(cell.Coords - roomDistanceVector, roomSize.X + minRoomDistance * 2, roomSize.Y + minRoomDistance * 2))
            {
                Room room = new Room(roomId, cell.Coords, roomSize);

                for (int x = cell.Coords.X; x <= cell.Coords.X + roomSize.X; ++x)
				{
                    for (int y = cell.Coords.Y; y <= cell.Coords.Y + roomSize.Y; ++y)
					{
                        if (!IsValidCoordinate(x, y))
                            continue;

                        Dictionary<EDirection, EEdgeType> walls = new Dictionary<EDirection, EEdgeType>();

                        bool isLeftEdge = x - cell.Coords.X == 0;
                        bool isRightEdge = x - (cell.Coords.X + roomSize.X) == 0;

						bool isTopEdge = y - (cell.Coords.Y + roomSize.Y) == 0;
                        bool isBottomEdge = y - cell.Coords.Y == 0;

						walls.Add(EDirection.West, isLeftEdge ? EEdgeType.Wall : EEdgeType.None);
                        walls.Add(EDirection.East, isRightEdge ? EEdgeType.Wall : EEdgeType.None);
                        walls.Add(EDirection.North, isTopEdge ? EEdgeType.Wall : EEdgeType.None);
                        walls.Add(EDirection.South, isBottomEdge ? EEdgeType.Wall : EEdgeType.None);

                        _grid[x, y].Type = ECellType.Room;
						_grid[x, y].SetEdges(walls);

                        if(_useDebugVisualization)
                            _grid[x, y].Renderer.SetColor(Color.white * 0.3f);

                        room.AddBlock(IdFromPosition(x, y), _grid[x, y]);
                        _freeCells--;
					}
				}

				roomId++;
                _rooms.Add(roomId, room);

                if (_showAnimatedGeneration)
                    yield return null;
			}

			attempts++;
        }
	}

	private IEnumerator GenerateCorridors()
    {
        bool[,] visited = new bool[_width, _height];

		// select a random starting cell
        GridCell currentCell = GetFirstFreeCell();

        _startTile = currentCell;

		// mark starting cell as visited
        currentCell.Type = ECellType.Corridor;

        _freeCells--;

        // mark walls and rooms as already visited
        for (int x = 0; x < _width; ++x)
        {
            for (int y = 0; y < _width; ++y)
            {
                visited[x, y] = _grid[x, y].Type != ECellType.Empty;
            }
        }

        if(_useDebugVisualization)
            currentCell.Renderer.SetColor(Color.magenta);

		Stack<GridCell> cellStack = new Stack<GridCell>();
        cellStack.Push(currentCell);

        Color debugColor = Color.white * 0.8f;

		List<Vector2Int> neighbors = new List<Vector2Int>();

        while (_freeCells > 0)
        {
            neighbors.Clear();

            for (int x = currentCell.Coords.X - 1; x <= currentCell.Coords.X + 1; ++x)
            {
                for (int y = currentCell.Coords.Y - 1; y <= currentCell.Coords.Y + 1; ++y)
                {
                    if (((x - currentCell.Coords.X) != 0 && (y - currentCell.Coords.Y) != 0) || (x - currentCell.Coords.X) == 0 && (y - currentCell.Coords.Y) == 0)
                        continue;
                    
                    if (IsValidCoordinate(x, y) && !visited[x, y])
                        neighbors.Add(new Vector2Int(x, y));
                }
            }

            if(neighbors.Count == 0)
            {
                if(cellStack.Count > 0)
					currentCell = cellStack.Pop();
                else
                {
                    currentCell = GetFirstFreeCell();
                    currentCell.Type = ECellType.Corridor;
                }
            }
            else
            {
                // pick a random neighbour cell and break the wall in that direction
                var randomNeighbourCoords = neighbors.GetRandomElement();
                Vector2Int dirVector = currentCell.Coords - randomNeighbourCoords;
                currentCell.SetEdge(DirectionFromVector(dirVector * -1), EEdgeType.None);

				// change the current cell to be the new random picked one and break the wall in direction from the previous one
				currentCell = _grid[randomNeighbourCoords.X, randomNeighbourCoords.Y];
                currentCell.Type = ECellType.Corridor;
                currentCell.SetEdge(DirectionFromVector(dirVector), EEdgeType.None);

                visited[currentCell.Coords.X, currentCell.Coords.Y] = true;

				cellStack.Push(currentCell);

				if (_useDebugVisualization)
					currentCell.Renderer.SetColor(debugColor);

                _freeCells--;

                if (_showAnimatedGeneration)
				    yield return null;
			}
        }
	}

    private IEnumerator RemoveDeadEnds()
    {
		List<GridCell> deadEnds = new List<GridCell>();

		for (int x = 0; x < _width; ++x)
		{
            for (int y = 0; y < _width; ++y)
            {
                var currentCell = _grid[x, y];
                if (currentCell.GetWallCount() == 3 && !currentCell.ID.Equals(_startTile.ID))
                {
                    if(_useDebugVisualization)
                        currentCell.Renderer.SetColor(Color.red);
                    
                    deadEnds.Add(currentCell);
                }
            }
        }

        foreach(GridCell deadEnd in deadEnds)
        {
            var currentCell = deadEnd;
            int attempts = 0;
            while(currentCell.GetWallCount() == 3)
            {
                if (_useDebugVisualization)
				    currentCell.Renderer.SetColor(Color.black);

                currentCell.Type = ECellType.Empty;
                var openEdge = currentCell.GetOpenEdges();

                var nextCellCoords = currentCell.Coords + VectorFromDirection(openEdge);

                currentCell = _grid[nextCellCoords.X, nextCellCoords.Y];
                currentCell.SetEdge(GetOppositeDirection(openEdge), EEdgeType.Wall);

                attempts++;

                if (attempts > 100)
                    break;

                if (_showAnimatedGeneration)
                    yield return null;
            }
        }
	}

    private void ConnectRooms()
    {
        foreach(KeyValuePair<int, Room> room in _rooms)
        {
            var boundary = room.Value.GetBounds();
            boundary.Shuffle();

            int doorsCreated = 0;
            int doorsToCreate = RandomUtility.Range(1, _maxDoorsPerRoom + 1) - room.Value.DoorCount;

            foreach(KeyValuePair<int, GridCell> roomBlockPair in boundary)
            {
                Vector2Int roomCoords = PositionFromId(roomBlockPair.Key);

                if ((roomCoords.X % 2 == 0 && roomCoords.Y % 2 == 0) || (roomCoords.Y % 2 != 0 && roomCoords.Y % 2 != 0))
                    continue;

                Vector2Int dirVector = VectorFromDirection(roomBlockPair.Value.GetWalls());
                EDirection wallDirection = DirectionFromVector(dirVector);

                if (dirVector.IsZero())
                    continue;
                
                Vector2Int adjacentTilePosition = roomCoords + dirVector;

                if (!IsValidCoordinate(adjacentTilePosition))
                    continue;

                _grid[adjacentTilePosition.X, adjacentTilePosition.Y].SetEdge(GetOppositeDirection(wallDirection), EEdgeType.Door);
                _grid[roomCoords.X, roomCoords.Y].SetEdge(wallDirection, EEdgeType.Door);

				room.Value.AddDoor();

				Room adjacentRoom = GetRoomAtPosition(adjacentTilePosition);

                if (adjacentRoom != null)
                    adjacentRoom.AddDoor();

                doorsCreated++;

                if (doorsCreated == doorsToCreate)
                    break;
            }
        }
    }

    private bool BoxCheck(Vector2Int topLeftCoords, int width, int height)
    {
        for (int x = topLeftCoords.X; x < topLeftCoords.X + width; ++x)
        {
            for (int y = topLeftCoords.Y; y < topLeftCoords.Y + height; ++y)
            {
                if (!IsValidCoordinate(x, y) || _grid[x, y].Type != ECellType.Empty)
                    return false;
            }
        }

        return true;
    }

    private Room GetRoomAtPosition(Vector2Int position)
    {
        foreach(KeyValuePair<int, Room> roomPair in _rooms)
        {
            if ((position.X >= roomPair.Value.Origin.X && position.X < roomPair.Value.Size.X) &&
               (position.Y >= roomPair.Value.Origin.Y && position.Y < roomPair.Value.Size.Y))
                return roomPair.Value;
        }

        return null;
    }

    private EDirection GetOppositeDirection(EDirection direction)
    {
		if ((direction & EDirection.East) == EDirection.East)
			return EDirection.West;

		if ((direction & EDirection.West) == EDirection.West)
			return EDirection.East;
        
        if ((direction & EDirection.North) == EDirection.North)
            return EDirection.South;

        if ((direction & EDirection.South) == EDirection.South)
            return EDirection.North;

        if (direction == EDirection.None)
            return EDirection.All;

        if (direction == EDirection.All)
            return EDirection.None;

        return EDirection.None;
    }

    private Vector2Int VectorFromDirection(EDirection direction)
    {
		if ((direction & EDirection.East) == EDirection.East)
			return new Vector2Int(1, 0);

		if ((direction & EDirection.West) == EDirection.West)
			return new Vector2Int(-1, 0);

		if ((direction & EDirection.North) == EDirection.North)
			return new Vector2Int(0, 1);

		if ((direction & EDirection.South) == EDirection.South)
			return new Vector2Int(0, -1);

		return new Vector2Int(0, 0);
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

    public GridCell GetRandomCell(bool onlyEmpty = false)
    {
        GridCell cell = null;
        while (cell == null)
		{
			cell = _grid[RandomUtility.Range(0, _width), RandomUtility.Range(0, _height)];

            if (!onlyEmpty)
                break;
            else if (onlyEmpty)
            {
                if (cell.Type == ECellType.Empty)
                    break;
                else
                    cell = null;
            }
		}

        return cell;
    }

    public GridCell GetFirstFreeCell()
    {
        foreach(GridCell cell in _grid)
        {
            if (cell.Type == ECellType.Empty)
                return cell;
        }

        return null;
    }

	public bool IsValidCoordinate(Vector2Int coords)
	{
		return IsValidCoordinate(coords.X, coords.Y);
	}

	public bool IsValidCoordinate(int x, int y)
	{
		return x >= 0 && x < _width && y >= 0 && y < _height;
	}

    public Vector2Int PositionFromId(int id)
    {
        return new Vector2Int(id % _width, id / _width);
    }

    public int IdFromPosition(Vector2Int pos)
    {
        return IdFromPosition(pos.X, pos.Y);
    }

    public int IdFromPosition(int x, int y)
	{
        return x + y * _width;
	}
}
