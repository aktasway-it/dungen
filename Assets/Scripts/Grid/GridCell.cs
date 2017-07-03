using System.Collections;
using System.Collections.Generic;

public class GridCell
{
	public int ID { get; private set; }

	public GridCellRenderer Renderer { get; set; }
    public ECellType Type { get; set; }
    public Vector2Int Coords { get; protected set; }

    private Dictionary<EDirection, EEdgeType> _edges = null;

    public GridCell(int id, ECellType type, int x, int y)
    {
        ID = id;
        Type = type;
        Coords = new Vector2Int(x, y);

        _edges = new Dictionary<EDirection, EEdgeType>() { { EDirection.North, EEdgeType.Wall }, { EDirection.South, EEdgeType.Wall }, { EDirection.East, EEdgeType.Wall }, { EDirection.West, EEdgeType.Wall } };
    }

    public void SetEdge(EDirection direction, EEdgeType edgeType)
    {
        _edges[direction] = edgeType;

		if (Renderer != null)
			Renderer.SetWalls(_edges);
    }

    public void SetEdges(Dictionary<EDirection, EEdgeType> newEdges)
    {
        _edges = newEdges;

		if (Renderer != null)
            Renderer.SetWalls(_edges);
    }

    public bool EdgeTypeCheck(EDirection direction, EEdgeType edgeType)
    {
        return _edges[direction] == edgeType;
    }

	public EDirection GetOpenEdges()
	{
        EDirection openEdges = EDirection.None;

		foreach (KeyValuePair<EDirection, EEdgeType> kvp in _edges)
		{
            if (kvp.Value == EEdgeType.None)
				openEdges |= kvp.Key;
		}

		return openEdges;
	}

    public EDirection GetWalls()
    {
        EDirection walls = EDirection.None;

        foreach(KeyValuePair<EDirection, EEdgeType> kvp in _edges)
        {
            if (kvp.Value == EEdgeType.Wall)
                walls |= kvp.Key;
        }

        return walls;
    }

    public int GetWallCount()
    {
        int wallCount = 0;

		foreach (KeyValuePair<EDirection, EEdgeType> kvp in _edges)
		{
            if (kvp.Value == EEdgeType.Wall)
                wallCount++;
		}

		return wallCount;
    }
}
