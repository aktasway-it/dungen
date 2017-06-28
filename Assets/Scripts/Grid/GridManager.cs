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
    private MinMax _roomSize;

    [SerializeField, Range(0f, 1f)]
    private float _sparseness;

    [SerializeField, Range(1, 10)]
    private int _maxDoorsPerRoom = 2;

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

    private int _freeCells;
    private GridCell[,] _grid = null;
    private Dictionary<int, Room> _rooms;

    private void Start()
    {
        Job.Create(Create());
    }

    public IEnumerator Create () 
	{
		if(_useRandomSeed)
			_seed = (int) (DateTime.Now.Ticks % 100000);
		
		RandomUtility.Create(_seed);
        GenerateGrid();
        GenerateRooms();
        yield return Job.Create(GenerateCorridors(), false).StartAsRoutine();
		ConnectRooms();
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
                GridCell cell = new GridCell(cellId, x, y);
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

    private void GenerateRooms()
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

                        RoomBlock roomBlock = new RoomBlock();

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

                        _grid[x, y].Occupant = roomBlock;
						_grid[x, y].SetWalls(walls);

                        if(_useDebugVisualization)
                            _grid[x, y].Renderer.SetColor(Color.cyan);

                        room.AddBlock(IdFromPosition(x, y), roomBlock);
                        _freeCells--;
					}
				}

				roomId++;
                _rooms.Add(roomId, room);
			}

			attempts++;
        }
    }

    private IEnumerator GenerateCorridors()
    {
        EDirection[,] visited = new EDirection[_width, _height];

		// select a random starting cell
        GridCell currentCell = GetFirstFreeCell();

        // mark walls and rooms as already visited
        for (int x = 0; x < _width; ++x)
        {
            for (int y = 0; y < _width; ++y)
            {
                visited[x, y] = _grid[x, y].Occupant != null ? _grid[x, y].Occupant.Walls : EDirection.All;
            }
        }

        // mark starting cell as visited
        currentCell.Occupant = new CorridorBlock();

        if(_useDebugVisualization)
            currentCell.Renderer.SetColor(Color.magenta);

		Stack<GridCell> cellStack = new Stack<GridCell>();
        cellStack.Push(currentCell);

        Color debugColor = Color.grey;

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
                    
                    if (IsValidCoordinate(x, y) && visited[x, y] == EDirection.All)
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
                    currentCell.Occupant = new CorridorBlock();
                }
            }
            else
            {
                // pick a random neighbour cell and break the wall in that direction
                var randomNeighbourCoords = neighbors.GetRandomElement();
                Vector2Int dirVector = currentCell.Coords - randomNeighbourCoords;
                currentCell.BreakWall(DirectionFromVector(dirVector * -1));

                // change the current cell to be the new random picked one and break the wall in direction from the previous one
                currentCell = _grid[randomNeighbourCoords.X, randomNeighbourCoords.Y];
                currentCell.Occupant = new CorridorBlock();
				currentCell.BreakWall(DirectionFromVector(dirVector));

                visited[randomNeighbourCoords.X, randomNeighbourCoords.Y] = currentCell.Occupant.Walls;
				cellStack.Push(currentCell);

				if (_useDebugVisualization)
					currentCell.Renderer.SetColor(debugColor);

                _freeCells--;
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

            foreach(KeyValuePair<int, RoomBlock> roomBlockPair in boundary)
            {
                Vector2Int roomCoords = PositionFromId(roomBlockPair.Key);

                if ((roomCoords.X % 2 == 0 && roomCoords.Y % 2 == 0) || (roomCoords.Y % 2 != 0 && roomCoords.Y % 2 != 0))
                    continue;

                Vector2Int dirVector = VectorFromDirection(roomBlockPair.Value.Walls);
                EDirection wallDirection = DirectionFromVector(dirVector);

                if (dirVector.IsZero())
                    continue;
                
                Vector2Int adjacentTilePosition = roomCoords + dirVector;

                if (!IsValidCoordinate(adjacentTilePosition))
                    continue;

                _grid[adjacentTilePosition.X, adjacentTilePosition.Y].BreakWall(GetOppositeVector(wallDirection));
                _grid[roomCoords.X, roomCoords.Y].BreakWall(wallDirection);

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
                if (!IsValidCoordinate(x, y) || _grid[x, y].Occupant != null)
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

    private EDirection GetOppositeVector(EDirection direction)
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
                if (cell.Occupant == null)
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
            if (cell.Occupant == null)
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
