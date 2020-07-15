using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MANA3DGames.Utilities.Coroutine;

namespace MANA3DGames
{
    public class Border
    {
        LevelManager _levelManager;

        Row[] _rows;
        public int RowCount { get { return _rows.Length; } }

        public int RoomCount { get { return _rows[0].GetRowCount; } }

        List<GameObject> _savedModey;

        GameObject[] explosionPrefabs;
        GameObject[] _currentExplosion;
        GameObject _haloPrefab;

        CoroutineTask _checkFilledRowsTask;
        CoroutineTask _savedModeyEffectTask;

        public bool IsCheckingFilledRows { get { return _checkFilledRowsTask != null; } }




        public Border( LevelManager levelManager, int rowCount, int roomCount, 
                       GameObject emptyCellPrefab, GameObject[] explosionPrefabs, GameObject haloPrefab )
        {
            _levelManager = levelManager;

            _haloPrefab = haloPrefab;

            this.explosionPrefabs = explosionPrefabs;

            // Create a new array of GameObjects to holds the explosion particles.
            _currentExplosion = new GameObject[10];

            _savedModey = new List<GameObject>();

            _rows = new Row[rowCount];

            float yPos = 4.8f;// 4.5f;
            for ( int rowIndex = 0; rowIndex < rowCount; rowIndex++ )
            {
                _rows[rowIndex] = new Row( roomCount, rowIndex, yPos, emptyCellPrefab, _levelManager.GetBorder().transform );
                _rows[rowIndex].index = rowIndex;
                yPos -= 0.64f;
            }
        }

        public void Destroy()
        {
            if ( _checkFilledRowsTask != null )
            {
                _checkFilledRowsTask.kill();
                _checkFilledRowsTask = null;
            }

            for ( int row = 0; row < RowCount; row++ )
            {
                for ( int room = 0; room < RoomCount; room++ )
                    _rows[row].DestroyRoom( room );
            }

            while ( _savedModey.Count > 0 )
            {
                GameObject.Destroy( _savedModey[0] );
                _savedModey.RemoveAt( 0 );
            }

            DestroyExplosions();
        }


        public bool IsRoomEmpty( int rowIndex, int roomIndex )
        {
            return _rows[rowIndex].IsRoomEmpty( roomIndex );
        }

        public bool IsRoomFace( int rowIndex, int roomIndex )
        {
            return _rows[rowIndex].IsFace( roomIndex );
        }

        public int GetEmptyCellCount( int rowIndex )
        {
            int count = 0;
            for ( int i = 0; i < RoomCount; i++ )
            {
               if ( IsRoomEmpty( rowIndex, i ) )
                    count++;
            }
            return count;
        }

        public int GetMostDominionColorAtRow( int rowIndex )
        {
           int[] colors = new int[5];
            for ( int i = 0; i < RoomCount; i++ )
            {
                switch ( _rows[rowIndex].GetColorID( i ) )
                {
                    case 0:
                        colors[0]++;
                        break;
                    case 1:
                        colors[1]++;
                        break;
                    case 2:
                        colors[2]++;
                        break;
                    case 3:
                        colors[3]++;
                        break;
                    case 4:
                        colors[4]++;
                        break;
                }
            }

            int maxCountIndex = 0;
            for ( int i = 1; i < colors.Length; i++ )
            {
                if ( colors[i] > colors[maxCountIndex] )
                    maxCountIndex = i;
            }

            return maxCountIndex;
        }


        public void SetAt( int rowIndex, int roomIndex, Cell cell, bool forcePosition = false )
        {
            _rows[rowIndex].SetAt( roomIndex, cell, forcePosition );
        }

        public void SetAt( Room room, Cell cell, bool forcePosition = false )
        {
            room.cell = cell;
            cell.SetInRoom( room, forcePosition );
        }


        public void SetTemporaryAt( int rowIndex, int roomIndex, Cell cell, bool forcePosition = false )
        {
            _rows[rowIndex].SetTemporaryAt( roomIndex, cell, forcePosition );
        }

        public void SetTemporaryAt( Room room, Cell cell, bool forcePosition = false )
        {
            cell.SetInRoom( room, forcePosition );
        }


