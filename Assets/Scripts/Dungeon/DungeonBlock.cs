public abstract class DungeonBlock 
{
	public int ID { get; private set; }
	public EDungeonBlockType BlockType { get; private set; }

    protected DungeonBlock(int id, EDungeonBlockType type)
    {
        ID = id;
        BlockType = type;
    }
}
