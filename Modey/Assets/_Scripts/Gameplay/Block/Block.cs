using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MANA3DGames.Utilities.Coroutine;

namespace MANA3DGames
{
    public abstract class Block
    {
        #region [Variables]

        /// <summary>
        /// Reference for the Border instance.
        /// </summary>
        protected Border _border;

        /// <summary>
        /// Array of this block cells.
        /// </summary>
        protected Cell[] _cells;

        protected Cell[] _projector;

        /// <summary>
        /// Current step time.
        /// </summary>
        private float _stepTime;

        ///// <summary>
        ///// On landing action.
        ///// </summary>
        //private Action _onLandedAction;

        /// <summary>
        /// Counter for each completed move (step) down for each cell in this block.
        /// </summary>
        private int _Completed_Y_Steps_Counter;

        /// <summary>
        /// Counter for each completed move (step) aside for each cell in this block.
        /// </summary>
        private int _Completed_X_Steps_Counter;

        private int _LandingEffect_Counter;

        /// <summary>
        /// Current rotation state.
        /// </summary>
        protected int _rotateState;

        protected bool _isRotating;
        protected bool _wasPushedDown;

        protected bool IncompletedX_OR_Ratating_OR_PushedDown_OR_Landing
        {
            get {
                return  _Completed_X_Steps_Counter > 0 || 
                        _isRotating || 
                        _wasPushedDown || 
                        _LandingEffect_Counter > 0 || 
                        _bombingTask != null;
            }
        }

        public bool IsLanding { get { return _LandingEffect_Counter > 0; } }

        List<int> _rowIDs;

        float _alpha = 0.4f;

        bool _isBomb;
        public bool IsBomb { get { return _isBomb; } }

        CoroutineTask _bombingTask;
        public bool IsBombing { get { return _bombingTask != null; } }

        GameObject _bombExplosionPrefab;
        List<GameObject> _bombsExplosions;

        #endregion


        #region [Constructors]

        /// <summary>
        /// Public Constructor.
        /// </summary>
        /// <param name="border">The given reference for the border instance</param>
        /// <param name="cellCount">The given cell count in this block</param>
        /// <param name="prefab">The given face prefab</param>
        /// <param name="stepTime">The given step time</param>
        /// <param name="onLanded">The given OnLanded action</param>
        public Block( Border border, int cellCount, GameObject prefab, int ColorId, float stepTime )//, Action onLanded  )
        {
            // Init this block variables.
            _border = border;
            _border.LockAllInputs( true );

            _stepTime = stepTime;
            //_onLandedAction = onLanded;

            // Create a new set of Cells.
            _cells = new Cell[cellCount];
            for ( int i = 0; i < cellCount; i++ )
            {
                GameObject go = GameObject.Instantiate<GameObject>( prefab );
                _cells[i] = new Cell( go, ColorId );
            }

            _projector = new Cell[cellCount];
            for ( int i = 0; i < cellCount; i++ )
            {
                GameObject go = GameObject.Instantiate<GameObject>( prefab );
                _projector[i] = new Cell( go, ColorId );
                _projector[i].SetAlpha( _alpha );
            }

            _wasPushedDown = false;

            ShowAllSprites( false );
        }

        protected void SpawnEffect()
        {
            float delay = 0.0f;
            float delayInc = 0.1f;
            
            for ( int i = 0; i < _cells.Length - 1; i++ )
            {
                int temp = i;
                _cells[temp].transform.localScale = Vector2.zero;
                var tween = LeanTween.scale( _cells[temp].gameObject, Vector2.one, 0.1f );
                tween.setDelay( delay+= delayInc );
                tween.setEaseOutBack();
                LeanTween.delayedCall( delay, ()=> { _cells[temp].gameObject.SetActive( true ); } );
            }

            _cells[_cells.Length - 1].transform.localScale = Vector2.zero;
            var tween2 = LeanTween.scale( _cells[_cells.Length - 1].gameObject, Vector2.one, 0.1f );
            tween2.setDelay( delay+= delayInc );
            tween2.setEaseOutBack();
            tween2.setOnComplete( ()=> {
                _border.LockAllInputs( false );
                ShowAllSprites( true );
                MoveDown();
            } );
            LeanTween.delayedCall( delay, ()=> { _cells[_cells.Length - 1].gameObject.SetActive( true ); } );
        }


