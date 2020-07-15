using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MANA3DGames.Utilities.Coroutine;

namespace MANA3DGames
{
    public class LevelManager
    {
        public class ChallengeLevelData
        {
            public string json;
            public string name;

            public ChallengeLevelData( string name, string json )
            {
                this.json = json;
                this.name = name;
            }
        }

        public class LevelStatistics
        {
            public int levelDuration;
            public int boostersUseCount;
            public int paintersUseCount;
            public int rowSameColor;
            public int rowDiffColors;
        }

        #region [Variables]

        GameManager _gameManager;
        
        LevelLogic _levelLogic;
        Level   _level;
        Border  _border;
        Block   _block;

        GameObject[] _blockPrefabs;
        GameObject[] _facePrefabs;
        GameObject[] _explosionPrefabs;
        GameObject[] _emptyCellPrefabs;
        GameObject _bombPrefab;
        GameObject _bombExplosionPrefab;
        GameObject _haloPrefab;

        CoroutineTask _spawnTask;
        

        Sprite[] _blockSprites;

        public int LandID   { get { return _level.LandID; } }
        public int LevelID  { get { return _level.LevelID; } }

        public const int LEVEL_COUNT = 30;
        public const int LAND_COUNT = 3;

        public const int ROW_COUNT = 16;
        public const int COL_COUNT = 10;

        bool _usedChangeColor;

        ChallengeLevelData _challengeLevel;
        public string LevelSenderName { get {  return _challengeLevel.name; } }

        LevelStatistics _levelStatistics;

        List<Face> _savedFaces;

        List<Block> _blockList; // only one block should be there in this list.

        #endregion


        #region [Constructor]

        public LevelManager( GameManager manager )
        {
            _gameManager = manager;

            _blockPrefabs           = Resources.LoadAll<GameObject>( "Blocks" );
            _facePrefabs            = Resources.LoadAll<GameObject>( "Faces"  );
            _explosionPrefabs       = Resources.LoadAll<GameObject>( "Particles" );
            _emptyCellPrefabs       = Resources.LoadAll<GameObject>( "Grid" );
            _bombPrefab             = Resources.Load<GameObject>( "Boosters/Bomb" );
            _bombExplosionPrefab    = Resources.Load<GameObject>( "Boosters/BombExp" );
            _haloPrefab             = Resources.Load<GameObject>( "FreeModeyEffects/Halo" );

            _blockSprites = new Sprite[_blockPrefabs.Length - 1];
            for ( int i = 0; i < _blockSprites.Length; i++ )
                _blockSprites[i] = _blockPrefabs[i].GetComponent<SpriteRenderer>().sprite;

            _levelLogic = new LevelLogic( this );
        }

        #endregion

        #region [Update]

        public void Update()
        {
            if ( _block != null )
            {
                _level.UpdateFaces( _block );
            }
        }

        #endregion

        #region [Load Level]

        void CreateBorder( int landID )
        {
            CreateBlockList();

            // Select Grid sprite.
            var gridPrefab = landID == 2 ? _emptyCellPrefabs[1] : _emptyCellPrefabs[0];

            // Create a new border instance.
            _border = new Border( this, ROW_COUNT, COL_COUNT, gridPrefab, _explosionPrefabs, _haloPrefab );//, _starPrefab );
        }

        void CreateBlockList()
        {
            // This list just to handle the unknown case of having multiple blocks at the same time
            if ( _blockList != null )
            {
                while ( _blockList.Count > 0 )
                {
                    _blockList[0].Destroy();
                    _blockList.RemoveAt( 0 );
                }
                _blockList.Clear();
            }

            _blockList = new List<Block>();
        }

        void RemoveBlockFromList()
        {
            if ( _blockList != null &&
                 _blockList.Count > 0 &&
                 _blockList.Contains( _block ) )
                _blockList.Remove( _block );
            else
                Debug.Log( "Error : Block list doesn't have this block" );
        }

        public void LoadLevel( int landID, int levelID )
        {
            CreateBorder( landID );
            var json = LoadLevelJSON( landID, levelID );
            LoadLevel( json );
        }

        public void LoadLevel( ChallengeLevelData data )
        {
            CreateBorder( 0 );
            _challengeLevel = data;
            LoadLevel( data.json );
        }

