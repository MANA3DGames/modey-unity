using System;
using System.Collections.Generic;
using UnityEngine;

namespace MANA3DGames
{
	public class InGameLevelBuilder
	{
        public class InGameColorEditMenu
	    {
		    GameObject gameObject;
		    public Transform transform;

		    InGameDummyCell _currentBlock;

            InGameLevelBuilder _inGameLevelBuilderEditor;

            int[] _colorBalance;
            public int[] ColorBalance { get { return _colorBalance; } }
            public int TotalBalance
            {
                get
                {
                    int total = 0;
                    for ( int i = 0; i < _colorBalance.Length; i++ )
                        total += _colorBalance[i];
                    return total;
                }
            }

            int _faceBalance;
            public int FaceBalance { get { return _faceBalance; } }
            public bool CanAddFace { get { return _faceBalance > 0; } }

            GOGroup _group;


		    public InGameColorEditMenu( GameObject go, UIManager uiManager, InGameLevelBuilder inGameLevelBuilderEditor )
		    {
                _inGameLevelBuilderEditor = inGameLevelBuilderEditor;

			    gameObject = go;
			    transform = gameObject.transform;

                _group = new GOGroup( gameObject );

			    for ( int i = 0; i <= 7; i++ )
			    {
				    int temp = i;
                    uiManager.AddOnClickListener( "LevelBuilderEditor_" + temp + "Btn", ()=> {
                        _currentBlock.SetVal( temp );
					    Close();
                    } );
			    }

                //gameObject.SetActive( false );

                Reset();
		    }

		    public void Show( InGameDummyCell cell )
		    {
                if ( _currentBlock != null )
                    _inGameLevelBuilderEditor.ResetSelectedCells( _currentBlock );

			    _currentBlock = cell;

                transform.localPosition = new Vector2( 0.0f, cell.position.y - 0.64f );

			    gameObject.SetActive( true );

                _inGameLevelBuilderEditor.HighlightSelectedCells( cell );
		    }

		    public void Close()
		    {
			    gameObject.SetActive( false );
                if ( _currentBlock != null )
                    _inGameLevelBuilderEditor.ResetSelectedCells( _currentBlock );
                _currentBlock = null;
		    }

		    public Sprite GetSprite( int id )
		    {
			    return _group.GetSpriteRenderer( "LevelBuilderEditor_" + id + "Btn" ).sprite;
		    }

            void UpdateBalanceUI( int id )
            {
                _group.SetText( "LevelBuilderEditor_" + id + "Btn", _colorBalance[id].ToString(), true );
            }

            public void UpdateBalanceValue( int id, int val )
            {
                _colorBalance[id] += val;
                UpdateBalanceUI( id );
            }

            public void UpdateFaceBalance( int val )
            {
                _faceBalance += val;
            }


            public void RemoveOtherFacesInTheSameRow( InGameDummyCell cell )
            {
                _inGameLevelBuilderEditor.RemoveOtherFacesInTheSameRow( cell );
            }

            public void Reset()
            {
                _colorBalance = new int[6];
                for ( int i = 0; i < 5; i++ )
                {
                    _colorBalance[i] = 10;
                    UpdateBalanceUI( i );
                }
                _colorBalance[5] = 7;
                UpdateBalanceUI( 5 );

                _faceBalance = 5;

                Close();
            }
	    }

	    public class InGameDummyCell
	    {
		    public int rowID;
		    public int roomID;

		    public int colorID;
		    public int typeID;

		    public Vector2 position;

            SpriteRenderer _image;
		    InGameColorEditMenu _editMenu;

            GameObject gameObject;
		    GameObject _face;

            BoxCollider2D collider;


		    public InGameDummyCell( int rowId, int roomId, 
                                GameObject prefab, Vector2 pos, Transform border, 
                                InGameColorEditMenu editMenu, UIManager uiManager )
		    {
			    colorID = -1;
			    typeID = 0;

			    rowID = rowId;
			    roomID = roomId;
			    position = pos;

			    _editMenu = editMenu;

			    gameObject = GameObject.Instantiate<GameObject>( prefab, border );
                gameObject.name = "InGameLevelBuilderEditorGrid_" + rowID + "_" + roomID + "btn";
			    gameObject.GetComponent<Transform>().localPosition = position;

                collider = gameObject.GetComponent<BoxCollider2D>();
			    _image = gameObject.GetComponent<SpriteRenderer>();
			    _face = gameObject.transform.Find( "Face" ).gameObject;
                _face.SetActive( false );

                if ( rowID >= 4 )
				    uiManager.AddOnClickListener( gameObject.name, ()=> { _editMenu.Show( this ); } );
                else
                {
                    _image.color = Color.gray;
                    GameObject.Destroy( gameObject.GetComponent<BoxCollider2D>() );
                }
		    }


