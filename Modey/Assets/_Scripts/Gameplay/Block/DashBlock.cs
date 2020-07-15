using System;
using UnityEngine;

namespace MANA3DGames
{
    public class DashBlock : Block
    {
        /// <summary>
        /// Public Constructor.
        /// </summary>
        /// <param name="border">The given reference for the border instance</param>
        /// <param name="prefab">The given face prefab</param>
        /// <param name="stepTime">The given step time</param>
        /// <param name="OnLanded">The given on landing action</param>
        public DashBlock( Border border, GameObject prefab, int ColorId, float stepTime)//, Action OnLanded ) 
            : base( border, 4, prefab, ColorId, stepTime)//, OnLanded )
        {
            int startRow = 1;

            // Set the initial position for each cell in this block.
            _border.SetTemporaryAt( startRow, 3, _cells[0], true );
            _border.SetTemporaryAt( startRow, 4, _cells[1], true );
            _border.SetTemporaryAt( startRow, 5, _cells[2], true );
            _border.SetTemporaryAt( startRow, 6, _cells[3], true );

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
            if ( IncompletedX_OR_Ratating_OR_PushedDown_OR_Landing ) return;

            int rowIndex = _cells[0].room.RowIndex;
            int roomIndex = _cells[0].room.RoomIndex;

            switch ( _rotateState )
            {
                case 0:
                    SetToState1( rowIndex, roomIndex );
                    break;
                case 1:
                    SetToState2( rowIndex, roomIndex );
                    break;
                case 2:
                    SetToState3( rowIndex, roomIndex );
                    break;
                case 3:
                    SetToState0( rowIndex, roomIndex );
                    break;
            }
        }


        void SetToState0( int rowIndex, int roomIndex )
        {
            // Set the rotation center to [2]
            rowIndex = _cells[2].room.RowIndex;
            roomIndex = _cells[2].room.RoomIndex;

            // Check if we can rotate?!
            //
            //  [3]
            //  [2]             [0][1][2][3]
            //  [1]   ===>      
            //  [0]
            //
            if ( roomIndex - 2 >= 0                                 &&
                 roomIndex + 1 < _border.RoomCount                  &&
                 _border.IsRoomEmpty( rowIndex    , roomIndex - 2 ) &&
                 _border.IsRoomEmpty( rowIndex    , roomIndex - 1 ) &&
                 _border.IsRoomEmpty( rowIndex    , roomIndex + 1 ) )
            {
                // Apply rotation.
                ApplyRotation( 2, 0,
                               rowIndex    , roomIndex - 2,
                               rowIndex    , roomIndex - 1,
                               rowIndex    , roomIndex    ,
                               rowIndex    , roomIndex + 1 );

                // There is no need to proceed.
                return;
            }


            // Set the rotation center to [1]
            rowIndex = _cells[1].room.RowIndex;
            roomIndex = _cells[1].room.RoomIndex;

            // Check if we can rotate?!
            //
            //  [3]
            //  [2]
            //  [1]   ===>      [0][1][2][3]
            //  [0]
            //
            if ( roomIndex - 1 >= 0                                 &&
                 roomIndex + 2 < _border.RoomCount                  &&
                 _border.IsRoomEmpty( rowIndex    , roomIndex - 1 ) &&
                 _border.IsRoomEmpty( rowIndex    , roomIndex + 1 ) &&
                 _border.IsRoomEmpty( rowIndex    , roomIndex + 2 ) )
            {
                // Apply rotation.
                ApplyRotation( 1, 0,
                               rowIndex    , roomIndex - 1,
                               rowIndex    , roomIndex    ,
                               rowIndex    , roomIndex + 1,
                               rowIndex    , roomIndex + 2 );

                // There is no need to proceed.
                return;
            }
        }

        void SetToState1( int rowIndex, int roomIndex )
        {
            // Set the rotation center to [2]
            rowIndex = _cells[2].room.RowIndex;
            roomIndex = _cells[2].room.RoomIndex;

            // Check if we can rotate?!
            //                              [0]
            //                              [1]
            //  [0][1][2][3]   ===>         [2]
            //                              [3]
            //
            if ( rowIndex - 2 >= 0                               &&
                 rowIndex + 1 < _border.RowCount                 &&
                 _border.IsRoomEmpty( rowIndex - 2 , roomIndex ) &&
                 _border.IsRoomEmpty( rowIndex - 1 , roomIndex ) &&
                 _border.IsRoomEmpty( rowIndex + 1 , roomIndex ) )
            {
                ApplyRotation( 2, 1,
                               rowIndex - 2, roomIndex    ,
                               rowIndex - 1, roomIndex    ,
                               rowIndex    , roomIndex    ,
                               rowIndex + 1, roomIndex     );

                return;
            }


            // Set the rotation center to [1]
            rowIndex = _cells[1].room.RowIndex;
            roomIndex = _cells[1].room.RoomIndex;

            //                              
            //                              [0]
            //  [0][1][2][3]   ===>         [1]
            //                              [2]
            //                              [3]
            //
            if ( rowIndex - 1 >= 0                               &&
                 rowIndex + 2 < _border.RowCount                 &&
                 _border.IsRoomEmpty( rowIndex - 1 , roomIndex ) &&
                 _border.IsRoomEmpty( rowIndex + 1 , roomIndex ) &&
                 _border.IsRoomEmpty( rowIndex + 2 , roomIndex ) )
            {
                ApplyRotation( 1, 1,
                               rowIndex - 1, roomIndex    ,
                               rowIndex    , roomIndex    ,
                               rowIndex + 1, roomIndex    ,
                               rowIndex + 2, roomIndex     );

                return;
            }
        }