        public void FreeRoom( int rowIndex, int roomIndex )
        {
            _rows[rowIndex].FreeRoom( roomIndex );
        }

        public void BurnFace( int rowIndex, int roomIndex )
        {
            _levelManager.RemoveFace( _rows[rowIndex].GetRoom( roomIndex ).cell as Face, false );
            FreeRoom( rowIndex, roomIndex );
        }


        public bool CanSpawn
        {
            get
            {
                return _rows[0].IsAllEmpty() && 
                       _rows[1].IsAllEmpty();
            }
        }

        public bool IsCirtical
        {
            get { return !_rows[2].IsAllEmpty() || !_rows[3].IsAllEmpty(); }
        }


        public Vector2 GetPosition( int rowIndex, int roomIndex ) 
        {
            return _rows[rowIndex].GetPosition( roomIndex );
        }


        public void OnBlockLanaded( List<int> rowIDs )
        {
            _checkFilledRowsTask = new CoroutineTask( CheckFilledRowCoroutine( rowIDs ), true );
        }

        IEnumerator CheckFilledRowCoroutine( List<int> rowIDs )
        {
            List<int> newLandedRowIDs = new List<int>();

            foreach ( var id in rowIDs )
            {
                if ( _rows[id].IsAllFilled() )
                {
                    bool filledSame = _rows[id].IsAllSameFilled();

                    if ( filledSame )
                        PlayAudio( AudioLibrary.CompleteRow.OnDestroyedSameColor );
                    else
                        PlayAudio( AudioLibrary.CompleteRow.OnDestroyedDiffColors );

                    float time = 0.05f;

                    //// Get the face ID, to create the same particles colo.r
                    //int faceID = _rows[id].GetCellID( 0 );

                    // Iterate through all the rooms to destroy cells inside.
                    for ( int i = 0; i < RoomCount; i++ )
                    {
                        // Check if this cell is a face?
                        if ( _rows[id].IsFace( i ) )
                        {
                            // If this row isn't filled with the same color then keep the face.
                            if ( !filledSame )
                                continue;

                            var pos = _rows[id].GetPosition( i );
                            PlayAudio2D( AudioLibrary.ModeySFX.BeforeSaved[UnityEngine.Random.Range( 0, AudioLibrary.ModeySFX.BeforeSaved.Length )] );

                            Face face = _rows[id].GetRoom( i ).cell as Face;
                            face.SmileToCam();
                            face.IncreaseOrder( 10 );

                            _levelManager.ZoomInCamera( pos );
                            yield return new WaitForSeconds( 1.0f );

                            FreeModeyEffect( face );
                            _levelManager.AddSavedFace( face );

                            _levelManager.ZoomOutCamera();
                            yield return new WaitForSeconds( 1.0f );
                        }
                        
                        //if ( _rows[id].GetRoom( i ).cell.ColorID == 5 )
                        //    continue;

                        // Create a particle effect.
                        GameObject exp = GameObject.Instantiate( explosionPrefabs[_rows[id].GetColorID( i )], 
                                                                 _rows[id].GetPosition( i ), 
                                                                 Quaternion.identity );

                        // Add this particle effect to the array, so we can delete later on.
                        _currentExplosion[i] = exp;

                        PlayAudio( AudioLibrary.CellSFX.OnRowDestroyed[UnityEngine.Random.Range( 0, AudioLibrary.CellSFX.OnRowDestroyed.Length )] );
                        // Free this room.
                        _rows[id].FreeRoom( i );

                        // Small delay to give a better effect.
                        yield return new WaitForSeconds( time );
                    }

                    // Check block above of this cell.
                    for ( int i = 0; i < RoomCount; i++ )
                    {
                        // Check if this room still contains a face.
                        if ( !IsRoomEmpty( id, i ) )
                            continue;

                        int aboveID = id - 1;

                        List<int> rowToMoveIndices = new List<int>();

                        // Collect all the blocks above this cell.
                        while ( aboveID >= 0 && !IsRoomEmpty( aboveID, i ) )
                        {
                            rowToMoveIndices.Add( aboveID );
                            aboveID--;
                        }

                        bool sfxYes = true;
                        // Move down all the blocks above this cell.
                        foreach ( var index in rowToMoveIndices )
                        {
                            if ( sfxYes )
                                PlayAudio( AudioLibrary.CellSFX.LandingDown, 0.5f );
                            sfxYes = !sfxYes;

                            Cell cell = _rows[index].GetRoom( i ).cell;
                            var tween = LeanTween.moveY( cell.gameObject, _rows[index + 1].GetPosition( i ).y, time );
                            tween.setOnComplete( () => {
                                SetAt( index + 1, i, _rows[index].GetRoom( i ).cell, true );
                                Cell faceCell = _rows[index + 1].GetFace( cell );
                                if ( cell.IsFace && faceCell != null && cell != faceCell )
                                {
                                    PlayAudio( AudioLibrary.ModeySFX.MoveDownAnotherColor );
                                    _levelManager.ChangeCellColor( cell, faceCell.ColorID );
                                }
                                else if ( cell.IsFace )
                                    PlayAudio( AudioLibrary.ModeySFX.MoveDown );
                            } );

                            if ( !newLandedRowIDs.Contains( index + 1 ) )
                                newLandedRowIDs.Add( index + 1 );

                            // Small delay to give a better effect.
                            yield return new WaitForSeconds( time );
                        }
                    }

                    // Remove all explosion patricles.
                    DestroyExplosions( 1.0f );


                    if ( filledSame )
                        _levelManager.RewardBooster();

                    _levelManager.RewardBlockColor();
                }
            }

            CheckFilledRowAgain( newLandedRowIDs );
        }

