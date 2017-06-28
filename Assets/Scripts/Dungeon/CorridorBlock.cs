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

    public CorridorBlock() : base(EDungeonBlockType.Corridor) {}
}
