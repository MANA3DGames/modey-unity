using System;
using UnityEngine;

namespace MANA3DGames
{
    public class TeeBlock : Block
    {
        public TeeBlock( Border border, GameObject prefab, int ColorId, float stepTime )//, Action OnLanded ) 
            : base( border, 4, prefab, ColorId, stepTime )//, OnLanded )
        {
            int startRow = 1;

            // Set the initial position for each cell in this block.
            //     [2]
            //  [1][0][3]
            _border.SetTemporaryAt( startRow, 5, _cells[0], true );
            _border.SetTemporaryAt( startRow, 4, _cells[1], true );
            _border.SetTemporaryAt( startRow - 1, 5, _cells[2], true );
            _border.SetTemporaryAt( startRow, 6, _cells[3], true );

            // Start to move this block down.
            //MoveDown();
            SpawnEffect();
        }

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
                    SetToState3(rowIndex, roomIndex);
                    break;
                case 3:
                    SetToState0(rowIndex, roomIndex);
                    break;
            }
        }


        void SetToState0( int rowIndex, int roomIndex )
        {
            // Check if we can rotate?!
            //     [3]                    [2]
            //  [2][0]       ===>      [1][0][3]
            //     [1]
            if ( roomIndex + 1 < _border.RoomCount                  &&
                 _border.IsRoomEmpty( rowIndex    , roomIndex + 1 )  )
            {
                // Apply rotation.
                ApplyRotation( 0, 0,
                               rowIndex    , roomIndex    ,
                               rowIndex    , roomIndex - 1,
                               rowIndex - 1, roomIndex    ,
                               rowIndex    , roomIndex + 1 );

                // There is no need to proceed.
                return;
            }


            // Set the rotation center to [3].
            rowIndex = _cells[3].room.RowIndex;
            roomIndex = _cells[3].room.RoomIndex;


            //                             [2]
            //     [3]                  [1][0][3]
            //  [2][0]       ===>      
            //     [1]
            // 
            if ( rowIndex - 1 >= 0                  &&
                 roomIndex - 2 >= 0                  &&
                 _border.IsRoomEmpty( rowIndex    , roomIndex - 1 ) &&
                 _border.IsRoomEmpty( rowIndex    , roomIndex - 2 ) &&
                 _border.IsRoomEmpty( rowIndex - 1, roomIndex - 1 ) )
            {
                // Apply rotation.
                ApplyRotation( 3, 0,
                               rowIndex    , roomIndex - 1,
                               rowIndex    , roomIndex - 2,
                               rowIndex - 1, roomIndex - 1,
                               rowIndex    , roomIndex     );
            }
        }

        void SetToState1( int rowIndex, int roomIndex )
        {
            // Check if we can rotate?!
            //     [2]              [1]
            //  [1][0][3]   ===>    [0][2]
            //                      [3]
            if ( rowIndex + 1 < _border.RowCount                    &&
                 _border.IsRoomEmpty( rowIndex + 1, roomIndex     ) )
            {
                ApplyRotation( 0, 1,
                               rowIndex    , roomIndex    ,
                               rowIndex - 1, roomIndex    ,
                               rowIndex    , roomIndex + 1,
                               rowIndex + 1, roomIndex     );
            }
        }

        void SetToState2( int rowIndex, int roomIndex )
        {
            // Check if we can rotate?!
            //  [1]
            //  [0][2]    ===>      [3][0][1]
            //  [3]                    [2]
            if ( roomIndex - 1 >= 0                 &&
                 _border.IsRoomEmpty( rowIndex    , roomIndex - 1 ) )
            {
                // Apply rotation.
                ApplyRotation( 0, 2,
                               rowIndex    , roomIndex    ,
                               rowIndex    , roomIndex + 1,
                               rowIndex + 1, roomIndex    ,
                               rowIndex    , roomIndex - 1 );

                return;
            }


            // Set the rotation center to [3].
            rowIndex = _cells[3].room.RowIndex;
            roomIndex = _cells[3].room.RoomIndex;


            // 
            //  [1]
            //  [0][2]    ===>      
            //  [3]                 [3][0][1]
            //                         [2]
            //
            if ( rowIndex + 1 < _border.RowCount                    &&
                 roomIndex + 2 < _border.RoomCount                  &&
                 _border.IsRoomEmpty( rowIndex    , roomIndex + 1 ) &&
                 _border.IsRoomEmpty( rowIndex    , roomIndex + 2 ) &&
                 _border.IsRoomEmpty( rowIndex + 1, roomIndex + 1 ) )
            {
                // Apply rotation.
                ApplyRotation( 3, 2,
                               rowIndex    , roomIndex + 1,
                               rowIndex    , roomIndex + 2,
                               rowIndex + 1, roomIndex + 1,
                               rowIndex    , roomIndex    );
            }
        }

        void SetToState3( int rowIndex, int roomIndex )
        {
            // Check if we can rotate?!
            //                                 [3]
            //  [3][0][1]       ===>        [2][0]        
            //     [2]                         [1]
            if ( rowIndex - 1 >= 0                    &&
                 _border.IsRoomEmpty( rowIndex - 1, roomIndex     ) )
            {
                ApplyRotation( 0, 3,
                               rowIndex    , roomIndex    ,
                               rowIndex + 1, roomIndex    ,
                               rowIndex    , roomIndex - 1,
                               rowIndex - 1, roomIndex     );
            }
        }


        public override int GetShapeID()
        {
            return 2;
        }
    }
}