        void FreeModeyEffect( Face face )
        {
            PlayAudio2D( AudioLibrary.ModeySFX.WhileSaving[UnityEngine.Random.Range( 0, AudioLibrary.ModeySFX.WhileSaving.Length )] );
                            
            // Fee Face Effect!!!!
            var goFace = GameObject.Instantiate( face.gameObject );
            _savedModey.Add( goFace );

            var halo = GameObject.Instantiate( _haloPrefab, goFace.transform );
            var haloTrans = halo.transform;
            haloTrans.localPosition = Vector3.zero;
            haloTrans.Find( "Halo" ).transform.localScale = Vector3.zero;
            var haloTween = LeanTween.scale( haloTrans.Find( "Halo" ).gameObject, Vector3.one, 0.5f );
            haloTween.setEaseOutBack();

            Transform[] effects = haloTrans.Find( "RandomEffect" ).gameObject.GetComponentsInChildren<Transform>();
            foreach ( var item in effects )
                item.gameObject.SetActive( false );
            haloTrans.Find( "RandomEffect" ).gameObject.SetActive( true );
            
            if ( _savedModeyEffectTask != null )
            {
                _savedModeyEffectTask.kill();
                _savedModeyEffectTask = null;
            }
            _savedModeyEffectTask = new CoroutineTask( RandomEffectsCoroutine( effects ) );

            var tween = LeanTween.moveY( goFace, 8.0f, 1.0f );
            tween.setOnComplete( ()=> {
                if ( _savedModeyEffectTask != null )
                {
                    _savedModeyEffectTask.kill();
                    _savedModeyEffectTask = null;
                }
                _savedModey.Remove( goFace );
                GameObject.Destroy( goFace );
            } );
            tween.setEaseInExpo();

            // Remove this face from the LevelManager.
            _levelManager.RemoveFace( face, true );
        }

        IEnumerator RandomEffectsCoroutine( Transform[] effects )
        {
            int index = 0;
            int lastIndex = 0;
            while ( true )
            {
                do
                {
                    index = Random.Range( 0, effects.Length );
                } while ( lastIndex == index );
                
                effects[index].gameObject.SetActive( true );

                yield return new WaitForSeconds( 0.1f );
                effects[index].gameObject.SetActive( false );
                lastIndex = index;
            }
        }