        void SetToState2( int rowIndex, int roomIndex )
        {
            // Set the rotation center to [2]
            rowIndex = _cells[2].room.RowIndex;
            roomIndex = _cells[2].room.RoomIndex;

            // Check if we can rotate?!
            //
            //  [0]
            //  [1]
            //  [2]   ===>      [3][2][1][0]
            //  [3]
            //
            if ( roomIndex - 1 >= 0                                 &&
                 roomIndex + 2 < _border.RoomCount                  &&
                 _border.IsRoomEmpty( rowIndex    , roomIndex + 2 ) &&
                 _border.IsRoomEmpty( rowIndex    , roomIndex + 1 ) &&
                 _border.IsRoomEmpty( rowIndex    , roomIndex - 1 ) )
            {
                // Apply rotation.
                ApplyRotation( 2, 2,
                               rowIndex    , roomIndex + 2,
                               rowIndex    , roomIndex + 1,
                               rowIndex    , roomIndex    ,
                               rowIndex    , roomIndex - 1 );

                // There is no need to proceed.
                return;
            }


            // Set the rotation center to [1]
            rowIndex = _cells[1].room.RowIndex;
            roomIndex = _cells[1].room.RoomIndex;

            // Check if we can rotate?!
            //
            //  [0]
            //  [1]             [3][2][1][0]
            //  [2]   ===>      
            //  [3]
            //
            if ( roomIndex - 2 >= 0                                 &&
                 roomIndex + 1 < _border.RoomCount                  &&
                 _border.IsRoomEmpty( rowIndex    , roomIndex + 1 ) &&
                 _border.IsRoomEmpty( rowIndex    , roomIndex - 1 ) &&
                 _border.IsRoomEmpty( rowIndex    , roomIndex - 2 ) )
            {
                // Apply rotation.
                ApplyRotation( 1, 2,
                               rowIndex    , roomIndex + 1,
                               rowIndex    , roomIndex    ,
                               rowIndex    , roomIndex - 1,
                               rowIndex    , roomIndex - 2 );

                // There is no need to proceed.
                return;
            }
        }

        void SetToState3( int rowIndex, int roomIndex )
        {
            // Set the rotation center to [2]
            rowIndex = _cells[2].room.RowIndex;
            roomIndex = _cells[2].room.RoomIndex;

            //                              
            //                              [3]
            //  [3][2][1][0]   ===>         [2]
            //                              [1]
            //                              [0]
            //
            if ( rowIndex - 1 >= 0                               &&
                 rowIndex + 2 < _border.RowCount                 &&
                 _border.IsRoomEmpty( rowIndex + 2 , roomIndex ) &&
                 _border.IsRoomEmpty( rowIndex + 1 , roomIndex ) &&
                 _border.IsRoomEmpty( rowIndex - 1 , roomIndex ) )
            {
                ApplyRotation( 2, 3,
                               rowIndex + 2, roomIndex    ,
                               rowIndex + 1, roomIndex    ,
                               rowIndex    , roomIndex    ,
                               rowIndex - 1, roomIndex     );

                return;
            }


            // Set the rotation center to [1]
            rowIndex = _cells[1].room.RowIndex;
            roomIndex = _cells[1].room.RoomIndex;

            // Check if we can rotate?!
            //
            //                           [3]
            //                           [2]
            //  [3][2][1][0]    ===>     [1]        
            //                           [0]
            //
            if ( rowIndex - 2 >= 0                               &&
                 rowIndex + 1 < _border.RowCount                 &&
                 _border.IsRoomEmpty( rowIndex + 1 , roomIndex ) &&
                 _border.IsRoomEmpty( rowIndex - 1 , roomIndex ) &&
                 _border.IsRoomEmpty( rowIndex - 2 , roomIndex ) )
            {
                ApplyRotation( 1, 3,
                               rowIndex + 1, roomIndex    ,
                               rowIndex    , roomIndex    ,
                               rowIndex - 1, roomIndex    ,
                               rowIndex - 2, roomIndex     );

                return;
            }
        }


        public override int GetShapeID()
        {
            return 0;
        }
    }
}