        public void Destroy()
        {
            DestroyCells();
            DestroyProjectors();
            DestroyExplosions();

            if ( _rowIDs != null )
                _rowIDs.Clear();
        }

        void DestroyCells()
        {
            //foreach ( var cell in _cells )
            //{
            //    if ( cell.gameObject )
            //        cell.FreeMemory();
            //}
            if ( _cells != null )
            {
                for ( int i = 0; i < _cells.Length; i++ )
                {
                    if ( _cells[i] != null )
                    {
                        _cells[i].FreeMemory();
                        _cells[i] = null;
                    }
                }
                _cells = null;
            }
        }

        void DestroyProjectors()
        {
            //if ( _projector != null )
            //{
            //    foreach ( var cell in _projector )
            //        cell.FreeMemory();
            //}

            if ( _projector != null )
            {
                for ( int i = 0; i < _projector.Length; i++ )
                {
                    if ( _projector[i] != null )
                    {
                        _projector[i].FreeMemory();
                        _projector[i] = null;
                    }
                }
                _projector = null;
            }
        }

        #endregion


        #region [Rotate Funcitons]

        /// <summary>
        /// Rotates this block around the Z-Axis.
        /// </summary>
        /// <returns></returns>
        public abstract void Rotate();

        protected void ApplyRotation( int centerCellIndex,
                                      int stateID,
                                      int row0, int room0,
                                      int row1, int room1,
                                      int row2, int room2,
                                      int row3, int room3 )
        {
            // Freeze all the tweens.
            foreach ( var cell in _cells )
                cell.Freeze();

            // Set the state to is Rotating so we prevent many rotation command at the same time.
            _isRotating = true;

            // Apply rotation.
            ApplyRotationEffect( centerCellIndex, ()=>
            {
                _border.SetTemporaryAt( row0, room0, _cells[0], false );
                _border.SetTemporaryAt( row1, room1, _cells[1], false );
                _border.SetTemporaryAt( row2, room2, _cells[2], false );
                _border.SetTemporaryAt( row3, room3, _cells[3], false );

                // Set new state ID.
                _rotateState = stateID;

                // Move again.
                ResumeMoveAfterRotation();

                // Now we can perform rotation again.
                _isRotating = false;
            } );

            // Draw Projector.
            DrawProjector( row0, room0,
                           row1, room1,
                           row2, room2,
                           row3, room3 );
        }

        protected void ApplyRotationEffect( int centerIndex, Action onComplete )
        {
            _border.PlayAudio( AudioLibrary.BlockSFX.Rotate[UnityEngine.Random.Range(0, AudioLibrary.BlockSFX.Rotate.Length)] );

            float time = 0.2f;

            GameObject parent = new GameObject( "BlockParent" );
            parent.transform.position = _cells[centerIndex].transform.position;

            foreach ( var cell in _cells )
                cell.transform.parent = parent.transform;

            var tween = LeanTween.rotateZ( parent, -90.0f, time );
            tween.setOnComplete( ()=>
            {
                // Make sure the rotations were set.
                foreach ( var cell in _cells )
                {
                    cell.transform.localRotation = Quaternion.Euler( 0, 0, 90.0f );
                    cell.transform.parent = null;
                }

                // Delete parent.
                GameObject.Destroy( parent );

                if ( onComplete != null )
                    onComplete.Invoke();
            });

            foreach ( var cell in _cells )
                LeanTween.rotateLocal( cell.gameObject, new Vector3( 0, 0, 90.0f ), time );
        }

        #endregion


        #region [Move Funcitons]

        /// <summary>
        /// Moves down this block.
        /// </summary>
        protected void MoveDown()
        {
            if ( _cells == null )
                return;

            // Check if we can move the block down.
            if ( CheckSafeMoveDown() )
            {
                DoMoveDown();
            }
            // We cannot move this block down.
            else
            {
                if ( _isBomb )
                    _bombingTask = new CoroutineTask( DoBombing(), true );
                else
                    DoLanding();
            }
        }