        void LoadLevel( string jsonLevel )
        {
            _levelStatistics = new LevelStatistics();
            CreateLevelInstance( jsonLevel );
            foreach ( var cell in _level.ReservedCells )
            {
                if ( cell.type == 0 )
                {
                    Cell block = new Cell( GameObject.Instantiate( _blockPrefabs[cell.colorId] ), cell.colorId );
                    _border.SetAt( cell.rowIndex, cell.roomIndex, block, true );
                }
                else if ( cell.type == 1 )
                {
                    Face face = new Face( GameObject.Instantiate( _facePrefabs[cell.colorId] ), cell.colorId, this );
                    _border.SetAt( cell.rowIndex, cell.roomIndex, face, true );
                    _level.AddFace( face );
                }
            }

            _levelLogic.SetBorder( _border );

            _gameManager.SetExtraItemsForCurrentLevel( _level );

            if ( _spawnTask != null )
            {
                _spawnTask.kill();
                _spawnTask = null;
            }
            _spawnTask = new CoroutineTask( PickupNextBlock(), true );
            _spawnTask.pause();
        }

        string LoadLevelJSON( int landID, int levelID )
        {
            TextAsset file = Resources.Load( "LevelDatabase/" + landID + "/" + levelID ) as TextAsset;
            return file.ToString();
        }

        void CreateLevelInstance( string json )
        {
            LevelJSON level = JsonUtility.FromJson<LevelJSON>( json );
            _level = new Level( level.landID, level.levelID, 
                                level.stepTime, level.nextSpawnDelay, 
                                level.reservedCells.ToArray(), 
                                level.colorBlocksCount, level.boostersCount, 
                                level.spawnBombAfter );
        }


        public void RestartChallengeLevel()
        {
            //string json = JsonUtility.ToJson( _level );
            LoadLevel( _challengeLevel );
        }

        #endregion


        

        #region [Spawn Block]

        IEnumerator PickupNextBlock()
        {
            // Small delay before spawn the next block.
            yield return new WaitForSeconds( _level.NextSpawnDelay );
            
            // Get next block to be spawn from the BlockPicker.
            //BlockToSpawn nextBlock = (BlockToSpawn)typeof(LevelLogic).GetMethod( "PickupNextBlock_" + _level.LandID + "_" + _level.LevelID ).Invoke( _levelLogic, null ); //new Level[] { _level }
            BlockToSpawn nextBlock = _levelLogic.PickupNextBlock( _level );

            // Spawn next block.
            SpawnBlock( nextBlock.shapeID, nextBlock.colorID );
            if ( nextBlock.isBomb )
                _block.ConvertBlockToBomb( _bombPrefab, _explosionPrefabs[1], _bombExplosionPrefab );
        }

        void SpawnBlock( int shapeID, int prefabID )
        {
            // Check if lose level?!
            if ( prefabID < 0 )
                return;

            if ( _blockList != null )
            {
                while ( _blockList.Count > 0 )
                {
                    if ( _blockList[0] != null )
                        _blockList[0].Destroy();
                    _blockList.RemoveAt( 0 );
                }
            }

            Block block = null;

            switch ( shapeID )
            {
                case 0:
                    block = new DashBlock( _border, _blockPrefabs[prefabID], prefabID, _level.StepTime );
                    break;
                case 1:
                    block = new SquareBlock( _border, _blockPrefabs[prefabID], prefabID, _level.StepTime );
                    break;
                case 2:
                    block = new TeeBlock( _border, _blockPrefabs[prefabID], prefabID, _level.StepTime );
                    break;
                case 3:
                    block = new RightGunBlock( _border, _blockPrefabs[prefabID], prefabID, _level.StepTime );
                    break;
                case 4:
                    block = new LeftGunBlock( _border, _blockPrefabs[prefabID], prefabID, _level.StepTime );
                    break;
                case 5:
                    block = new RightSnakeBlock( _border, _blockPrefabs[prefabID], prefabID, _level.StepTime );
                    break;
                case 6:
                    block = new LeftSnakeBlock( _border, _blockPrefabs[prefabID], prefabID, _level.StepTime );
                    break;
                case 7:
                    block = new ModeyBlock( _border, _facePrefabs[prefabID], prefabID, _level.StepTime );
                    block.SetColor( _blockSprites[prefabID], prefabID );
                    break;
            }

            if ( _blockList == null )
                _blockList = new List<Block>();
            _blockList.Add( block );
            _block = block;

            PlayAudio( AudioLibrary.BlockSFX.SpawnBlock[1] );
        }

        #endregion

        #region [Move & Rotate Functions]

        public void SpeedUpMoveY()
        {
            if ( _block != null )
                _block.SpeedUpMoveY();
        }

