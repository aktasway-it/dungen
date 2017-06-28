public abstract class DungeonBlock 
{
    public int WallCount
    {
        get
        {
			int iCount = 0;
            var wCount = Walls;

			//Loop the value while there are still bits
			while (wCount != 0)
			{
				//Remove the end bit
				wCount = wCount & (wCount - 1);

				//Increment the count
				iCount++;
			}

			//Return the count
			return iCount;
        }
    }

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