        protected void DoMoveDown()
        {
            // Reset the value of the completed cells step counter.
            _Completed_Y_Steps_Counter = _cells.Length;

            // Iterate through all our cells in this block and move them down.
            for ( int i = 0; i < _cells.Length; i++ )
            {
                // Set the current cell's room to the beneath room under its current room.
                _border.SetTemporaryAt( _cells[i].room.RowIndex + 1, _cells[i].room.RoomIndex, _cells[i] );
                    
                // Move the gameObject of the current cell to the target room postion.
                _cells[i].MoveY( _cells[i].room.Position.y, _stepTime, ()=> { OnYStepComplete(); } );
            }

            // Draw Projector.
            DrawProjector();
        }

        protected void DoLanding()
        {
            _border.PlayAudio( AudioLibrary.BlockSFX.Landing );

            //Debug.Log( "Landing!!!" );
            if ( _rowIDs != null )
                _rowIDs.Clear();

            _rowIDs = new List<int>();

            _LandingEffect_Counter = _cells.Length;

            // Iterate through all the cells array to reserve an actual room for each one of them.
            for ( int i = 0; i < _cells.Length; i++ )
            {
                // Set/Reserve a actual room for the current Cell. 
                _border.SetAt( _cells[i].room.RowIndex, _cells[i].room.RoomIndex, _cells[i], true );

                _rowIDs.Add( _cells[i].room.RowIndex );

                // Apply landing Effect.
                if ( _cells[i].room.RowIndex < _border.RowCount - 1 &&               // Check this is not the last row.
                        Have( _cells[i].room.RowIndex + 1, _cells[i].room.RoomIndex ) )    // Check if the cell beneath the current cell is one of this block cell as well. 
                {
                    // Apply landing effect with move value equal to the full scale value (Send false).
                    _cells[i].ApplyLandingEffect( false, () => { OnCompleteLandingEffect(); } );
                }
                else
                {
                    // Apply landing effect with move value equal to half of the scale value (Send true). 
                    _cells[i].ApplyLandingEffect( true, () => { OnCompleteLandingEffect(); } );
                }
            }
        }

        IEnumerator DoBombing()
        {
            DestroyProjectors();

            List<Cell> bombs = new List<Cell>( _cells );
            List<Cell> temp = new List<Cell>( _cells );
            List<Vector3> roomPositions = new List<Vector3>();

            _bombsExplosions = new List<GameObject>();

            int count = _cells.Length - 1;
            while ( temp.Count > 0 )
            {
                Cell downCell = temp[0];
                if ( temp.Count > 1 )
                {
                    for ( int i = 1; i < temp.Count; i++ )
                    {
                        if ( temp[i].room.RowIndex > downCell.room.RowIndex )
                            downCell = temp[i];
                    }
                }

                int rowIndex = downCell.room.RowIndex;
                int roomIndex = downCell.room.RoomIndex;
                int timeFactor = 1;
                while ( rowIndex + 1 < _border.RowCount && 
                        ( _border.IsRoomEmpty( rowIndex + 1, roomIndex ) || ( !_border.IsRoomEmpty( rowIndex + 1, roomIndex ) && roomPositions.Contains( _border.GetPosition( rowIndex + 1, roomIndex ) ) ) ) )
                {
                    rowIndex++;
                    timeFactor++;
                }

                if ( rowIndex < _border.RowCount - 1 )
                {
                    rowIndex++;
                    timeFactor++;
                }

                downCell.MoveY( _border.GetPosition( rowIndex, roomIndex ).y, timeFactor * _stepTime * 0.05f, ()=>
                    {
                        if ( count >= 0 )
                        {
                            _border.PlayAudio( AudioLibrary.CellSFX.OnDestroyedByBomb[count] );
                            count--;
                        }

                        GameObject exp = GameObject.Instantiate( _bombExplosionPrefab, downCell.transform.position, Quaternion.identity );
                        _bombsExplosions.Add( exp );
                        downCell.FreeMemory();
                        bombs.Remove( downCell );

                        if ( _border.IsRoomFace( rowIndex, roomIndex ) )
                        {
                            _border.PlayAudio( AudioLibrary.ModeySFX.OnDestroyed );
                            _border.BurnFace( rowIndex, roomIndex );
                        }
                        else
                            _border.FreeRoom( rowIndex, roomIndex );
                    }
                );

                roomPositions.Add( _border.GetPosition( rowIndex, roomIndex ) );
                temp.Remove( downCell );
            }

            temp.Clear();

            while ( bombs.Count > 0 )
                yield return new WaitForEndOfFrame();

            DestroyExplosions( 1.0f );

            if ( _rowIDs != null )
                _rowIDs.Clear();

            OnFinishBombing();
        }


