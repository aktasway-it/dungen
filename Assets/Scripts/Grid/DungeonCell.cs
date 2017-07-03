using System.Collections;
using System.Collections.Generic;

public class DungeonCell
{
	public int ID { get; private set; }

    public ECellType Type { get; set; }
    public Vector2Int Coords { get; protected set; }
    public Dictionary<EDirection, EEdgeType> Edges { get; private set; }

    public DungeonCell(int id, ECellType type, int x, int y)
    {
        ID = id;
        Type = type;
        Coords = new Vector2Int(x, y);

        Edges = new Dictionary<EDirection, EEdgeType>() { { EDirection.North, EEdgeType.Wall }, { EDirection.South, EEdgeType.Wall }, { EDirection.East, EEdgeType.Wall }, { EDirection.West, EEdgeType.Wall } };
    }

    public void SetEdge(EDirection direction, EEdgeType edgeType)
    {
        Edges[direction] = edgeType;
    }

    public void SetEdges(Dictionary<EDirection, EEdgeType> newEdges)
    {
        Edges = newEdges;
    }

    public bool EdgeTypeCheck(EDirection direction, EEdgeType edgeType)
    {
        return Edges[direction] == edgeType;
    }

	public EDirection GetOpenEdges()
	{
        EDirection openEdges = EDirection.None;

		foreach (KeyValuePair<EDirection, EEdgeType> kvp in Edges)
		{
            if (kvp.Value == EEdgeType.None)
				openEdges |= kvp.Key;
		}

		return openEdges;
	}

    public EDirection GetWalls()
    {
        EDirection walls = EDirection.None;

        foreach(KeyValuePair<EDirection, EEdgeType> kvp in Edges)
        {
            if (kvp.Value == EEdgeType.Wall)
                walls |= kvp.Key;
        }

        return walls;
    }

    public int GetWallCount()
    {
        int wallCount = 0;

		foreach (KeyValuePair<EDirection, EEdgeType> kvp in Edges)
		{
            if (kvp.Value == EEdgeType.Wall)
                wallCount++;
		}

		return wallCount;
    }
}
