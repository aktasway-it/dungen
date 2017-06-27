using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCellRenderer : MonoBehaviour 
{
	[System.Serializable]
	private struct WallRenderer
	{
		public EDirection direction;
		public GameObject gameObject;
	}

    private SpriteRenderer Renderer
    {
        get
        {
            if (_renderer == null)
                _renderer = GetComponent<SpriteRenderer>();

            return _renderer;
        }
    }

	[SerializeField]
	private WallRenderer[] _walls = null;

    private SpriteRenderer _renderer = null;

    public void SetColor(Color color)
    {
        Renderer.color = color;
    }

    public void SetWalls(EDirection directions)
    {
        for (int i = 0; i < _walls.Length; ++i)
        {
            _walls[i].gameObject.SetActive((directions & (_walls[i].direction)) == _walls[i].direction);
        }
    }
}
