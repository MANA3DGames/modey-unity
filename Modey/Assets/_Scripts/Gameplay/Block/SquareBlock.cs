using System;
using UnityEngine;

namespace MANA3DGames
{
    public class SquareBlock : Block
    {
        /// <summary>
        /// Public Constructor.
        /// </summary>
        /// <param name="border">The given reference for the border instance</param>
        /// <param name="prefab">The given face prefab</param>
        /// <param name="stepTime">The given step time</param>
        /// <param name="OnLanded">The given on landing action</param>
        public SquareBlock( Border border, GameObject prefab, int ColorId, float stepTime )//, Action OnLanded ) 
            : base( border, 4, prefab, ColorId, stepTime )//, OnLanded )
        {
            int startRow = 0;

            // Set the initial position for each cell in this block.
            _border.SetTemporaryAt( startRow, 4, _cells[0], true );
            _border.SetTemporaryAt( startRow, 5, _cells[1], true );
            _border.SetTemporaryAt( startRow + 1, 4, _cells[2], true );
            _border.SetTemporaryAt( startRow + 1, 5, _cells[3], true );

            // Start to move this block down.
            //MoveDown();
            SpawnEffect();
        }

        /// <summary>
        /// Rotates this block around Z-Axis.
        /// </summary>
        /// <returns>Returns true if it was rotated successfully</returns>
        public override void Rotate()
        {
        }


        public override int GetShapeID()
        {
            return 1;
        }
    }
}