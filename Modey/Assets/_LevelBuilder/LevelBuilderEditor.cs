
#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace MANA3DGames
{
	public class LevelBuilderEditor : MonoBehaviour 
	{
		public class ColorEditMenu
		{
			GameObject gameObject;
			public Transform transform;
			RectTransform rect;

			DummyCell[] _currentBlocks;



			public ColorEditMenu( GameObject go )
			{
				gameObject = go;
				transform = gameObject.transform;
				rect = gameObject.GetComponent<RectTransform>();

				gameObject.GetComponent<Button>().onClick.AddListener( Close );

				for ( int i = 0; i <= 7; i++ ) 
				{
					int temp = i;
					AddListener( temp );
				}
			}

			void AddListener( int id )
			{
				transform.Find( id.ToString() ).GetComponent<Button>().onClick.AddListener( ()=> {  
					foreach ( var block in _currentBlocks ) 
						block.SetVal( id );
					Close();
				} );
			}

			public void Show( DummyCell[] blocks )
			{
				_currentBlocks = blocks;
				switch ( blocks.Length ) 
				{
				case 1:
					rect.anchoredPosition = blocks[0].position;
					break;
				case 10:
					rect.anchoredPosition = blocks[4].position;
					break;
				case 16:
					rect.anchoredPosition = blocks[8].position;
					break;
				}

				gameObject.SetActive( true );
			}

			public void Close()
			{
				gameObject.SetActive( false );
			}

			public Color GetColor( int id )
			{
				return transform.Find( id.ToString() ).GetComponent<Image>().color;
			}
		}

		public class DummyCell
		{
			public int rowID;
			public int roomID;

			public int colorID;
			public int typeID;

			public Vector2 position;

			Image _image;
			ColorEditMenu _editMenu;

			GameObject _face;

            public bool IsEmpty { get { return colorID == -1; } }


			public DummyCell( int rowId, int roomId, GameObject prefab, Vector2 pos, Transform border, ColorEditMenu editMenu )
			{
				colorID = -1;
				typeID = 0;

				rowID = rowId;
				roomID = roomId;
				position = pos;

				_editMenu = editMenu;

				var temp = Instantiate<GameObject>( prefab, border );
				temp.GetComponent<RectTransform>().anchoredPosition = position;
				temp.GetComponentInChildren<Text>().text = rowID + ", " + roomID;
				temp.GetComponent<Button>().onClick.AddListener( ()=> { _editMenu.Show( new DummyCell[]{ this } ); } );
				_image = temp.GetComponent<Image>();
				_face = temp.transform.Find( "Face" ).gameObject;
			}

			public void SetVal( int val )
			{
				if ( val <= 5 )
				{
					colorID = val;
					_image.color = _editMenu.GetColor( colorID );

					if ( val == 5 )
					{
						RemoveFace();
					}
				}
				else if ( val == 6  )
				{
					EmptyCell();
				}
				else if ( val == 7  )
				{
					if ( typeID == 1 )
					{
						RemoveFace();
					}
					else
					{
						if ( colorID == -1 || colorID == 5 )
						{
							colorID = 0;
							_image.color = _editMenu.GetColor( 0 );
						}

						SetFace();
					}
				}
			}

			public void EmptyCell()
			{
				colorID = -1;
				typeID = 0;
				_image.color = Color.white;
				_face.SetActive( false );
			}


			public void SetFace()
			{
				typeID = 1;
				_face.SetActive( true );
			}

			public void RemoveFace()
			{
				typeID = 0;
				_face.SetActive( false );
			}
		}
			


		public Transform border;
		public GameObject btnPrefab;
		public GameObject labelPrefab;
		public GameObject editMenuGO;

		ColorEditMenu _editMenu;

		DummyCell[,] _cells;

		Dropdown _landID;
		Dropdown _levelID;

		InputField _stepTime;
		InputField _spawnDelay;
		InputField _convertToBombAfter;

		Dropdown[] _colorBlocksCount;
		Dropdown[] _boostersCount;

        public string _lastSavePath;
        public string _lastOpenPath;


		void Start()
		{
			_editMenu = new ColorEditMenu( editMenuGO );

			CreateGridBorder();

			transform.Find( "Save" ).GetComponent<Button>().onClick.AddListener( Save );
			transform.Find( "SaveAs" ).GetComponent<Button>().onClick.AddListener( SaveAs );
			transform.Find( "Open" ).GetComponent<Button>().onClick.AddListener( Open );
			transform.Find( "Clear" ).GetComponent<Button>().onClick.AddListener( Clear );

            transform.Find( "Up" ).GetComponent<Button>().onClick.AddListener( MoveUp );
            transform.Find( "Down" ).GetComponent<Button>().onClick.AddListener( MoveDown );
            transform.Find( "Right" ).GetComponent<Button>().onClick.AddListener( MoveRight );
            transform.Find( "Left" ).GetComponent<Button>().onClick.AddListener( MoveLeft );


			_landID = transform.Find( "LandID" ).GetComponent<Dropdown>();
            AddOptions( _landID, 0, 2 );

			_levelID = transform.Find( "LevelID" ).GetComponent<Dropdown>();
            AddOptions( _levelID, 0, 29 );

			_stepTime = transform.Find( "StepTime" ).GetComponent<InputField>();
			_spawnDelay = transform.Find( "SpawnDelay" ).GetComponent<InputField>();
			_convertToBombAfter = transform.Find( "ConvertToBombAfter" ).GetComponent<InputField>();

			_colorBlocksCount = new Dropdown[5];
			for ( int i = 0; i < 5; i++ ) 
			{
				int temp = i;
				_colorBlocksCount[temp] = transform.Find( "ColorBlockCounts_" + temp ).GetComponent<Dropdown>();
                AddOptions( _colorBlocksCount[temp], 0, 20 );
			}

			_boostersCount = new Dropdown[3];
			for ( int i = 0; i < 3; i++ ) 
			{
				int temp = i;
				_boostersCount[temp] = transform.Find( "BoostersCount_" + temp ).GetComponent<Dropdown>();
                AddOptions( _boostersCount[temp], 0, 20 );
			}
		}

        void AddOptions( Dropdown dropDown, int from, int to )
        {
            List<string> options = new List<string>( to - from + 1 );
			for ( int i = from; i <= to; i++ ) 
				options.Add( i.ToString() );
			dropDown.ClearOptions();
			dropDown.AddOptions( options );
        }


		void CreateGridBorder()
		{
			_cells = new DummyCell[16,10];

			Vector2 originalPos = btnPrefab.GetComponent<RectTransform>().anchoredPosition;
			float size = 64;
			Vector2 pos = originalPos;

			pos.x -= size;
			for ( int i = 0; i < 16; i++ ) 
			{
				CreateRowColBtn( i, pos, true );
				pos.y -= size;
			}
			pos = originalPos;
			pos.y += size;
			for ( int j = 0; j < 10; j++ )
			{
				CreateRowColBtn( j, pos, false );
				pos.x += size;
			}

			pos = originalPos;
			for ( int i = 0; i < 16; i++ ) 
			{
				for ( int j = 0; j < 10; j++ )
				{
					_cells[i, j] = CreateDummyCell( i, j, pos );
					pos.x += size;
				}

				pos.x = originalPos.x;
				pos.y -= size;
			}

			Destroy( labelPrefab );
			Destroy( btnPrefab );
		}

		void CreateRowColBtn( int id, Vector2 pos, bool selectRow )
		{
			var temp = Instantiate<GameObject>( labelPrefab, border );
			temp.GetComponent<RectTransform>().anchoredPosition = pos;
			temp.GetComponentInChildren<Text>().text = id.ToString();
			UnityAction act1 = ()=> { SelectRow( id ); };
			UnityAction act2 = ()=> { SelectCol( id ); };
			temp.GetComponent<Button>().onClick.AddListener( selectRow ?  act1 : act2 );
		}

		void SelectRow( int index )
		{
			var selectedCells = new DummyCell[10];
			for ( int i = 0; i < selectedCells.Length; i++ ) 
				selectedCells[i] = _cells[index, i];
			_editMenu.Show( selectedCells );
		}
		void SelectCol( int index )
		{
			var selectedCells = new DummyCell[16];
			for ( int i = 0; i < selectedCells.Length; i++ ) 
				selectedCells[i] = _cells[i, index];
			_editMenu.Show( selectedCells );
		}

		DummyCell CreateDummyCell( int rowId, int roomId, Vector2 pos )
		{
			return new DummyCell( rowId, roomId, btnPrefab, pos, border, _editMenu );
		}



		void Save()
		{
			_editMenu.Close();

			var databasePath = Application.dataPath + "/Resources/LevelDatabase/" + _landID.value + "/" + _levelID.value;
			if ( !System.IO.Directory.Exists( databasePath ) )
				System.IO.Directory.CreateDirectory( databasePath );
			CreateJSONFile( databasePath );
		}

		void SaveAs()
		{
			_editMenu.Close();
            string path = UnityEditor.EditorUtility.SaveFilePanel( "Save LevelJOSN file", _lastSavePath, _levelID.value.ToString(), "json" );
			CreateJSONFile( path );
            _lastSavePath = path;
		}

		void CreateJSONFile( string databasePath )
		{
			if ( string.IsNullOrEmpty( databasePath ) )
				return;
			
			LevelJSON level = new LevelJSON();
			level.levelID = _levelID.value;
			level.landID = _landID.value;
			float.TryParse( _stepTime.text.Trim(), out level.stepTime );
			float.TryParse( _spawnDelay.text.Trim(), out level.nextSpawnDelay );
			int.TryParse( _convertToBombAfter.text.Trim(), out level.spawnBombAfter );
			level.colorBlocksCount = new int[_colorBlocksCount.Length];
			for ( int i = 0; i < _colorBlocksCount.Length; i++ ) 
				level.colorBlocksCount[i] = _colorBlocksCount[i].value;
			level.boostersCount = new int[_boostersCount.Length];
			for ( int i = 0; i < _boostersCount.Length; i++ ) 
				level.boostersCount[i] = _boostersCount[i].value;
			
			level.reservedCells = new List<ReservedCell>();
			foreach ( var cell in _cells )
			{
				if ( cell.colorID != -1 )
					level.reservedCells.Add( new ReservedCell( cell.typeID, cell.colorID, cell.rowID, cell.roomID ) );
			}

			var json = JsonUtility.ToJson( level );//LitJson.JsonMapper.ToJson( level );

			System.IO.File.WriteAllText( databasePath, json );
			UnityEditor.AssetDatabase.Refresh();
		}

		void Open()
		{
			_editMenu.Close();

			string path = UnityEditor.EditorUtility.OpenFilePanel( "Open LevelJSON file", _lastOpenPath, "json" );
            
			if ( string.IsNullOrEmpty( path ) )
				return;

			Clear();

			string json = System.IO.File.ReadAllText( path );

			LevelJSON level = JsonUtility.FromJson<LevelJSON>( json );//LitJson.JsonMapper.ToObject<LevelJSON>( json );
			FillLevelData( level );

            _lastOpenPath = path;
		}

		void FillLevelData( LevelJSON level )
		{
			_landID.value = level.landID;
			_levelID.value = level.levelID;

			_stepTime.text = level.stepTime.ToString();
			_spawnDelay.text = level.nextSpawnDelay.ToString();
			_convertToBombAfter.text = level.spawnBombAfter.ToString();

			for ( int i = 0; i < _colorBlocksCount.Length; i++ ) 
				_colorBlocksCount[i].value = level.colorBlocksCount[i];
			
			for ( int i = 0; i < _boostersCount.Length; i++ ) 
				_boostersCount[i].value = level.boostersCount[i];

			foreach ( var rCell in level.reservedCells ) 
			{
				_cells[rCell.rowIndex, rCell.roomIndex].rowID = rCell.rowIndex;
				_cells[rCell.rowIndex, rCell.roomIndex].roomID = rCell.roomIndex;
				_cells[rCell.rowIndex, rCell.roomIndex].SetVal( rCell.colorId );
				if ( rCell.type == 1 )
					_cells[rCell.rowIndex, rCell.roomIndex].SetFace();
			}
		}

		void Clear()
		{
			_editMenu.Close();

			foreach ( var cell in _cells )
				cell.EmptyCell();

			_landID.value = 0;
			_levelID.value = 0;

			_stepTime.text = "";
			_spawnDelay.text = "";
			_convertToBombAfter.text = "";

			foreach ( var item in _colorBlocksCount )
				item.value = 0;

			foreach ( var item in _boostersCount )
				item.value = 0;
		}


        void MoveUp()
        {
            foreach ( var cell in _cells )
            {
                if ( !cell.IsEmpty && cell.rowID > 4 && _cells[cell.rowID - 1, cell.roomID].IsEmpty )
                {
                    _cells[cell.rowID - 1, cell.roomID].SetVal( cell.colorID );
                    if ( cell.typeID == 1 )
                        _cells[cell.rowID - 1, cell.roomID].SetFace();
                    cell.EmptyCell();
                }
            }
        }
        void MoveDown()
        {
            for ( int i = _cells.GetLength( 0 ) - 1; i >= 0; i-- )
            {
                for ( int j = _cells.GetLength( 1 ) - 1; j >= 0; j-- )
                {
                    var cell = _cells[i,j];
                    if ( !cell.IsEmpty && cell.rowID < 15 && _cells[cell.rowID + 1, cell.roomID].IsEmpty )
                    {
                        _cells[cell.rowID + 1, cell.roomID].SetVal( cell.colorID );
                        if ( cell.typeID == 1 )
                            _cells[cell.rowID + 1, cell.roomID].SetFace();
                        cell.EmptyCell();
                    }
                }
            }
        }
        void MoveRight()
        {
            for ( int i = 0; i < _cells.GetLength( 0 ); i++ )
            {
                for ( int j = _cells.GetLength( 1 ) - 1; j >= 0; j-- )
                {
                    var cell = _cells[i,j];
                    if ( !cell.IsEmpty && cell.roomID < 9 && _cells[cell.rowID, cell.roomID + 1].IsEmpty )
                    {
                        _cells[cell.rowID, cell.roomID + 1].SetVal( cell.colorID );
                        if ( cell.typeID == 1 )
                            _cells[cell.rowID, cell.roomID + 1].SetFace();
                        cell.EmptyCell();
                    }
                }
            }
        }
        void MoveLeft()
        {
            foreach ( var cell in _cells )
            {
                if ( !cell.IsEmpty && cell.roomID > 0 && _cells[cell.rowID, cell.roomID - 1].IsEmpty )
                {
                    _cells[cell.rowID, cell.roomID - 1].SetVal( cell.colorID );
                    if ( cell.typeID == 1 )
                        _cells[cell.rowID, cell.roomID - 1].SetFace();
                    cell.EmptyCell();
                }
            }
        }
	}
}

#endif