            int GetFirstAvailableColorID()
            {
                for ( int i = 0; i < _editMenu.ColorBalance.Length - 1; i++ )
                {
                    if ( _editMenu.ColorBalance[i] > 0 )
                        return i;
                }

                return -1;
            }

		    public void SetVal( int val )
		    {
                if ( val <= 5 && _editMenu.ColorBalance[val] < 1 )
                    return;

                if ( val != colorID && colorID >= 0 && val <= 5 )
                    _editMenu.UpdateBalanceValue( colorID, 1 );

			    if ( val <= 5 )
			    {
				    colorID = val;
				    _image.sprite = _editMenu.GetSprite( colorID );

				    if ( val == 5 )
				    {
					    RemoveFace();
				    }

                    _editMenu.UpdateBalanceValue( colorID, -1 );
			    }
			    else if ( val == 6  )
			    {
                    if ( colorID >= 0 )
                        _editMenu.UpdateBalanceValue( colorID, 1 );

                    //if ( typeID == 1 )
                    //    _editMenu.UpdateFaceBalance( 1 );

				    EmptyCell();
			    }
			    else if ( val == 7  )
			    {
				    if ( typeID == 1 )
				    {
					    RemoveFace();
				    }
				    else if ( _editMenu.CanAddFace )
				    {
					    if ( colorID == -1 || colorID == 5 )
					    {
                            int colorIDTemp = colorID;
						    colorID = GetFirstAvailableColorID();

                            if ( colorID == -1 )
                            {
                                Debug.Log( "Balance Zero!!!!" );
                                return;
                            }

                            if ( colorIDTemp == 5 )
                                _editMenu.UpdateBalanceValue( 5, 1 );

						    _image.sprite = _editMenu.GetSprite( colorID );
                            _editMenu.UpdateBalanceValue( colorID, -1 );
					    }

					    SetFace();
				    }
			    }
		    }

		    public void EmptyCell()
		    {
			    colorID = -1;
			    //typeID = 0;
			    _image.sprite = _editMenu.GetSprite( 6 );
			    //_face.SetActive( false );
                RemoveFace();
		    }

		    public void SetFace()
		    {
                if ( typeID == 0 )
                    _editMenu.UpdateFaceBalance( -1 );

			    typeID = 1;
			    _face.SetActive( true );
                _editMenu.RemoveOtherFacesInTheSameRow( this );
		    }

		    public void RemoveFace()
		    {
                if ( typeID == 1 )
                    _editMenu.UpdateFaceBalance( 1 );

			    typeID = 0;
			    _face.SetActive( false );
		    }


            public void AdjustLayerOrderBy( int val )
            {
                _image.sortingOrder += val;
                for ( int i = 0; i < 2; i++ )
                {
                    _face.transform.Find( "Eye" + i ).GetComponent<SpriteRenderer>().sortingOrder += val;
                    _face.transform.Find( "Eye" + i ).Find( "Ball" ).GetComponent<SpriteRenderer>().sortingOrder += val;
                    _face.transform.Find( "Eye" + i ).Find( "EyeLashes" + i ).GetComponent<SpriteRenderer>().sortingOrder += val;
                    _face.transform.Find( "Mouth" ).GetComponent<SpriteRenderer>().sortingOrder += val;
                }
            }

            public void EnableCollider( bool enable )
            {
                collider.enabled = enable;
            }
	    }
	

        GOGroup _group;

		InGameColorEditMenu _editMenu;

		InGameDummyCell[,] _cells;

        GameManager _gameManager;
        UIManager _uiManager;
        
        public const int INDEX = -100;


		public InGameLevelBuilder( GameObject rootGO, UIManager uiManager, GameManager gameManager )
		{
            _gameManager = gameManager;
            _uiManager = uiManager;
            _group = new GOGroup( rootGO );
			_editMenu = new InGameColorEditMenu( _group.Get( "EditMenu" ), uiManager, this );

			CreateGridBorder( _group.Get( "btnPrefab" ), _group.Get( "Border" ).transform, uiManager );

            uiManager.AddOnClickListener( "LevelBuilderEditor_InviteBtn", OnClickInviteBtn );
            uiManager.AddOnClickListener( "LevelBuilderEditor_ClearBtn", OnClickClearBtn );
            uiManager.AddOnClickListener( "LevelBuilderEditor_BackBtn", OnClickBackBtn );
		}