        public void PushDown()
        {
            if ( _block != null )
                _block.PushDown();
        }

        public void MoveX( int direction )
        {
            if ( _block != null )
                _block.MoveX( direction );
        }

        public void RotateBlock()
        {
            if ( _block != null )
                _block.Rotate();
        }

        #endregion

        #region [Painters & Boosters]

        public void ChangeCellColor( Cell cell, int id )
        {
            cell.SetColor( _blockSprites[id], id );
        }

        float _lastTimePainterSFXPlayed;
        public void ChangeBlockColor( int id )
        {
            if ( _block != null )
            {
                if ( _block.GetCenterCell().ColorID == id || 
                     _block.IsBomb || _block.IsBombing )
                {
                    PlayAudio( AudioLibrary.PainterSFX.Error );
                    return;
                }

                _usedChangeColor = true;
                _block.SetColor( _blockSprites[id], id );

                _levelStatistics.paintersUseCount++;

                if ( Time.time > _lastTimePainterSFXPlayed + 1.5f )
                {
                    PlayAudio( AudioLibrary.PainterSFX.OnPressPainter );
                    _lastTimePainterSFXPlayed = Time.time;
                }
            }
            else
            {
                PlayAudio( AudioLibrary.PainterSFX.Error );
            }
        }


        public bool CanUseBooster( int id = -1 )
        {
            if ( _block == null ) return false;

            if ( id == 2 && _block.IsBomb )
                return false;

            return _block != null && 
                   !_block.IsLanding && 
                   !_block.IsBombing && 
                   !_border.IsCheckingFilledRows;
        }

        public void GenerateCustomBlock( int id )
        {
            PlayAudio( AudioLibrary.BoostersSFX.SpawnNewCustomBlock );

            RemoveBlockFromList();
            _block.Destroy();
            _block = null;

            _levelStatistics.boostersUseCount++;
            SpawnBlock( id, _levelLogic.PickupBestColorID() );
        }

        public void BurnBlock()
        {
            PlayAudio( AudioLibrary.BoostersSFX.OnPressBurner );

            int shapeId = _block.GetShapeID();
            int row = 0;
            int nextID = _levelLogic.GetBestShapeID( ref row );

            _block.Burn( _explosionPrefabs[_block.GetCenterCell().ColorID] );

            // Spawn a new Block but not the same shape as the burned one.
            while ( nextID == shapeId )
                nextID = Random.Range( 0, 7 );

            _levelStatistics.boostersUseCount++;

            SpawnBlock( nextID, _levelLogic.PickupBestColorID() );
        }

        public void ConvertBlockToBomb()
        {
            PlayAudio( AudioLibrary.BoostersSFX.OnPressBomb );

            _levelStatistics.boostersUseCount++;
            _block.ConvertBlockToBomb( _bombPrefab, _explosionPrefabs[1], _bombExplosionPrefab );
        }


        public void RewardBlockColor()
        {
            PlayAudio( AudioLibrary.CompleteRow.Reward );
            _gameManager.RewardPainter( _levelLogic.PickupBestColorID() );

            // Only if this function was called because of a complete row
            _levelStatistics.rowDiffColors++;
        }

        public void RewardBooster()
        {
            PlayAudio( AudioLibrary.CompleteRow.Reward );
            _gameManager.RewardBooster( Random.Range( 0, 3 ) );

            // Only if this function was called because of a complete row
            _levelStatistics.rowSameColor++;
        }

        #endregion

        #region [Utility Functions]

        int GetFirstAvailableBlockColorID()
        {
            return _gameManager.GetFirstAvailablePainterID();
        }

        public GameObject GetBorder()
        {
            return _gameManager.GetBorder();
        }


        public void OffsetAllSpritesLayerBy( int val )
        {
            if ( _block != null )
                _block.OffsetAllSpritesLayerBy( val );

            _border.OffsetAllSpritesLayerBy( val );
        }

        public void ShowAllSprites( bool show )
        {
            if ( _block != null )
                _block.ShowAllSprites( show );

            _border.ShowAllSprites( show );
        }


        public bool ContainsFace( Face face )
        {
            return _level.ContainsFace( face );
        }

        public void RemoveFace( Face face, bool legal )
        {
            _level.RemoveFace( face, legal );
            if ( !legal &&
                 _level.IllegalFaceCount >= _level.OriginalFaceCount / 2 )
            {
                 LoseLevel();
            }
        }


        public void ZoomInCamera( Vector2 pos, float time = 0.5f )
        {
            _gameManager.ZoomInCamera( pos, time );
        }

