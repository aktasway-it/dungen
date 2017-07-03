using System;
using System.Collections.Generic;

public class Dungeon
{
    public DungeonCell[,] Grid { get; private set; }
    public DungeonCell Start { get; private set; }
    public Dictionary<int, Room> Rooms { get; private set; }

    public int Width { get { return _configuration.Width; } }
	public int Height { get { return _configuration.Height; } }

    private DungeonConfiguration _configuration = null;
    private int _freeCells;

    public void Create (DungeonConfiguration configuration) 
	{
        _configuration = configuration;

        int seed = _configuration.Seed;
        if (seed == -1)
			seed = (int) (DateTime.Now.Ticks % 100000);
		
		RandomUtility.Create(seed);

        GenerateGrid();
        GenerateRooms();
        GenerateCorridors();
		ConnectRooms();

        if (_configuration.RemoveDeadEnds)
            RemoveDeadEnds();
	}

    private void GenerateGrid()
	{
        Grid = new DungeonCell[_configuration.Width, _configuration.Height];
        _freeCells = _configuration.Width * _configuration.Height;

        for (int x = 0; x < _configuration.Width; ++x)
        {
            for (int y = 0; y < _configuration.Height; ++y)
            {
                int cellId = DungeonUtils.IdFromPosition(x, y, _configuration.Width);
                DungeonCell cell = new DungeonCell(cellId, ECellType.Empty, x, y);
				Grid[x, y] = cell;
            }
        }
	}

    private void GenerateRooms()
    {
        Rooms = new Dictionary<int, Room>();

        int attempts = 0;
        int roomId = 0;
        int minRoomDistance = (_configuration.RoomSize.Min + _configuration.RoomSize.Max) / 2;
        minRoomDistance = (int) Math.Max(1, Math.Round(minRoomDistance * _configuration.Sparseness));

        while(attempts < _configuration.MaxAttempts)
        {
            DungeonCell cell = GetRandomCell();
            Vector2Int roomSize = new Vector2Int(RandomUtility.Range(_configuration.RoomSize.Min, _configuration.RoomSize.Max + 1), RandomUtility.Range(_configuration.RoomSize.Min, _configuration.RoomSize.Max + 1));

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

                        Grid[x, y].Type = ECellType.Room;
						Grid[x, y].SetEdges(walls);

                        room.AddCell(Grid[x, y]);
                        _freeCells--;
					}
				}

				roomId++;
                Rooms.Add(roomId, room);
			}

			attempts++;
        }
	}

    private void GenerateCorridors()
    {
        bool[,] visited = new bool[_configuration.Width, _configuration.Height];

		// select a random starting cell
        DungeonCell currentCell = GetFirstFreeCell();

        Start = currentCell;

		// mark starting cell as visited
        currentCell.Type = ECellType.Corridor;

        _freeCells--;

        // mark walls and rooms as already visited
        for (int x = 0; x < _configuration.Width; ++x)
        {
            for (int y = 0; y < _configuration.Height; ++y)
            {
                visited[x, y] = Grid[x, y].Type != ECellType.Empty;
            }
        }            

		Stack<DungeonCell> cellStack = new Stack<DungeonCell>();
        cellStack.Push(currentCell);

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
                currentCell.SetEdge(DungeonUtils.DirectionFromVector(dirVector * -1), EEdgeType.None);

				// change the current cell to be the new random picked one and break the wall in direction from the previous one
				currentCell = Grid[randomNeighbourCoords.X, randomNeighbourCoords.Y];
                currentCell.Type = ECellType.Corridor;
                currentCell.SetEdge(DungeonUtils.DirectionFromVector(dirVector), EEdgeType.None);

                visited[currentCell.Coords.X, currentCell.Coords.Y] = true;

				cellStack.Push(currentCell);
                _freeCells--;
			}
        }
	}

    private void RemoveDeadEnds()
    {
		List<DungeonCell> deadEnds = new List<DungeonCell>();

        for (int x = 0; x < _configuration.Width; ++x)
		{
            for (int y = 0; y < _configuration.Height; ++y)
            {
                var currentCell = Grid[x, y];
                if (currentCell.GetWallCount() == 3 && !currentCell.ID.Equals(Start.ID))
                {
                    deadEnds.Add(currentCell);
                }
            }
        }

        foreach(DungeonCell deadEnd in deadEnds)
        {
            var currentCell = deadEnd;
            int attempts = 0;
            while(currentCell.GetWallCount() == 3)
            {
                currentCell.Type = ECellType.Empty;
                var openEdge = currentCell.GetOpenEdges();

                var nextCellCoords = currentCell.Coords + DungeonUtils.VectorFromDirection(openEdge);

                currentCell = Grid[nextCellCoords.X, nextCellCoords.Y];
                currentCell.SetEdge(DungeonUtils.GetOppositeDirection(openEdge), EEdgeType.Wall);

                attempts++;

                if (attempts > 100)
                    break;
            }
        }
	}

    private void ConnectRooms()
    {
        foreach(KeyValuePair<int, Room> room in Rooms)
        {
            var boundary = room.Value.GetBounds();
            boundary.Shuffle();

            int doorsCreated = 0;
            int doorsToCreate = RandomUtility.Range(1, _configuration.MaxDoorsPerRoom + 1) - room.Value.DoorCount;

            foreach(DungeonCell boundCell in boundary)
            {
                Vector2Int roomCoords = boundCell.Coords;

                if ((roomCoords.X % 2 == 0 && roomCoords.Y % 2 == 0) || (roomCoords.X % 2 != 0 && roomCoords.Y % 2 != 0))
                    continue;

                Vector2Int dirVector = DungeonUtils.VectorFromDirection(boundCell.GetWalls());
                EDirection wallDirection = DungeonUtils.DirectionFromVector(dirVector);

                if (dirVector.IsZero())
                    continue;

				Vector2Int adjacentTilePosition = roomCoords + dirVector;

                if (!IsValidCoordinate(adjacentTilePosition))
                    continue;

                Grid[adjacentTilePosition.X, adjacentTilePosition.Y].SetEdge(DungeonUtils.GetOppositeDirection(wallDirection), EEdgeType.Door);
                Grid[roomCoords.X, roomCoords.Y].SetEdge(wallDirection, EEdgeType.Door);

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
                if (!IsValidCoordinate(x, y) || Grid[x, y].Type != ECellType.Empty)
                    return false;
            }
        }

        return true;
    }

    private Room GetRoomAtPosition(Vector2Int position)
    {
        foreach(KeyValuePair<int, Room> roomPair in Rooms)
        {
            if ((position.X >= roomPair.Value.Origin.X && position.X < roomPair.Value.Size.X) &&
               (position.Y >= roomPair.Value.Origin.Y && position.Y < roomPair.Value.Size.Y))
                return roomPair.Value;
        }

        return null;
    }

    public DungeonCell GetRandomCell(bool onlyEmpty = false)
    {
        DungeonCell cell = null;
        while (cell == null)
		{
            cell = Grid[RandomUtility.Range(0, _configuration.Width), RandomUtility.Range(0, _configuration.Height)];

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

    public DungeonCell GetFirstFreeCell()
    {
        foreach(DungeonCell cell in Grid)
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
        return x >= 0 && x < _configuration.Width && y >= 0 && y < _configuration.Height;
	}
}
