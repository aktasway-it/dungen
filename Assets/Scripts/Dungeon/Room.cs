using System.Collections.Generic;
using Utils;

namespace Dungeon
{
	public class Room
	{
		public int ID { get; private set; }
		public Vector2Int Origin { get; private set; }
		public Vector2Int Size { get; private set; }
		public List<DungeonCell> GridCells { get; private set; }
		public int DoorCount { get; private set; }

		public Room(int id, Vector2Int origin, Vector2Int size) 
		{
			ID = id;
			Origin = origin;
			Size = size;
			GridCells = new List<DungeonCell>();
		}

		public void AddCell(DungeonCell cell)
		{
			if(!GridCells.Contains(cell))
				GridCells.Add(cell);
		}

		public void AddDoor()
		{
			DoorCount++;
		}

		public List<DungeonCell> GetBounds()
		{
			List<DungeonCell> bounds = new List<DungeonCell>();

			for (int i = 0; i < GridCells.Count; ++i)
			{
				bool isLeftEdge = GridCells[i].Coords.X - Origin.X == 0;
				bool isRightEdge = GridCells[i].Coords.X - (Origin.X + Size.X) == 0;

				bool isTopEdge = GridCells[i].Coords.Y - (Origin.Y + Size.Y) == 0;
				bool isBottomEdge = GridCells[i].Coords.Y - Origin.Y == 0;

				if (!isLeftEdge && !isRightEdge && !isTopEdge && !isBottomEdge)
					continue;

				bounds.Add(GridCells[i]);
			}

			return bounds;
		}
	}
}