        public void ZoomOutCamera( float time = 0.5f )
        {
            _gameManager.ZoomOutCamera( time );
        }


        public void LockAllInputs( bool lockAll )
        {
            _gameManager.LockAllInputs( lockAll );
        }


        public void PlayAudio( string name, float val = 1.0f )
        {
            _gameManager.AudioManagerInstance.Play( name, val );
        }

        public void PlayAudioAt( string name, Vector3 pos, float val = 1.0f )
        {
            _gameManager.AudioManagerInstance.PlayAt( name, pos, val );
        }

        public void PlayAudio2D( string name, float val = 1.0f )
        {
            _gameManager.AudioManagerInstance.Play2D( name, val );
        }


        public int GetFaceCount()
        {
            return _level.FaceCount;
        }
        public int GetFaceColorIDAtIndex( int index )
        {
            return _level.GetFaceColorIDAtIndex( index );
        }

        #endregion

        #region [Event Handlers]

        public void OnBlockLanaded()
        {
            int id = 0;
            if ( _block != null )
                id = _block.GetCenterCell().ColorID;
            
            if ( _usedChangeColor )
                _gameManager.UseOnePainter( id );

            _usedChangeColor = false;

            RemoveBlockFromList();
            _block = null;

            _gameManager.OnBlockLanded();

            if ( !CanSpawnMoreBlock() )
                return;

            // Pickup a new block!
            _spawnTask = new CoroutineTask( PickupNextBlock(), true );
        }

        bool CanSpawnMoreBlock()
        {
            if ( _level.FaceCount == 0 )
            {
                if ( _level.LegalFaceCount >= _level.IllegalFaceCount )
                {
                    if ( _level.LevelID == InGameLevelBuilder.INDEX )
                        _gameManager.OnWinChallengeLevel();
                    else
                        WinLevel();
                }
                else
                {
                    if ( _level.LevelID == InGameLevelBuilder.INDEX )
                        _gameManager.OnLoseChallengeLevel( _challengeLevel.name );
                    else
                        LoseLevel();
                }   

                return false;
            }

            if ( !_border.CanSpawn )
            {
                if ( _level.LevelID == InGameLevelBuilder.INDEX )
                    _gameManager.OnLoseChallengeLevel( _challengeLevel.name );
                else
                    LoseLevel();

                return false;
            }

            if ( _border.IsCirtical )
            {
                if ( _warningTask == null )
                    _warningTask = new CoroutineTask( WarningCoroutine() );
            }

            return true;
        }

        CoroutineTask _warningTask;
        IEnumerator WarningCoroutine()
        {
            float length = _gameManager.AudioManagerInstance.GetClipLength( AudioLibrary.BlockSFX.LoseWarning );
            for ( int i = 0; i < 3; i++ )
            {
                PlayAudio( AudioLibrary.BlockSFX.LoseWarning );
                yield return new WaitForSeconds( length );
            }

            _warningTask = null;
        }


        void WinLevel()
        {
            //LevelData data = (LevelData)typeof( LevelLogic ).GetMethod( "CalculateWinLevelData_" + _level.LandID + "_" + _level.LevelID ).Invoke( _levelLogic, new Level[] { _level } );
            _gameManager.OnLevelEnd( CalculateWinLevelData() );
        }

        public void LoseLevel()
        {
            // Create lose data.
            LevelData data = new LevelData();
            data.landID = _level.LandID;
            data.levelID = _level.LevelID;
            data.score = 0;
            data.stars = 0;
            data.unlocked = 1;
            data.coinsReward = _level.LegalFaceCount * 2;
            data.bonusRewardID = -1;

            // Send the data to the gameManager to save it.
            _gameManager.OnLevelEnd( data );
        }


        public void OnGamePaused()
        {
            ShowAllSprites( false );

            if ( _block != null )
                _block.OnGamePaused();

            _border.OnGamePaused();

            _level.OnGamePaused();

            if ( _spawnTask != null )
                _spawnTask.pause();
        }

        public void OnGameResumed()
        {
            if ( _block != null )
                _block.OnGameResumed();

            _border.OnGameResumed();

            _level.OnGameResumed();

            if ( _spawnTask != null )
                _spawnTask.unPause();
        }