        protected void ResumeMoveAfterRotation()
        {
            foreach ( var cell in _cells )
                cell.ResumeMoveDownAfterRotation( _stepTime, ()=> { OnYStepComplete(); } );
        }

        /// <summary>
        /// Called when a cell reaches its target room Y position (tween completed)
        /// </summary>
        void OnYStepComplete()
        {
            // Decrement the step counter.
            _Completed_Y_Steps_Counter--;

            // Check if all the cells in this block reached their target rooms. 
            if ( _Completed_Y_Steps_Counter == 0 )
                // Move down to the next room.
                MoveDown();
        }

        /// <summary>
        /// Checks whether we can move the block down or not.
        /// </summary>
        /// <returns>The return value; whether we can move this block down or not</returns>
        bool CheckSafeMoveDown()
        {
            // Iterate through all the cells in this block.
            for ( int i = 0; i < _cells.Length; i++ )
            {
                if ( _cells[i].room.RowIndex >= _border.RowCount - 1                                    // Check if this is not the last row.
                     ||
                     !_border.IsRoomEmpty( _cells[i].room.RowIndex + 1, _cells[i].room.RoomIndex ) )    // Check if whether the room beneath is not empty.
                    return false;
            }
            
            // Its safe; we can move down.
            return true;
        }

        /// <summary>
        /// Check if the current block has a cell on a spacific position.
        /// </summary>
        /// <param name="rowIndex">The given row index</param>
        /// <param name="roomIndex">The given room index</param>
        /// <returns>Return true if there a cell in the given position</returns>
        bool Have( int rowIndex, int roomIndex )
        {
            // Iterate through all the cells in this block.
            foreach ( var cell in _cells )
            {
                // Check if the given indices match one of our cells indices.
                if ( cell.room.RowIndex == rowIndex &&
                     cell.room.RoomIndex == roomIndex )
                    return true;
            }

            // There is no such a cell, return false.
            return false;
        }

        /// <summary>
        /// Increases the move down speed of this block. 
        /// </summary>
        public void SpeedUpMoveY()
        {
            if ( IncompletedX_OR_Ratating_OR_PushedDown_OR_Landing ) return;

            // Iterate through all the cells in this block and increase it's move down speed.
            foreach ( var cell in _cells )
                cell.SpeedUpMoveY();

            _border.PlayAudio( AudioLibrary.BlockSFX.PushDown );
        }

        public void PushDown()
        {
            if ( IncompletedX_OR_Ratating_OR_PushedDown_OR_Landing ) return;

            Freeze();

            // Reset the value of the completed cells step counter.
            _Completed_Y_Steps_Counter = _cells.Length;

            // Iterate through all our cells in this block and move them down.
            for ( int i = 0; i < _cells.Length; i++ )
            {
                if ( _projector != null )
                    _border.SetTemporaryAt( _projector[i].room.RowIndex, _projector[i].room.RoomIndex, _cells[i] );
                    
                // Move the gameObject of the current cell to the target room postion.
                _cells[i].MoveY( _cells[i].room.Position.y, 0.2f, ()=> { OnYStepComplete(); } );
            }

            _wasPushedDown = true;

            _border.PlayAudio( AudioLibrary.BlockSFX.FastPushDown );
        }


        public void MoveX( int direction )
        {
            // Check if we can move the block aside.
            if ( CheckSafeMoveX( direction ) )
            {
                // Reset the value of the completed cells step counter.
                _Completed_X_Steps_Counter = _cells.Length;

                // Iterate through all our cells in this block and move them aside.
                for ( int i = 0; i < _cells.Length; i++ )
                {
                    // Set the current cell's room to the side room.
                    _border.SetTemporaryAt( _cells[i].room.RowIndex, _cells[i].room.RoomIndex + direction, _cells[i] );
                    
                    // Move the gameObject of the current cell to the target room postion.
                    _cells[i].MoveX( _cells[i].room.Position.x, 0.1f, ()=> { OnXStepComplete(); } );
                }

                // Draw Projector.
                DrawProjector();

                //_border.PlayAudio( AudioLibrary.BlockSFX.SwipeLeftRight[0] );
                _border.PlayAudio( AudioLibrary.UI.Back );
            }
        }

