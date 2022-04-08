using System.Collections.Generic;
using UnityEngine;

namespace Dungeon
{
    public class DungeonCellRenderer : MonoBehaviour 
    {
        [System.Serializable]
        private struct WallRenderer
        {
            public EDirection direction;
            public SpriteRenderer spriteRenderer;
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

        public void SetWalls(Dictionary<EDirection, EEdgeType> walls)
        {
            for (int i = 0; i < _walls.Length; ++i)
            {
                _walls[i].spriteRenderer.gameObject.SetActive(walls[_walls[i].direction] != EEdgeType.None);
                _walls[i].spriteRenderer.color = walls[_walls[i].direction] == EEdgeType.Wall ? Color.black : Color.yellow;
            }
        }
    }
}
