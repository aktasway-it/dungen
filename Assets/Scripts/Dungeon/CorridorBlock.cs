using System.Collections;
using System.Collections.Generic;

public class CorridorBlock : DungeonBlock 
{
    public int ConnectionCount
    {
        get
        {
            return _connections.Count;
        }
    }

    public Dictionary<int, CorridorBlock> _connections;

    public CorridorBlock(int id) : base(id, EDungeonBlockType.Corridor) {}

    public void Connect(CorridorBlock corridorBlock)
    {
        if (!_connections.ContainsKey(corridorBlock.ID))
            _connections.Add(corridorBlock.ID, corridorBlock);
    }
}
