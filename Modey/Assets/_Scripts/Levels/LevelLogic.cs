using System.Collections.Generic;
using UnityEngine;

namespace MANA3DGames
{
    public class BlockToSpawn
    {
        public int shapeID;
        public int colorID;

        public bool isBomb;

        public float speed;
    }

    public class LevelLogic
    {
        class EmptySpace
        {
            public int rowIndex;
            public int startIndex;
            public int endIndex;
            public int length;

            public EmptySpace()
            {

            }
            public EmptySpace( int rowIndex, int startIndex, int endIndex, int length )
            {
                this.rowIndex = rowIndex;
                this.startIndex = startIndex;
                this.endIndex = endIndex;
                this.length = length;
            }
        }

        LevelManager    _levelManager;

        Border          _border;

        EmptySpace      _lastEmptySpace;

        int             _failedToFindSpaceCount;

        int             _lastShapeID;



        #region [Constructors]

        public LevelLogic( LevelManager manager )
        {
            _levelManager = manager;
        }

        public void SetBorder( Border border )
        {
            _border = border;
            _failedToFindSpaceCount = 0;
        }
        public void Reset()
        {
            _lastEmptySpace = null;
            _failedToFindSpaceCount = 0;
            _lastShapeID = -1;
        }

        #endregion

        #region [Core Functions]

        int GetStartIndex( int rowIndex, int startFromIndex )
        {
            int index = -1;
            for ( int roomIndex = startFromIndex; roomIndex < _border.RoomCount; roomIndex++ )
            {
                if ( _border.IsRoomEmpty( rowIndex, roomIndex ) )
                {
                    index = roomIndex;
                    break;
                }
            }
            return index;
        }

        int GetEndIndex( int rowIndex, int startFromIndex )
        {
            int index = startFromIndex;
            for ( int roomIndex = startFromIndex; roomIndex < _border.RoomCount; roomIndex++ )
            {
                if ( !_border.IsRoomEmpty( rowIndex, roomIndex ) )
                    break;

                index++;
            }
            return index;
        }

        bool CheckDown( int rowIndex, int start, int end )
        {
            if ( rowIndex == _border.RowCount - 1 )
                return true;

            int count = 0;
            for ( int roomIndex = start; roomIndex <= end; roomIndex++ )
            {
                if ( _border.IsRoomEmpty( rowIndex + 1, roomIndex ) )
                    count++;

                if ( count > 1 || 
                     ( count == 1 && start == end ) )
                    return false;
            }

            return true;
        }

        bool CheckUp( int rowIndex, int start, int end )
        {
            for ( rowIndex--; rowIndex > 0; rowIndex-- )
            {
                for ( int roomIndex = start; roomIndex <= end; roomIndex++ )
                {
                    if ( rowIndex <= 1 )
                        return true;

                    if ( !_border.IsRoomEmpty( rowIndex, roomIndex ) )
                        return false;
                }
            }
            return true;
        }

        EmptySpace FindEmptySpaceAtRow( int rowIndex, int startRoomIndex )
        {
            int startIndex = GetStartIndex( rowIndex, startRoomIndex );
            int endIndex = GetEndIndex( rowIndex, startIndex + 1 ) - 1;
            int length = endIndex - startIndex + 1;
            return new EmptySpace( rowIndex, startIndex, endIndex, length );;
        }

        EmptySpace FindCandidateEmptySpaceAtRow( int rowIndex )
        {
            EmptySpace space = null;

            int roomIndex = 0;
            do
            {
                EmptySpace temp = FindEmptySpaceAtRow( rowIndex, roomIndex );
                roomIndex = temp.startIndex;

                if ( roomIndex != -1 )
                {
                    if ( CheckDown( rowIndex, temp.startIndex, temp.endIndex ) &&
                         CheckUp( rowIndex, temp.startIndex, temp.endIndex ) )
                    {
                        if ( space != null && temp.length < space.length )
                            space = temp;
                        else if ( space == null )
                            space = temp;
                    }

                    roomIndex = temp.endIndex + 1;
                }
            } while ( roomIndex != -1 );

            return space;
        }

        EmptySpace GetCriticalEmptySpace()
        {
            List<int>[] _indices = new List<int>[8];
            for ( int i = 0; i < _indices.Length; i++ )
                _indices[i] = new List<int>();

            for ( int i = 0; i < _border.RowCount; i++ )
            {
                int count = _border.GetEmptyCellCount( i );
                switch ( count )
                {
                    case 8:
                        _indices[7].Add( i );
                        break;
                    case 7:
                        _indices[6].Add( i );
                        break;
                    case 6:
                        _indices[5].Add( i );
                        break;
                    case 5:
                        _indices[4].Add( i );
                        break;
                    case 4:
                        _indices[3].Add( i );
                        break;
                    case 3:
                        _indices[2].Add( i );
                        break;
                    case 2:
                        _indices[1].Add( i );
                        break;
                    case 1:
                        _indices[0].Add( i );
                        break;
                }
            }

            for ( int i = 0; i < _indices.Length; i++ )
            {
                for ( int j = 0; j < _indices[i].Count; j++ )
                {
                    var space = FindCandidateEmptySpaceAtRow( _indices[i][j]  );
                    if ( space != null )
                        return space;
                }
            }

            return null;
        }

