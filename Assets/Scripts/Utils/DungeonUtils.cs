using Dungeon;

namespace Utils
{
	public static class DungeonUtils
	{
		public static EDirection GetOppositeDirection(EDirection direction)
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

		public static Vector2Int VectorFromDirection(EDirection direction)
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

		public static EDirection DirectionFromVector(Vector2Int vector)
		{
			if (vector.X != 0)
				return vector.X > 0 ? EDirection.East : EDirection.West;
			else if (vector.Y != 0)
				return vector.Y > 0 ? EDirection.North : EDirection.South;
			else
				return EDirection.All;
		}


		public static Vector2Int PositionFromId(int id, int width)
		{
			return new Vector2Int(id % width, id / width);
		}

		public static int IdFromPosition(Vector2Int pos, int width)
		{
			return IdFromPosition(pos.X, pos.Y, width);
		}

		public static int IdFromPosition(int x, int y, int width)
		{
			return x + y * width;
		}
	}
}
