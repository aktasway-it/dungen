using System.Collections;
using System.Diagnostics;

using UnityEngine;
using Debug = UnityEngine.Debug;

public class DungeonCreator2D : MonoBehaviour 
{
	[SerializeField]
	[Range(0.1f, 5.0f)]
	private float _tileSize = 1.0f;

	[SerializeField]
	private float _spacing = 0.1f;

	[SerializeField]
	private bool _useDebugVisualization;

	[SerializeField]
	private bool _showAnimatedGeneration;

    [SerializeField]
    private DungeonConfiguration _configuration = null;

	[SerializeField]
	private GridCellRenderer _cellVisualPrefab = null;

    private Dungeon _dungeon = null;
	private GridCellRenderer[,] _gridRenderer = null;

    private void Start()
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        _dungeon = new Dungeon();
        _dungeon.Create(_configuration);
        sw.Stop();

        Debug.Log(string.Format("Dungeon creation time: {0}ms", sw.ElapsedMilliseconds));

		if (_useDebugVisualization)
            Job.Create(Draw());
    }

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.R))
			UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
	}

    private IEnumerator Draw()
	{
        Vector3 initialPos = new Vector3(-_configuration.Width * _tileSize * 0.5f, -_configuration.Height * _tileSize * 0.5f, 0);
        initialPos.x -= _spacing * (_configuration.Width - 1) * 0.5f;
        initialPos.y -= _spacing * (_configuration.Height - 1) * 0.5f;

        _gridRenderer = new GridCellRenderer[_configuration.Width, _configuration.Height];

        for (int x = 0; x < _configuration.Width; ++x)
		{
            for (int y = 0; y < _configuration.Height; ++y)
			{
                int cellId = _dungeon.Grid[x, y].ID;

				Vector3 spawnPos = new Vector3(x * (_tileSize + _spacing) + _tileSize * 0.5f, y * (_tileSize + _spacing) + _tileSize * 0.5f, 0);
				GridCellRenderer cellRenderer = Instantiate<GridCellRenderer>(_cellVisualPrefab, initialPos + spawnPos, Quaternion.identity);
				cellRenderer.transform.localScale = Vector3.one * _tileSize;
				cellRenderer.name = string.Format("Tile_{0}x{1} ({2})", x, y, cellId);
				cellRenderer.transform.parent = transform;

				_gridRenderer[x, y] = cellRenderer;

                switch (_dungeon.Grid[x, y].Type)
				{
					case ECellType.Empty:
						_gridRenderer[x, y].SetColor(Color.black);
						break;
					case ECellType.Corridor:
                        if (cellId.Equals(_dungeon.Start.ID))
							_gridRenderer[x, y].SetColor(Color.magenta);
						else
							_gridRenderer[x, y].SetColor(Color.white * 0.7f);

                        _gridRenderer[x, y].SetWalls(_dungeon.Grid[x, y].Edges);
						break;
					case ECellType.Room:
						_gridRenderer[x, y].SetColor(Color.white * 0.3f);
                        _gridRenderer[x, y].SetWalls(_dungeon.Grid[x, y].Edges);
						break;
				}

				if (_showAnimatedGeneration)
					yield return null;
			}
		}
	}
}
