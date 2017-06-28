public abstract class DungeonBlock 
{
	public EDirection Walls { get; protected set; }

	public EDungeonBlockType BlockType { get; private set; }

    protected DungeonBlock(EDungeonBlockType type)
    {
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
