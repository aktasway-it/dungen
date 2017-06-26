using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCellRenderer : MonoBehaviour 
{
    private SpriteRenderer Renderer
    {
        get
        {
            if (_renderer == null)
                _renderer = GetComponent<SpriteRenderer>();

            return _renderer;
        }
    }

    private SpriteRenderer _renderer = null;

    public void SetColor(Color color)
    {
        Renderer.color = color;
    }
}