		void CreateGridBorder( GameObject btnPrefab, Transform border, UIManager uiManager )
		{
			_cells = new InGameDummyCell[16,10];

            float x = -2.88f;
			Vector2 pos = new Vector2( x, 5.44f );


			for ( int i = 0; i < 16; i++ ) 
			{
				for ( int j = 0; j < 10; j++ )
				{
					_cells[i, j] = CreateDummyCell( btnPrefab, border, i, j, pos, uiManager );
					pos.x += 0.64f;
				}
                pos.x = x;
				pos.y -= 0.64f;
			}
		}

		InGameDummyCell CreateDummyCell( GameObject btnPrefab, Transform border, int rowId, int roomId, Vector2 pos, UIManager uiManager )
		{
			return new InGameDummyCell( rowId, roomId, btnPrefab, pos, border, _editMenu, uiManager );
		}



		string CreateJSONFile()
		{
			LevelJSON level = new LevelJSON();
			level.levelID = INDEX;
			level.landID = INDEX;
            level.stepTime = 2.0f;
            level.nextSpawnDelay = 0.5f;
            level.spawnBombAfter = 3;

			level.colorBlocksCount = new int[5];
			for ( int i = 0; i < 5; i++ ) 
				level.colorBlocksCount[i] = 3;

			level.boostersCount = new int[3];
			for ( int i = 0; i < 3; i++ ) 
				level.boostersCount[i] = 1;
			
			level.reservedCells = new List<ReservedCell>();
			foreach ( var cell in _cells )
			{
				if ( cell.colorID != -1 )
					level.reservedCells.Add( new ReservedCell( cell.typeID, cell.colorID, cell.rowID, cell.roomID ) );
			}

			return JsonUtility.ToJson( level );
		}

        void OnClickInviteBtn()
        {
            _group.ShowRoot( false );

            if ( _editMenu.FaceBalance >= 5 )
            {
                _uiManager.DisplayMessageDialog( "Add Modey!!", 
                                                 "Add At least one Modey to your level.",
                                                 UIManager.MessageDialogAction.OK,
                                                 ()=> { _group.ShowRoot( true ); } );
                return;
            }

            if ( _editMenu.TotalBalance > 47 )
            {
                _uiManager.DisplayMessageDialog( "Add More Block!!", 
                                                 "Spend at least 10 block on your level.",
                                                 UIManager.MessageDialogAction.OK,
                                                 ()=> { _group.ShowRoot( true ); } );
                return;
            }
            
            _uiManager.DisplayLoadingScreen( "Baking Level\nPlease Wait..." );
            _gameManager.UploadJSONToGameSparks( CreateJSONFile() );
        }

		void OnClickClearBtn()
		{
			foreach ( var cell in _cells )
				cell.EmptyCell();

            _editMenu.Reset();
		}

        public void OnClickBackBtn()
        {
            _editMenu.Close();
            _group.ShowRoot( false );
            _uiManager.DisplayFacebookMenu();
        }



        public void Display()
        {
            _group.ShowRoot( true );
        }


        public void HighlightSelectedCells( InGameDummyCell cell )
        {
            _group.ShowItem( "Msg", false );

            _group.AdjustSpriteLayerOrderBy( "BG", 5 );
            cell.AdjustLayerOrderBy( 5 );

            if ( cell.rowID < 15 )
            {
                for ( int i = 0; i < 10; i++ )
                    _cells[cell.rowID+1, i].EnableCollider( false );
            }

            EnableButtons( false );
        }

        public void ResetSelectedCells( InGameDummyCell cell )
        {
            _group.AdjustSpriteLayerOrderBy( "BG", -5 );
            cell.AdjustLayerOrderBy( -5 );

            if ( cell.rowID < 15 )
            {
                for ( int i = 0; i < 10; i++ )
                    _cells[cell.rowID+1, i].EnableCollider( true );
            }

            EnableButtons( true );
        }

        void EnableButtons( bool enable )
        {
            _group.Get( "LevelBuilderEditor_InviteBtn" ).GetComponent<Collider2D>().enabled = enable;
            _group.Get( "LevelBuilderEditor_ClearBtn" ).GetComponent<Collider2D>().enabled = enable;
            _group.Get( "LevelBuilderEditor_BackBtn" ).GetComponent<Collider2D>().enabled = enable;
        }


        public void RemoveOtherFacesInTheSameRow( InGameDummyCell cell )
        {
            for ( int i = 0; i < 10; i++ )
            {
                if ( i == cell.roomID )
                    continue;

                if ( _cells[cell.rowID, i].typeID == 1 )
                    _cells[cell.rowID, i].RemoveFace();
            }
        }

	}
}