        bool CheckSafeMoveX( int direction )
        {
            if ( IncompletedX_OR_Ratating_OR_PushedDown_OR_Landing ) return false;

            // Iterate through all the cells in this block.
            for ( int i = 0; i < _cells.Length; i++ )
            {
                // Move Right?
                if ( direction == 1 )
                {
                    if ( _cells[i].room.RoomIndex == _border.RoomCount - 1 ||
                         !_border.IsRoomEmpty( _cells[i].room.RowIndex, _cells[i].room.RoomIndex + 1 ) )
                        return false;
                }
                // Move Left.
                else if ( direction == -1 )
                {
                    if ( _cells[i].room.RoomIndex == 0 ||
                         !_border.IsRoomEmpty( _cells[i].room.RowIndex, _cells[i].room.RoomIndex - 1 ) )
                        return false;
                }
            }

            // Its safe; we can move side.
            return true;
        }

        void OnXStepComplete()
        {
            // Decrement the step counter.
            _Completed_X_Steps_Counter--;
        }

        public void Freeze()
        {
            foreach ( var cell in _cells )
                cell.Freeze();
        }


        void OnCompleteLandingEffect()
        {
            _LandingEffect_Counter--;

            if ( _LandingEffect_Counter == 0 )
                OnFinishLanding();
        }

        void OnFinishBombing()
        {
            if ( _bombingTask != null )
            {
                _bombingTask.kill();
                _bombingTask = null;
            }

            _border.AfterBlockLanded();
        }

        protected virtual void OnFinishLanding()
        {
            DestroyProjectors();
            _border.OnBlockLanaded( _rowIDs );
        }

        #endregion


        #region [Projectors]

        void DrawProjector()
        {
            if ( _cells.Length == 4 )
            {
                DrawProjector( _cells[0].room.RowIndex, _cells[0].room.RoomIndex,
                               _cells[1].room.RowIndex, _cells[1].room.RoomIndex,
                               _cells[2].room.RowIndex, _cells[2].room.RoomIndex,
                               _cells[3].room.RowIndex, _cells[3].room.RoomIndex );
            }
            else if ( _cells.Length == 1 )
            {
                DrawProjector( _cells[0].room.RowIndex, _cells[0].room.RoomIndex );
            }
        }

        void DrawProjector( int row0, int room0 )
        {
            int offset = 0;

            while ( true )
            {
                offset++;

                if ( row0 + offset >= _border.RowCount   ||
                     !_border.IsRoomEmpty( row0 + offset, room0 ) )
                    break;
            }

             offset--;
            if ( offset < 0 )
                offset = 0;

            _border.SetTemporaryAt( row0 + offset, room0, _projector[0], true );
        }

        void DrawProjector( int row0, int room0,
                            int row1, int room1,
                            int row2, int room2,
                            int row3, int room3 )
        {
            int offset = 0;

            while ( true )
            {
                offset++;

                if ( row0 + offset >= _border.RowCount   ||
                     row1 + offset >= _border.RowCount   ||
                     row2 + offset >= _border.RowCount   ||
                     row3 + offset >= _border.RowCount   ||
                     !_border.IsRoomEmpty( row0 + offset, room0 ) ||
                     !_border.IsRoomEmpty( row1 + offset, room1 ) || 
                     !_border.IsRoomEmpty( row2 + offset, room2 ) || 
                     !_border.IsRoomEmpty( row3 + offset, room3 ) )
                    break;
            }

            offset--;
            if ( offset < 0 )
                offset = 0;

            _border.SetTemporaryAt( row0 + offset, room0, _projector[0], true );
            _border.SetTemporaryAt( row1 + offset, room1, _projector[1], true );
            _border.SetTemporaryAt( row2 + offset, room2, _projector[2], true );
            _border.SetTemporaryAt( row3 + offset, room3, _projector[3], true );
        }

        #endregion


        #region [Swap Cells]
        protected void Swap( Cell cell1, Cell cell2 )
        {
            Cell temp = cell1;
            cell1 = cell2;

            cell1.transform.position = cell2.transform.position;
            cell2.transform.position = temp.transform.position;
        }

        #endregion


        #region [Utility Functions]