        public void OnEndLevel()
        {
            _usedChangeColor = false;

            if ( _block != null )
            {
                _block.Destroy();
                _block = null;
            }

            if ( _border != null )
            {
                _border.Destroy();
                _border = null;
            }
            
            if ( _level != null )
            {
                _level.Destroy();
                //_level = null;
            }
            
            if ( _spawnTask != null )
            {
                _spawnTask.kill();
                _spawnTask = null;
            }

            if ( _savedFaces != null )
            {
                while ( _savedFaces.Count > 0 )
                {
                    if ( _savedFaces[0] != null && _savedFaces[0].gameObject )
                        _savedFaces[0].FreeMemory();

                    _savedFaces.RemoveAt( 0 );
                }
            }

            _levelLogic.Reset();
        }

        #endregion

        #region [Score Calculater]

        LevelData InitLevelData( Level level )
        {
            LevelData data = new LevelData();
            data.landID = level.LandID;
            data.levelID = level.LevelID;
            data.unlocked = 1;
            return data;
        }

        LevelData CalculateWinLevelData()
        {
            LevelData data = InitLevelData( _level );

            #region [SCORE]
            data.score = 0;
            data.score += ( _levelStatistics.boostersUseCount * 2 );
            data.score += _levelStatistics.paintersUseCount;
            data.score += ( _levelStatistics.rowSameColor * 2 );
            data.score += _levelStatistics.rowDiffColors;
            data.score += ( _level.LegalFaceCount * 5 );

            int rCells = _border.GetRemainingCellsCount();
            if ( rCells == 0 )
                data.score += ( 5 * ( _level.LandID + 1 ) );
            else if ( rCells < 5 )
                data.score += ( 2 * ( _level.LandID + 1 ) );

            if ( _levelStatistics.levelDuration < 30 )
                data.score += ( 20 * ( _level.LandID + 1 ) );
            else if ( _levelStatistics.levelDuration < 60 )
                data.score += ( 10 * ( _level.LandID + 1 ) );
            else if ( _levelStatistics.levelDuration < 120 )
                data.score += ( 5 * ( _level.LandID + 1 ) );
            else if ( _levelStatistics.levelDuration < 180 )
                data.score += ( 1 * ( _level.LandID + 1 ) );

            #endregion

            #region [STARs]
            //int stars = 1; // Starts with one star at least.

            //// Land = 60s
            //// Level = 1s
            //int durationLimit = ( ( _level.LandID + 1 ) * 65 ) + ( _level.LevelID + 1 );
            //if ( _levelStatistics.levelDuration < ( durationLimit * 2 ) )
            //{
            //    if ( _levelStatistics.levelDuration < durationLimit )
            //        stars++;

            //    // Check if player has saved all modey.
            //    if ( _level.LegalFaceCount == _level.OriginalFaceCount )
            //        stars++;
            //}

            //data.stars = stars;
            data.stars = _starsCount;
            #endregion

            #region [Rewards]
            data.coinsReward = _level.LegalFaceCount * 2;
            data.bonusRewardID = Random.Range( 0, 20 ) > 10 ? Random.Range( 0, 3 ) : -1;
            #endregion

            return data;
        }

        #endregion

        //public void UpdateGameDuration( int sec )
        //{
        //    _levelStatistics.levelDuration = sec;
        //}

        int _starsCount;
        int _durationLimit;
        bool _illegalFaceStar;
        
        public void CalulcateDurationLimit()
        {
            _illegalFaceStar = false;
            _starsCount = 3;
            _gameManager.SetUIStarsCount( _starsCount );
            _durationLimit = ( ( _level.LandID + 1 ) * 60 * 3 ) + ( _level.LevelID + 1 ) * 3;
        }

        public void UpdateGameDuration( int totalSec )
        {
            bool updateUI = false;

            _levelStatistics.levelDuration = totalSec;

            if ( !_illegalFaceStar && _starsCount > 1 && _level.IllegalFaceCount > 0 )
            {
                _starsCount--;
                updateUI = true;
                _illegalFaceStar = true;
            }

            if ( _starsCount == 3 &&
                 totalSec > _durationLimit )
            {
                _starsCount = 2;
                updateUI = true;
            }
            else if ( _starsCount == 2 &&
                      totalSec > _durationLimit &&
                      _illegalFaceStar )
            {
                _starsCount = 1;
                updateUI = true;
            }
            else if ( _starsCount == 2 &&
                      totalSec > _durationLimit * 2 )
            {
                _starsCount = 1;
                updateUI = true;
            }

            if ( updateUI )
                _gameManager.SetUIStarsCount( _starsCount );
        }

        public void AddSavedFace( Face face )
        {
            if ( _savedFaces == null )
                _savedFaces = new List<Face>();

            _savedFaces.Add( face );
        }
    }
}