        void CheckFilledRowAgain( List<int> newLandedRowIDs )
        {
            // Check if there a new landing rows.
            if ( newLandedRowIDs.Count > 0 )
            {
                OnBlockLanaded( newLandedRowIDs );
                return;
            }

            // There is no filled rows left.
            if ( _checkFilledRowsTask != null )
            {
                _checkFilledRowsTask.kill();
                _checkFilledRowsTask = null;
            }

            AfterBlockLanded();
        }

        public void AfterBlockLanded()
        {
            _levelManager.OnBlockLanaded();
        }

        public bool Contains( Face face )
        {
            return _levelManager.ContainsFace( face );
        }


        public void OffsetAllSpritesLayerBy( int val )
        {
            for ( int row = 0; row < RowCount; row++ )
            {
                for ( int room = 0; room < RoomCount; room++ )
                {
                    if ( !IsRoomEmpty( row, room ) &&
                         _rows[row].GetRoom( room ).cell != null &&
                         _rows[row].GetRoom( room ).cell.SRenderer )
                        _rows[row].GetRoom( room ).cell.SRenderer.sortingOrder += val;
                }
            }
        }

        public void ShowAllSprites( bool show )
        {
            for ( int row = 0; row < RowCount; row++ )
            {
                for ( int room = 0; room < RoomCount; room++ )
                {
                    if ( !IsRoomEmpty( row, room ) && 
                         _rows[row].GetRoom( room ).cell != null &&
                         _rows[row].GetRoom( room ).cell.gameObject )
                        _rows[row].GetRoom( room ).cell.gameObject.SetActive( show );
                }
            }

            for ( int i = 0; i < _currentExplosion.Length; i++ )
            {
                if ( _currentExplosion[i] )
                {
                    var renderers = _currentExplosion[i].GetComponentsInChildren<SpriteRenderer>();
                    foreach ( var sprite in renderers )
                        sprite.enabled = show;
                }
            }

            for ( int i = 0; i < _savedModey.Count; i++ )
                _savedModey[i].SetActive( show );
        }


        void DestroyExplosions( float destroyTime = 0.0f )
        {
            // Remove all explosion patricles.
            for ( int i = 0; i < _currentExplosion.Length; i++ )
            {
                if ( _currentExplosion[i] )
                    GameObject.Destroy( _currentExplosion[i], destroyTime );
            }
        }

        void PauseExplosions( bool pause )
        {
            // Remove all explosion patricles.
            for ( int i = 0; i < _currentExplosion.Length; i++ )
            {
                if ( _currentExplosion[i] )
                    _currentExplosion[i].GetComponent<Animator>().speed = pause ? 0.0f : 1.0f;
            }
        }



        public void LockAllInputs( bool lockAll )
        {
            _levelManager.LockAllInputs( lockAll );
        }

        public void PlayAudio( string name, float val = 1.0f )
        {
            _levelManager.PlayAudio( name, val );
        }
        public void PlayAudioAt( string name, Vector3 pos, float val = 1.0f )
        {
            _levelManager.PlayAudioAt( name, pos, val );
        }
        public void PlayAudio2D( string name, float val = 1.0f )
        {
            _levelManager.PlayAudio2D( name, val );
        }


        public int GetRemainingCellsCount()
        {
            int count = 0;
            for ( int row = 0; row < RowCount; row++ )
            {
                for ( int room = 0; room < RoomCount; room++ )
                {
                    if ( !IsRoomEmpty( row, room ) )
                        count++;
                }
            }

            return count;
        }


        public void OnGamePaused()
        {
            if ( _checkFilledRowsTask != null )
                _checkFilledRowsTask.pause();

            if ( _savedModeyEffectTask != null )
                _savedModeyEffectTask.pause();

            PauseExplosions( true );
        }

        public void OnGameResumed()
        {
            if ( _checkFilledRowsTask != null )
                _checkFilledRowsTask.unPause();

            if ( _savedModeyEffectTask != null )
            {
                if ( _levelManager.GetFaceCount() == 0 )
                {
                    _savedModeyEffectTask.kill();
                    _savedModeyEffectTask = null;
                }
                else
                    _savedModeyEffectTask.unPause();
            }

            PauseExplosions( false );
        }
    }
}