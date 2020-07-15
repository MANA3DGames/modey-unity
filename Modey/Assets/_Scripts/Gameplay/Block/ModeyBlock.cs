using System;
using UnityEngine;

namespace MANA3DGames
{
    public class ModeyBlock : Block
    {
        /// <summary>
        /// Public Constructor.
        /// </summary>
        /// <param name="border">The given reference for the border instance</param>
        /// <param name="prefab">The given face prefab</param>
        /// <param name="stepTime">The given step time</param>
        /// <param name="OnLanded">The given on landing action</param>
        public ModeyBlock( Border border, GameObject prefab, int ColorId, float stepTime)//, Action OnLanded ) 
            : base( border, 1, prefab, ColorId, stepTime )//, OnLanded )
        {
            // Set the initial position for each cell in this block.
            _border.SetTemporaryAt( 0, 4, _cells[0], true );

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
            return 7;
        }

        protected override void OnFinishLanding()
        {
            GameObject.Destroy( _cells[0].transform.Find( "Eye0" ).gameObject );
            GameObject.Destroy( _cells[0].transform.Find( "Eye1" ).gameObject );
            GameObject.Destroy( _cells[0].transform.Find( "Mouth" ).gameObject );

            base.OnFinishLanding();
        }
    }
}