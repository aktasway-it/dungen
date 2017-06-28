public abstract class DungeonBlock 
{
	public int ID { get; private set; }
	public EDirection Walls { get; protected set; }

	public EDungeonBlockType BlockType { get; private set; }

    protected DungeonBlock(int id, EDungeonBlockType type)
    {
        ID = id;
        BlockType = type;
        Walls = EDirection.All;
    }

	public void SetWalls(EDirection newWalls)
	{
		if (newWalls == Walls)
			return;

		Walls = newWalls;
	}

	public void BreakWall(EDirection direction)
	{
		var newWalls = Walls & ~direction;

		if (newWalls == Walls)
			return;

		Walls &= ~direction;
	}
}
