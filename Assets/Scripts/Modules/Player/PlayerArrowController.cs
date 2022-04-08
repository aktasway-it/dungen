using System;
using Dungeon;
using UnityEngine;

namespace Modules.Player
{
    public class PlayerArrowController : MonoBehaviour 
    {
        public event Action<EDirection> onPlayerMoved;
        public DungeonCell CurrentCell { get; set; }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) && onPlayerMoved != null)
                onPlayerMoved(EDirection.North);

            if (Input.GetKeyDown(KeyCode.DownArrow) && onPlayerMoved != null)
                onPlayerMoved(EDirection.South);

            if (Input.GetKeyDown(KeyCode.LeftArrow) && onPlayerMoved != null)
                onPlayerMoved(EDirection.West);

            if (Input.GetKeyDown(KeyCode.RightArrow) && onPlayerMoved != null)
                onPlayerMoved(EDirection.East);
        }
    }
}