        public virtual Cell GetCenterCell()
        {
            return _cells[0];
        }


        public void OffsetAllSpritesLayerBy( int val )
        {
            foreach ( var cell in _cells )
            {
                if ( cell.SRenderer )
                    cell.SRenderer.sortingOrder += val;
            }

            if ( _projector != null )
            {
                foreach ( var cell in _projector )
                    cell.SRenderer.sortingOrder += val;
            }
        }

        public void ShowAllSprites( bool show )
        {
            foreach ( var cell in _cells )
            {
                if ( cell.gameObject )
                    cell.gameObject.SetActive( show );
            }

            if ( _projector != null )
            {
                foreach ( var cell in _projector )
                    cell.gameObject.SetActive( show );
            }

            if ( _bombsExplosions != null )
            {
                foreach ( var bomb in _bombsExplosions )
                {
                    var renderers = bomb.GetComponentsInChildren<SpriteRenderer>();
                    foreach ( var sprite in renderers )
                        sprite.enabled = show;
                }
            }
        }


        public void SetColor( Sprite sprite, int id )
        {
            if ( _cells != null )
            {
                foreach ( var cell in _cells )
                    cell.SetColor( sprite, id );
            }
            
            if ( _projector != null )
            {
                foreach ( var cell in _projector )
                    cell.SetColor( sprite, id );
            }
        }


        public abstract int GetShapeID();

        #endregion


        #region [Boosters]

        public void Burn( GameObject firePrefab )
        {
            BurnEffect( firePrefab );
            Destroy();
        }

        public void BurnEffect( GameObject firePrefab )
        {
            List<GameObject> _fires = new List<GameObject>( _cells.Length );

            foreach ( var cell in _cells )
            {
                GameObject go = GameObject.Instantiate<GameObject>( firePrefab, cell.transform.position, Quaternion.identity );
                _fires.Add( go );
            }

            while ( _fires.Count > 0 )
            {
                var temp = _fires[0];
                _fires.RemoveAt( 0 );
                GameObject.Destroy( temp, 0.5f );
            }

            _fires.Clear();
            _fires = null;
        }

        public void ConvertBlockToBomb( GameObject bombPrefab, GameObject firePrefab, GameObject bombExplosionPrefab  )
        {
            _bombExplosionPrefab = bombExplosionPrefab;

            BurnEffect( firePrefab );

            foreach ( var cell in _cells )
            {
                GameObject go = GameObject.Instantiate<GameObject>( bombPrefab, cell.transform.position, Quaternion.identity );
                go.transform.parent = cell.gameObject.transform;
                cell.SRenderer.enabled = false;
            }

            foreach ( var cell in _projector )
            {
                GameObject go = GameObject.Instantiate<GameObject>( bombPrefab, cell.transform.position, Quaternion.identity );
                go.transform.parent = cell.gameObject.transform;
                cell.SRenderer.enabled = false;
                cell.transform.Find( "Bomb(Clone)" ).GetComponent<SpriteRenderer>().color = new Color( 1.0f, 0.0f, 0.0f, _alpha );
                cell.transform.Find( "Bomb(Clone)" ).GetChild( 0 ).GetComponent<SpriteRenderer>().color = new Color( 1.0f, 1.0f, 1.0f, _alpha );
            }

            _isBomb = true;
        }

        #endregion


        void DestroyExplosions( float destroyTime = 0.0f )
        {
            if ( _bombsExplosions == null ) return;

            // Remove all explosion patricles.
            foreach ( var bomb in _bombsExplosions )
            {
                if ( bomb != null )
                    GameObject.Destroy( bomb, destroyTime );
            } 
        }

        void PauseExplosions( bool pause )
        {
            if ( _bombsExplosions == null ) return;

            // Remove all explosion patricles.
            for ( int i = 0; i < _bombsExplosions.Count; i++ )
            {
                if ( _bombsExplosions[i] )
                {
                    _bombsExplosions[i].GetComponent<Animator>().speed = pause ? 0.0f : 1.0f;
                }
            }
        }


        public void OnGamePaused()
        {
            if ( _bombingTask != null )
                _bombingTask.pause();

            PauseExplosions( true );
        }

        public void OnGameResumed()
        {
            if ( _bombingTask != null )
                _bombingTask.unPause();

            PauseExplosions( false );
        }
    }
}