        #endregion


        bool DecideBestCase( int landID, int levelID )
        {
            int randX = Random.Range( 0, 30 );
            return randX > ( levelID + ( landID * 2 ) );
        }

        #region [ShapeID Pickers]

        public int GetBestShapeID( ref int rowIndex )
        {
            int id = Random.Range( 0, 7 );
            var space = GetCriticalEmptySpace();
            if ( space != null )
            {
                rowIndex = space.rowIndex;

                if ( _lastEmptySpace    != null &&
                     space.rowIndex     == _lastEmptySpace.rowIndex &&
                     space.startIndex   == _lastEmptySpace.startIndex &&
                     space.endIndex     == _lastEmptySpace.endIndex )
                    return id;

                switch ( space.length )
                {
                    case 4:
                        id = 0;
                        break;
                    case 3:
                        id = 2;
                        break;
                    case 2:
                        id = 1;
                        break;
                    case 1:
                        id = 7;
                        break;
                }
            }
            else
            {
                //Debug.Log( "Couldn't find a critical space" );
            }

            _lastEmptySpace = space;

            return id;
        }

        int GetWorstShapeID( ref int rowIndex )
        {
            int id = Random.Range( 0, 7 );
            var space = GetCriticalEmptySpace();
            if ( space != null )
            {
                rowIndex = space.rowIndex;

                switch ( space.length )
                {
                    case 4:
                        id = Random.Range( 4, 6 );
                        break;
                    case 3:
                        id = 0;
                        break;
                    case 2:
                        id = 2;
                        break;
                    case 1:
                        if ( space.startIndex == 0 )
                            id = 5;
                        else if ( space.endIndex == 9 )
                            id = 6;
                        else 
                            id = 1;
                        break;
                }
            }
            return id;
        }

        #endregion

        #region [ColorID Pickers]

        int GetBestColorID( int rowIndex, bool checkGood )
        {
            int id = _border.GetMostDominionColorAtRow( rowIndex );
            CheckIsGoodFaceColorID( ref id );
            return id;
        }

        int GetBestColorIDOverAll( bool checkGood )
        {
            int[] colorIDs = new int[5];
            for ( int i = 0; i < _border.RowCount; i++ )
            {
                var id = _border.GetMostDominionColorAtRow( i );
                if ( id > -1 )
                    colorIDs[id]++;
            }

            var finalID = 0;
            var maxCount = colorIDs[0];
            for ( int i = 1; i < colorIDs.Length; i++ )
            {
                if ( colorIDs[i] > maxCount )
                {
                    maxCount = colorIDs[i];
                    finalID = i;
                }  
            }

            CheckIsGoodFaceColorID( ref finalID );

            return finalID;
        }

        void CheckIsGoodFaceColorID( ref int id )
        {
            bool isGoodFace = false;
            // Check if the most dominan color is not one of the remaining faces.
            for ( int i = 0; i < _levelManager.GetFaceCount(); i++ )
            {
                if ( id == _levelManager.GetFaceColorIDAtIndex( i ) )
                {
                    isGoodFace = true;
                    break;
                }
            }

            if ( !isGoodFace )
            {
                int randVal = Random.Range( 0, 10 );
                if ( randVal != 3 && randVal != 7 )
                    id = _levelManager.GetFaceColorIDAtIndex( Random.Range( 0, _levelManager.GetFaceCount() ) );
            }
        }

        // Called from outside to get the best ID.
        public int PickupBestColorID()
        {
            if ( _levelManager.GetFaceCount() == 1 )
                return _levelManager.GetFaceColorIDAtIndex( 0 );

            int rowIndex = -1;
            GetBestShapeID( ref rowIndex );
            int colorID = 0;
            colorID = rowIndex >= 0 ? GetBestColorID( rowIndex, true ) : GetBestColorIDOverAll( true );
            return colorID;
        }

        #endregion


        #region [Final Converting]

        bool CheckNeedToConvertToBomb( int rowIndex, int spawnBombAfter )
        {
            if ( rowIndex == -1 )
                _failedToFindSpaceCount++;

            if ( _failedToFindSpaceCount >= spawnBombAfter )
            {
                _failedToFindSpaceCount = 0;
                return true;
            }

            return false;
        }

        #endregion


        public BlockToSpawn PickupNextBlock( Level level )
        {
            BlockToSpawn block = new BlockToSpawn();
            int rowIndex = -1;

            bool isBestShape = DecideBestCase( level.LandID, level.LevelID );
            block.shapeID = isBestShape ? GetBestShapeID( ref rowIndex ) : GetWorstShapeID( ref rowIndex );

            if ( !isBestShape && block.shapeID == 1 && _lastShapeID == 1 )
                block.shapeID = Random.Range( 2, 8 );

            _lastShapeID = block.shapeID;

            bool isBestCase = DecideBestCase( level.LandID, level.LevelID );
            block.colorID = rowIndex > -1 ? GetBestColorID( rowIndex, isBestCase ) : GetBestColorIDOverAll( isBestCase );
            block.isBomb = CheckNeedToConvertToBomb( rowIndex, level.SpawnBombAfter );
            return block;
        }

        
    }
}