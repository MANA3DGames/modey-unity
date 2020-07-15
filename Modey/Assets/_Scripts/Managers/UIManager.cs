using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MANA3DGames.Utilities.Coroutine;
using TMPro;

namespace MANA3DGames
{
    public class UIManager
    {
        #region [Private Classes]

        class ClickedSprite
        {
            public string name;
            public SpriteRenderer render;
            public Color originalColor;

            public ClickedSprite( string name, SpriteRenderer render, Color color )
            {
                this.name = name;
                this.render = render;
                this.originalColor = color;
            }
        }

        #endregion

        #region [Variables]

        GameManager _gameManager;

        GOGroup _rootMenu;
        GOGroup _loadingScreen;
        GOGroup _mainMenu;
        GOGroup _settingsMenu;
        GOGroup _gameSpraksMenu;
        GOGroup _gameplayMenu;
        GOGroup _infoMenu;
        GOGroup _levelsMenu;
        GOGroup _pauseMenu;
        GOGroup _startupTimer;
        GOGroup _messageDialog;
        GOGroup _gameOverDialog;
        GOGroup _winChallengeLevelDialog;
        GOGroup _customBlockDialog;
        GOGroup _storeMenu;
        GOGroup _store_BoostersSubMenu;
        GOGroup _store_PaintersSubMenu;
        GOGroup _store_CoinsSubMenu;
        GOGroup _statisticsMenu;
        GOGroup _facebookMenu;
        GOGroup _tutorialMenu;
        GOGroup _notificationMenu;
        GOGroup _newLandNotificationMenu;

        Dictionary<string, Action> _onClickUpActions;
        Dictionary<string, Sprite> _allSprites;
        Sprite[] _landSprites;

        Color _onClickStartColor = new Color( 1.0f, 0.6f, 0.6f, 1.0f );

        ClickedSprite _currentSprite;

        List<GameObject> _rewardTweenGOs;

        CoroutineTask _displayMenuTask;
        public bool IsDisplayingMenu { get { return _displayMenuTask != null && _displayMenuTask.IsRunning; } }

        public Sprite GameSparksSprite
        {
            get
            {
                if ( _mainMenu != null )
                    return _mainMenu.Get( "GameSparksBtn" ).transform.Find( "Icon" ).GetComponent<SpriteRenderer>().sprite;
                return null;
            }
        }

        public Sprite GameSparksAvatar
        {
            get
            {
                return _mainMenu.Get( "UserProfileBtn" ).transform.Find( "Avatar" ).GetComponent<SpriteRenderer>().sprite;
            }
        }

        string[] LAND_NAMES = { "Koko Land", "Yamoon Land", "Mana Land" };

        #endregion

        // ******************************************************* //

        #region [Constructors]
        
        public UIManager( GameManager manager )
        {
            _gameManager = manager;

            _rootMenu                   = new GOGroup( _gameManager.gameObject );
            _loadingScreen              = new GOGroup( _rootMenu.Get( "LoadingScreen" ) );
            _mainMenu                   = new GOGroup( _rootMenu.Get( "MainMenu" ) );
            _settingsMenu               = new GOGroup( _rootMenu.Get( "SettingsMenu" ) );
            _gameSpraksMenu             = new GOGroup( _rootMenu.Get( "GameSparksMenu" ) );
            _gameplayMenu               = new GOGroup( _rootMenu.Get( "Gameplay" ) );
            _infoMenu                   = new GOGroup( _rootMenu.Get( "InfoMenu" ) );
            _levelsMenu                 = new GOGroup( _rootMenu.Get( "LevelsMenu" ) );
            _pauseMenu                  = new GOGroup( _rootMenu.Get( "PauseMenu" ) );
            _startupTimer               = new GOGroup( _rootMenu.Get( "StartupTimer" ) );
            _messageDialog              = new GOGroup( _rootMenu.Get( "MessageDialog" ) );
            _gameOverDialog             = new GOGroup( _rootMenu.Get( "GameOverDialog" ) );
            _winChallengeLevelDialog    = new GOGroup( _rootMenu.Get( "WinChallengeLevelDialog" ) );
            _customBlockDialog          = new GOGroup( _rootMenu.Get( "CustomBlockDialog" ) );
            _storeMenu                  = new GOGroup( _rootMenu.Get( "StoreMenu" ) );
            _statisticsMenu             = new GOGroup( _rootMenu.Get( "StatisticsMenu" ) );
            _facebookMenu               = new GOGroup( _rootMenu.Get( "FacebookMenu" ) );
            _tutorialMenu               = new GOGroup( _rootMenu.Get( "Tutorials" ) );
            _notificationMenu           = new GOGroup( _rootMenu.Get( "NewInvitationNotification" ) );
            _newLandNotificationMenu    = new GOGroup( _rootMenu.Get( "NewLandNotification" ) );

            _onClickUpActions = new Dictionary<string, Action>();

            SetupMainMenu();
            SetupSettingsMenu();
            SetupInfoMenu();
            SetupGameplayMenu();
            SetupLevelsMenu();
            SetupPauseMenu();
            SetupGameOverDialog();
            SetupWinChallengeLevelDialog();
            SetupCustomBlockDialog();
            SetupStoreMenu();
            SetupStatisticsMenu();
            SetupFacebookMenu();
            SetupGameSparksMenu();
            SetupTutorialsMenu();

            _rootMenu.ShowAllItems( false );

            _rewardTweenGOs = new List<GameObject>(2);
        }

        #endregion

        // ******************************************************* //

        public void SetTextFromOutSide( Transform tMeshPro, string text )
        {
            tMeshPro.GetComponent<TextMeshPro>().text = "";
            TextMesh txtMesh = tMeshPro.Find( "ArabicTxt" ).GetComponent<TextMesh>();
            txtMesh.gameObject.SetActive( true ); 
            txtMesh.text = ArabicSupport.ArabicFixer.Fix( text );
            if ( string.IsNullOrEmpty( txtMesh.text ) )
                txtMesh.text = text;
        }

        #region [MANA 3D Games Logo]

        public void ShowMANA3DGamesLogo( bool show )
        {
            _rootMenu.ShowItem( "MANA3DGamesLogo", show );
        }

        public void ShowGameSparksLogo( bool show )
        {
            _rootMenu.ShowItem( "GameSparksLogo", show );
        }

        public void FadeOutGameSparksLogo( Action OnCompleteAction, float time = 1.0f )
        {
            var tween = LeanTween.alpha( _rootMenu.Get( "GameSparksLogo" ), 0.0f, time );
            tween.setOnComplete( OnCompleteAction );
            tween.setDelay( 2.0f );
        }

        public void FadeOutMANA3DGamesLogo( Action OnCompleteAction, float time = 1.0f )
        {
            var tween = LeanTween.alpha( _rootMenu.Get( "MANA3DGamesLogo" ), 0.0f, time );
            tween.setOnComplete( OnCompleteAction );
            tween.setDelay( 2.0f );
        }

        #endregion

        #region [Loading Screen]

        CoroutineTask _loadingScreenTask;

        public void DisplayLoadingScreen( string message )
        {
            CancelLoadingScreen();
            _loadingScreen.SetText( "MSG", message );
            _loadingScreen.ShowAllItems( true );
            _loadingScreen.ShowRoot( true );
            _loadingScreenTask = new CoroutineTask( LoadingIndicatorCoroutine() );
            LeanTween.alpha( _loadingScreen.GetRoot(), 1.0f, 0.1f );
        }

        IEnumerator LoadingIndicatorCoroutine()
        {
            int index = 0;
            while ( true )
            {
                _loadingScreen.ShowAllItems( true );
                _loadingScreen.ShowItem( index.ToString(), false );
                index++;
                if ( index > 4 )
                    index = 0;
                yield return new WaitForSeconds( 0.2f );
            }
        }

        public void FadeOutLoadingScreen( Action OnCompleteAction, float time = 1.0f )
        {
            CancelLoadingScreen();

            Action action = ()=> {
                if ( OnCompleteAction != null )
                    OnCompleteAction.Invoke();
                _loadingScreen.ShowRoot( false );
            };

            var tween = LeanTween.alpha( _loadingScreen.GetRoot(), 0.0f, time );
            tween.setOnComplete( action );
        }

        void CancelLoadingScreen()
        {
            if ( _loadingScreenTask != null )
            {
                _loadingScreenTask.kill();
                _loadingScreenTask = null;
            }

            LeanTween.cancel( _loadingScreen.GetRoot() );
        }

        public bool IsLoadingScreen { get { return _loadingScreen.IsActive; } }

        #endregion


        #region [Main Menu]

        void SetupMainMenu()
        {
            AddOnClickListener( "PlayBtn", OnClick_MainMenu_PlayBtn );
            AddOnClickListener( "SettingsBtn", OnClick_MainMenu_SettingsBtn );
            AddOnClickListener( "InfoBtn", OnClick_MainMenu_InfoBtn );
            AddOnClickListener( "UserProfileBtn", OnClick_MainMenu_UserProfileBtn );
            AddOnClickListener( "FacebookMenuBtn", OnClick_MainMenu_FacebookMenuBtnBtn );
            AddOnClickListener( "ShopBtn", OnClick_MainMenu_ShopBtn );
            AddOnClickListener( "GameSparksBtn", OnClick_MainMenu_GameSparksBtn );
        }
        void OnClick_MainMenu_PlayBtn()
        {
            _mainMenu.ShowRoot( false );
            _gameManager.GoToLevelsMenu();
        }
        void OnClick_MainMenu_SettingsBtn()
        {
            _mainMenu.ShowRoot( false );
            _gameManager.GoToSettingsMenu();
        }
        void OnClick_MainMenu_InfoBtn()
        {
            _mainMenu.ShowRoot( false );
            _gameManager.GoToInfoMenu();
        }
        void OnClick_MainMenu_UserProfileBtn()
        {
            _mainMenu.ShowRoot( false );
            _gameManager.GoToStatisticsMenu();
        }
        void OnClick_MainMenu_FacebookMenuBtnBtn()
        {
            _mainMenu.ShowRoot( false );
            _gameManager.GoToFacebookMenu();
        }
        void OnClick_MainMenu_ShopBtn()
        {
            _mainMenu.ShowRoot( false );
            _gameManager.GoToStoreMenu();
        }
        void OnClick_MainMenu_GameSparksBtn()
        {
            _mainMenu.ShowRoot( false );
            _gameManager.GoToGameSparksMenu();
        }

        public void DisplayMainMenu( float time = 1.0f )
        {
            _displayMenuTask = new CoroutineTask( DisplayMainMenuCoroutine( time ), true );
        }
        IEnumerator DisplayMainMenuCoroutine( float time = 1.0f )
        {
            _gameManager.LockButtonsInput( true );

            _rootMenu.SetSprite( "BG", _landSprites[0] );

            _mainMenu.SetAllScale(Vector3.zero);
            _mainMenu.ShowRoot(true);

            if ( !_rootMenu.GetActive( "BG" ) )
            {
                _rootMenu.SetSpriteAlpha( "BG", 0.0f );
                _rootMenu.ShowItem( "BG", true );
                LTDescr tweenBG = LeanTween.alpha( _rootMenu.Get("BG"), 1.0f, time );
                tweenBG.setFrom( 0.0f );

                yield return new WaitForSeconds( time );
            }

            _mainMenu.ShowItem( "Image", true );
            LTDescr tweenImg = LeanTween.scale( _mainMenu.Get("Image"), Vector3.one, time * 0.4f );
            tweenImg.setOnStart( ()=> { _gameManager.AudioManagerInstance.Play( AudioLibrary.UI.Scaled[2] ); } );
            tweenImg.setFrom( Vector3.zero );
            tweenImg.setEaseOutBounce();
            

            int[] scaleSFx = { 1, 1, 1, 1, 2 };

            Transform imageTransf = _mainMenu.Get( "Image" ).transform;
            float delayChild = time;
            for ( int i = 0; i < 5; i++ )
            {
                int temp = i;
                imageTransf.Find( temp.ToString() ).localScale = Vector3.zero;
                var tweenChild = LeanTween.scale( imageTransf.Find( temp.ToString() ).gameObject, Vector3.one, 0.5f );
                tweenChild.setFrom( Vector3.zero );
                tweenChild.setDelay( delayChild );
                tweenChild.setEaseInOutElastic();

                LeanTween.delayedCall( delayChild + ( time * 0.1f ), ()=> { _gameManager.AudioManagerInstance.Play( AudioLibrary.UI.Scaled[scaleSFx[temp]] ); } );

                delayChild += 0.3f;
            }


            LTDescr tweenTitle = LeanTween.moveLocalY(_mainMenu.Get("Title"), _mainMenu.GetPosY("Title"), time);
            tweenTitle.setEaseOutBounce();

            _mainMenu.SetPosY("Title", 7.0f);
            _mainMenu.SetScale("Title", Vector3.one);
            _mainMenu.ShowItem("Title", true);

            yield return new WaitForSeconds( time );

            _mainMenu.ShowItem("PlayBtn", true);
            LTDescr tweenPlayBtn = LeanTween.scale(_mainMenu.Get("PlayBtn"), Vector3.one, time);
            tweenPlayBtn.setDelay(0.0f);
            tweenPlayBtn.setFrom(Vector3.zero);
            tweenPlayBtn.setEaseOutElastic();
            
            Action scaleAction = null;

            float delay = 0.2f;
            float increment = 0.2f;
            ScaleEaseOutBack( _mainMenu, "SettingsBtn", time, time * delay, scaleAction );
            ScaleEaseOutBack( _mainMenu, "ShopBtn", time, time * (delay+= increment), scaleAction );
            ScaleEaseOutBack( _mainMenu, "FacebookMenuBtn", time, time * (delay+= increment), scaleAction );
            ScaleEaseOutBack( _mainMenu, "GameSparksBtn", time, time * (delay+= increment), scaleAction );
            ScaleEaseOutBack( _mainMenu, "InfoBtn", time, time * ( delay+= increment ), scaleAction, () => {
                _gameManager.LockButtonsInput( false );
            } );

            if ( _mainMenu.GetLocalScale( "UserProfileBtn" ) == Vector3.zero )
                DisplayMainMenuUserProfileBtn();
        }

        internal void DisplayMessageDialog( string v1, string v2, Func<object> p1, Action p2 )
        {
            throw new NotImplementedException();
        }

        public void SetUserProfileBtn_Name( string userName )
        {
            if ( !string.IsNullOrEmpty( userName ) )
            {
                //_mainMenu.Get( "UserProfileBtn" ).transform.FindChild( "Name" ).GetComponent<TMPro.TextMeshPro>().text = userName;
                SetTextFromOutSide( _mainMenu.Get( "UserProfileBtn" ).transform.Find( "Name" ), userName );
                var meshText = _mainMenu.Get( "UserProfileBtn" ).transform.Find( "Name" ).Find( "ArabicTxt" ).GetComponent<TextMesh>();
                if ( meshText.text.Length > 20 )
                    meshText.text = meshText.text.Substring( 0, 20 );
                meshText.text = "Welcome\n" + meshText.text;
            }  
        }
        public void SetUserProfileBtn_Avatar( Sprite avatar )
        {
            if ( avatar != null  )
                _mainMenu.Get( "UserProfileBtn" ).transform.Find( "Avatar" ).GetComponent<SpriteRenderer>().sprite = avatar;
        }
        public void DisplayMainMenuUserProfileBtn( float time = 0.5f )
        {
            MoveY( _mainMenu, "UserProfileBtn", 7.0f, _mainMenu.GetPosY( "UserProfileBtn" ), time, 0.0f );
            _mainMenu.SetScale( "UserProfileBtn", Vector3.one );
        }
        public void HideMainMenuUserProfileBtn()
        {
            _mainMenu.ShowItem( "UserProfileBtn", false );
        }

        #endregion

        #region [Statistics Menu]

        void SetupStatisticsMenu()
        {
            AddOnClickListener( "StatisticsMenu_BackBtn", OnClick_StatisticsMenu_BackBtn );
        }
        public void OnClick_StatisticsMenu_BackBtn()
        {
            _statisticsMenu.ShowRoot( false );
            _gameManager.GoToMainMenu();
        }

        public void DisplayStatisticsMenu( StatisticsData data, float time = 0.3f )
        {
            _gameManager.LockButtonsInput( true );

            _statisticsMenu.SetSprite( "Avatar", GameSparksAvatar );
            //_statisticsMenu.SetText( "Name", data.name );
            SetTextFromOutSide( _statisticsMenu.Get( "Name" ).transform, data.name );
            var meshText = _statisticsMenu.Get( "Name" ).transform.Find( "ArabicTxt" ).GetComponent<TextMesh>();
            if ( meshText.text.Length > 20 )
                meshText.text = meshText.text.Substring( 0, 20 );
            //meshText.text = "Welcome\n" + meshText.text;

            _statisticsMenu.SetText( "ScoreVal", data.score.ToString() );
            _statisticsMenu.SetText( "RankVal", data.rank == -1 ? "!" : data.rank.ToString() );
            _statisticsMenu.SetText( "LandVal", ( data.land + 1 ).ToString() );
            _statisticsMenu.SetText( "LevelVal", ( data.level + 1 ).ToString() );
            _statisticsMenu.SetText( "CoinsVal", data.coins.ToString() );
            for ( int i = 0; i < 3; i++ )
                _statisticsMenu.SetText( ( i + 1 ) + "StarsVal", data.starsCount[i].ToString() );
            for ( int i = 0; i < 3; i++ )
                _statisticsMenu.SetText( "Booster" + i + "Val", data.boosters[i].ToString() );
            for ( int i = 0; i < 5; i++ )
                _statisticsMenu.SetText( "Painter" + i + "Val", data.painters[i].ToString() );

            _statisticsMenu.ShowAllItems( true );
            _statisticsMenu.ShowRoot( true );

            MoveY( _statisticsMenu, "StatisticsMenu_BackBtn", -7.5f, _statisticsMenu.GetPosY( "StatisticsMenu_BackBtn" ), time, 0.0f, () => { _gameManager.LockButtonsInput(false); });
            LeanTween.delayedCall( time, () => { _gameManager.AudioManagerInstance.Play( AudioLibrary.UI.TweenUp, 0.3f ); } );
        }

        #endregion

        #region [Settigns Menu]

        void SetupSettingsMenu()
        {
            AddOnClickListener("Settings_SFXBtn", OnClick_SettingsMenu_SFXBtn );
            AddOnClickListener("Settings_BGMBtn", OnClick_SettingsMenu_BGMBtn );
            AddOnClickListener("Settings_StartupTimerBtn", OnClick_SettingsMenu_StartupTimerBtn);
            AddOnClickListener("Settings_TutorialsBtn", OnClick_Settings_TutorialsBtn );
            AddOnClickListener("Settings_DefaultSettingsBtn", OnClick_Settings_DefaultSettingsBtn );
            AddOnClickListener("Settings_BackBtn", OnClick_SettingsMenu_BackBtn );
        }
        void OnClick_SettingsMenu_SFXBtn()
        {
            _gameManager.EnableSFX( !_gameManager.gameSettings.sfx );
            _settingsMenu.Get( "Settings_SFXBtn" ).transform.Find( "Disable" ).gameObject.SetActive( !_gameManager.gameSettings.sfx );
        }
        void OnClick_SettingsMenu_BGMBtn()
        {
            _gameManager.EnableBGM( !_gameManager.gameSettings.bgm );
            _settingsMenu.Get( "Settings_BGMBtn" ).transform.Find( "Disable" ).gameObject.SetActive( !_gameManager.gameSettings.bgm );
        }
        void OnClick_SettingsMenu_StartupTimerBtn()
        {
            _gameManager.EnableStartupTimer( !_gameManager.gameSettings.startupTimer );
            _settingsMenu.Get( "Settings_StartupTimerBtn" ).transform.Find( "Disable" ).gameObject.SetActive( !_gameManager.gameSettings.startupTimer );
        }
        void OnClick_Settings_TutorialsBtn()
        {
            _settingsMenu.ShowRoot( false );
            _gameManager.GoToTutorialsMenu( 0 );
        }
        void OnClick_Settings_DefaultSettingsBtn()
        {
            _gameManager.ApplyDefaultSettings();
            _settingsMenu.Get( "Settings_SFXBtn" ).transform.Find( "Disable" ).gameObject.SetActive( !_gameManager.gameSettings.sfx );
            _settingsMenu.Get( "Settings_BGMBtn" ).transform.Find( "Disable" ).gameObject.SetActive( !_gameManager.gameSettings.bgm );
            _settingsMenu.Get( "Settings_StartupTimerBtn" ).transform.Find( "Disable" ).gameObject.SetActive( !_gameManager.gameSettings.startupTimer );
        }
        public void OnClick_SettingsMenu_BackBtn()
        {
            _settingsMenu.ShowRoot( false );
            _gameManager.GoBackFromSettingsMenu();
        }

        public void DisplaySettingsMenu( float time = 0.5f )
        {
            _settingsMenu.Get( "Settings_SFXBtn" ).transform.Find( "Disable" ).gameObject.SetActive( !_gameManager.gameSettings.sfx );
            _settingsMenu.Get( "Settings_BGMBtn" ).transform.Find( "Disable" ).gameObject.SetActive( !_gameManager.gameSettings.bgm );
            _settingsMenu.Get( "Settings_StartupTimerBtn" ).transform.Find( "Disable" ).gameObject.SetActive( !_gameManager.gameSettings.startupTimer );

            _displayMenuTask = new CoroutineTask( DisplayMenu( _settingsMenu, "Settings", time ) );
        }

        #endregion

        #region [Info Menu]

        void SetupInfoMenu()
        {
            AddOnClickListener( "Info_RateBtn", OnClick_InfoMenu_RateBtn );
            AddOnClickListener( "Info_LinkBtn", OnClick_InfoMenu_LinkBtn );
            AddOnClickListener( "Info_BackBtn", OnClick_InfoMenu_BackBtn );
        }
        void OnClick_InfoMenu_RateBtn()
        {
            _gameManager.OpenGooglePlayStore();
        }
        void OnClick_InfoMenu_LinkBtn()
        {
            _gameManager.OpenMANA3DGames();
        }
        public void OnClick_InfoMenu_BackBtn()
        {
            _infoMenu.ShowRoot( false );
            _gameManager.GoToMainMenu();
        }

        public void DisplayInfoMenu( float time = 0.5f )
        {
            _displayMenuTask = new CoroutineTask( DisplayInfoMenuCoroutine(), true );
        }
        IEnumerator DisplayInfoMenuCoroutine( float time = 0.5f )
        {
            _gameManager.LockButtonsInput( true );

            _infoMenu.ShowAllItems(false);
            _infoMenu.ShowRoot(true);
            _infoMenu.ShowItem("Border", true);

            yield return new WaitForSeconds(0.1f);

            float from = -10.5f;
            float delay = 0.2f;
            float delayInc = 0.2f;

            _gameManager.AudioManagerInstance.Play( AudioLibrary.BlockSFX.SwipeLeftRight[0], 0.3f );

            MoveY(_infoMenu, "MANA3DGames", from, _infoMenu.GetPosY("MANA3DGames"), time, time * (delay += delayInc));
            LeanTween.delayedCall( time * delay + delayInc, () => { _gameManager.AudioManagerInstance.Play( AudioLibrary.BlockSFX.SwipeLeftRight[0], 0.3f ); } );

            MoveY(_infoMenu, "Info", from, _infoMenu.GetPosY("Info"), time, time * (delay += delayInc));
            LeanTween.delayedCall( time * delay + delayInc, () => { _gameManager.AudioManagerInstance.Play( AudioLibrary.BlockSFX.SwipeLeftRight[0], 0.3f ); } );

            MoveY(_infoMenu, "Info_RateBtn", from, _infoMenu.GetPosY("Info_RateBtn"), time, time * (delay += delayInc));
            LeanTween.delayedCall( time * delay + delayInc, () => { _gameManager.AudioManagerInstance.Play( AudioLibrary.BlockSFX.SwipeLeftRight[0], 0.3f ); } );

            MoveY(_infoMenu, "Info_LinkBtn", from, _infoMenu.GetPosY("Info_LinkBtn"), time, time * (delay += delayInc));
            LeanTween.delayedCall( time * delay + delayInc, () => { _gameManager.AudioManagerInstance.Play( AudioLibrary.BlockSFX.SwipeLeftRight[0], 0.3f ); } );

            MoveY(_infoMenu, "Info_BackBtn", from, _infoMenu.GetPosY("Info_BackBtn"), time, time * (delay += delayInc), () => { _gameManager.LockButtonsInput(false); });
            LeanTween.delayedCall( time * delay, () => { _gameManager.AudioManagerInstance.Play( AudioLibrary.UI.TweenUp, 0.3f ); } );
        }

        #endregion

        #region [Levels Menu]

        int _currentLandID = 0;

        void SetupLevelsMenu()
        {
            _landSprites = Resources.LoadAll<Sprite>( "Graphics/Lands" );

            // Create level buttons
            GameObject lvlBtnPrefab = Resources.Load<GameObject>( "UI/LevelBtn" );
            Transform parent = _levelsMenu.Get( "Levels" ).transform;
            float x = -2.7f;
            float y = 3.5f;
            float xOffset = 1.35f;
            float yOffset = -1.39f;
            int levelID = 0;
            
            for ( int i = 0; i < 6; i++ )
            {
                for ( int j = 0; j < 5; j++ )
                {
                    var btn = GameObject.Instantiate( lvlBtnPrefab );
                    btn.transform.position = new Vector3( x, y, 0 );
                    btn.transform.parent = parent;
                    btn.name = "Level_" + levelID;

                    x += xOffset;
                    levelID++;
                }

                x = -2.7f;
                y += yOffset;
            }

            // Add onClick listeners,
            for ( int i = 0; i < 30; i++ )
            {
                var temp = i;
                AddOnClickListener( "Level_" + temp, ()=> { OnClickLevelMenu_LevelIDBtn( temp ); } );
            }

            for ( int i = 0; i < LevelManager.LAND_COUNT; i++ )
            {
                int temp = i;
                AddOnClickListener( "PageBtn" + temp, ()=> { OnClickLevelsMenu_PageBtn( temp ); } );
            }

            AddOnClickListener( "Levels_NextBtn", OnClickLevelsMenu_NextBtn );
            AddOnClickListener( "Levels_PreviousBtn", OnClickLevelsMenu_PreviousBtn );

            AddOnClickListener( "Levels_HomeBtn", OnClickLevelsMenu_HomeBtn );
            AddOnClickListener( "Levels_LeaderboardBtn", OnClickLevelsMenu_LeaderboardBtn );
            AddOnClickListener( "Levels_ShopBtn", OnClickLevelsMenu_ShopBtn );
        }
        void OnClickLevelMenu_LevelIDBtn( int id )
        {
            _levelsMenu.ShowRoot( false );
            _gameplayMenu.ShowRoot( true );
            _gameManager.LoadLevel( _currentLandID, id );
        }
        void OnClickLevelsMenu_PageBtn( int id )
        {
            switch ( _currentLandID )
            {
                case 0:
                    if ( id == 1 )
                        NextLand();
                    else if ( id == 2 )
                        PreviousLand();
                    break;
                case 1:
                    if ( id == 2 )
                        NextLand();
                    else if ( id == 0 )
                        PreviousLand();
                    break;
                case 2:
                    if ( id == 0 )
                        NextLand();
                    else if ( id == 1 )
                        PreviousLand();
                    break;
            }
        }
        void OnClickLevelsMenu_NextBtn()
        {
            NextLand();
        }
        void OnClickLevelsMenu_PreviousBtn()
        {
            PreviousLand();
        }
        void OnClickLevelsMenu_LeaderboardBtn()
        {
            _levelsMenu.ShowRoot( false );
            _gameManager.GoToGameSparksLeaderBoard();
        }
        void OnClickLevelsMenu_ShopBtn()
        {
            _levelsMenu.ShowRoot( false );
            _gameManager.GoToStoreMenu();
        }
        public void OnClickLevelsMenu_HomeBtn()
        {
            _levelsMenu.ShowRoot( false );
            _gameManager.GoToMainMenu();
        }

        public void DisplayLevelsMenu( float time = 0.5f )
        {
            _displayMenuTask = new CoroutineTask( DisplayLevelsMenuCoroutine( time ), true );
        }
        IEnumerator DisplayLevelsMenuCoroutine( float time = 0.5f )
        {
            _gameManager.LockButtonsInput( true );

            yield return new WaitForSeconds( 0.1f );

            LoadLandUIData( _currentLandID );

            _levelsMenu.ShowAllItems( false );
            _levelsMenu.ShowRoot( true );

            SetCurrentLand();
            _levelsMenu.ShowItem( "LandImg1", true );

            float delay = 0.0f;
            float delayInc = 0.01f;

            float from = -7.0f;
            Transform levels = _levelsMenu.Get( "Levels" ).transform;
            for ( int i = 0; i < levels.childCount; i++ )
                MoveXEaseOutBack( levels.GetChild( i ).gameObject, from, levels.GetChild( i ).position.x, time, delay+=delayInc );
            _levelsMenu.ShowItem( "Levels", true );

            LeanTween.delayedCall( delay, ()=> { _gameManager.AudioManagerInstance.Play( AudioLibrary.UI.Back ); } );

            MoveY( _levelsMenu, "Title", 7.0f, _levelsMenu.GetPosY( "Title" ), time, delay );
            MoveY( _levelsMenu, "Middle", 7.0f, _levelsMenu.GetPosY( "Middle" ), time, delay );
            MoveY( _levelsMenu, "Levels_PreviousBtn", 7.0f, _levelsMenu.GetPosY( "Levels_PreviousBtn" ), time, delay );
            MoveY( _levelsMenu, "Levels_NextBtn", 7.0f, _levelsMenu.GetPosY( "Levels_NextBtn" ), time, delay );

            for ( int i = 0; i < LevelManager.LAND_COUNT; i++ )
                ScaleEaseOutBack( _levelsMenu, "PageBtn" + i, time, delay +=delayInc, null, null, true );
            
            Action action = ()=> { _gameManager.AudioManagerInstance.Play( AudioLibrary.UI.Scaled[0] ); };

            delayInc = 0.1f;
            ScaleEaseOutBack( _levelsMenu, "Levels_HomeBtn", time, delay += delayInc, action, null, true );
            ScaleEaseOutBack( _levelsMenu, "Levels_LeaderboardBtn", time, delay += delayInc, action, null, true );
            ScaleEaseOutBack( _levelsMenu, "Levels_ShopBtn", time, delay += delayInc, action, ()=> { _gameManager.LockButtonsInput( false ); }, true );
        }

        public void HideLevelsMenu()
        {
            _levelsMenu.ShowRoot( false );
        }

        public void NextLand()
        {
            LeanTween.delayedCall( 0.1f, ()=> { _gameManager.AudioManagerInstance.Play( AudioLibrary.UI.TweenLeftRight ); } );
            _gameManager.LockAllInputs( true );

            _currentLandID++;
            if ( _currentLandID > 2 )
                _currentLandID = 0;
            _levelsMenu.SetSprite( "LandImg2", _landSprites[_currentLandID] );

            int temp = _currentLandID + 1;
            if ( temp > 2 )
                temp = 0;
            _levelsMenu.SetSprite( "LandImg3", _landSprites[temp] );

            float time = 0.5f;
            float delay = 0.0f;
            float delayInc = 0.01f;
            float to = -7.0f;

            Transform levels = _levelsMenu.Get( "Levels" ).transform;
            float[] xPositions = new float[levels.childCount];
            for ( int i = 0; i < levels.childCount; i++ )
            {
                xPositions[i] = levels.GetChild(i).position.x;
                MoveXEaseOutBack( levels.GetChild( i ).gameObject, xPositions[i], to, time, delay+=delayInc );
            }

            LoadLandUIData( _currentLandID );

            delay += 0.1f;

            float from = 7.0f;
            for ( int i = 0; i < levels.childCount; i++ )
                MoveXEaseOutBack( levels.GetChild(i).gameObject, from, xPositions[i], time, delay += delayInc );

            MoveXEaseOutBack( _levelsMenu.Get( "LandImg1" ), 0.0f, -_gameManager.Land_Img_Pos2, time + delay, 0.0f );
            MoveXEaseOutBack( _levelsMenu.Get( "LandImg2" ), _gameManager.Land_Img_Pos2, 0.0f, time + delay, 0.0f );
            MoveXEaseOutBack( _levelsMenu.Get( "LandImg3" ), _gameManager.Land_Img_Pos3, _gameManager.Land_Img_Pos2, time + delay, 0.0f, null, 
                ()=> {
                    SetCurrentLand();
                    _gameManager.LockAllInputs( false );
                } );

            SetCurrentLand( false );
        }
        public void PreviousLand()
        {
            LeanTween.delayedCall( 0.1f, ()=> { _gameManager.AudioManagerInstance.Play( AudioLibrary.UI.TweenLeftRight ); } );
            _gameManager.LockAllInputs( true );

            _currentLandID--;
            if ( _currentLandID < 0 )
                _currentLandID = 2;
            _levelsMenu.SetSprite( "LandImg2", _landSprites[_currentLandID] );

            int temp = _currentLandID - 1;
            if ( temp < 0 )
                temp = 2;
            _levelsMenu.SetSprite( "LandImg3", _landSprites[temp] );

            float time = 0.5f;
            float delay = 0.0f;
            float delayInc = 0.01f;
            float to = 7.0f;

            Transform levels = _levelsMenu.Get( "Levels" ).transform;
            float[] xPositions = new float[levels.childCount];
            for ( int i = 0; i < levels.childCount; i++ )
            {
                xPositions[i] = levels.GetChild(i).position.x;
                MoveXEaseOutBack( levels.GetChild( i ).gameObject, xPositions[i], to, time, delay+=delayInc );
            }

            LoadLandUIData( _currentLandID );

            delay += 0.1f;

            float from = -7.0f;
            for ( int i = 0; i < levels.childCount; i++ )
                MoveXEaseOutBack( levels.GetChild(i).gameObject, from, xPositions[i], time, delay += delayInc );

            MoveXEaseOutBack( _levelsMenu.Get( "LandImg1" ), 0.0f, _gameManager.Land_Img_Pos2, time + delay, 0.0f );
            MoveXEaseOutBack( _levelsMenu.Get( "LandImg2" ), -_gameManager.Land_Img_Pos2, 0.0f, time + delay, 0.0f );
            MoveXEaseOutBack( _levelsMenu.Get( "LandImg3" ), -_gameManager.Land_Img_Pos3, -_gameManager.Land_Img_Pos2, time + delay, 0.0f, null,
                ()=> {
                    SetCurrentLand();
                    _gameManager.LockAllInputs( false );
                } );

            SetCurrentLand( false );
        }
        void SetCurrentLand( bool updateLand1Img = true )
        {
            for ( int i = 0; i < LevelManager.LAND_COUNT; i++ )
                _levelsMenu.SetSpriteColor( "PageBtn" + i, i == _currentLandID ? Color.white : Color.gray );

            if ( updateLand1Img )
            {
                _levelsMenu.SetPosX( "LandImg1", 0.0f );
                _levelsMenu.SetSprite( "LandImg1", _landSprites[_currentLandID] );
            }

            _levelsMenu.SetText( "Title", LAND_NAMES[_currentLandID] );

            _rootMenu.SetSprite( "BG", _landSprites[_currentLandID] );
        }

        void LoadLandUIData( int landID )
        {
            Transform levels = _levelsMenu.Get( "Levels" ).transform;
            
            for ( int i = 0; i < 30; i++ )
            {
                LevelData data = _gameManager.GetLevelData( landID, i );

                Transform lvl = levels.Find( "Level_" + i );

                if ( data == null || data.unlocked == 0 )
                {
                    lvl.GetComponent<Collider2D>().enabled = false;

                    lvl.Find( "Label" ).gameObject.SetActive( false );

                    for ( int star = 0; star < 3; star++ )
                        lvl.Find( "Star" + star ).gameObject.SetActive( false );

                    for ( int star = 0; star < 3; star++ )
                        lvl.Find( "Star" + star + "BG" ).gameObject.SetActive( false );

                    lvl.Find( "Lock" ).gameObject.SetActive( true );
                }
                else
                {
                    lvl.GetComponent<Collider2D>().enabled = true;

                    lvl.Find( "Label" ).gameObject.SetActive( true );
                    lvl.Find( "Label" ).GetComponent<TMPro.TextMeshPro>().text = ( i + 1 ).ToString();

                    for ( int star = 0; star < 3; star++ )
                    {
                        bool show = data.stars >= star + 1;
                        lvl.Find( "Star" + star ).gameObject.SetActive( show );
                        lvl.Find( "Star" + star + "BG" ).gameObject.SetActive( show );
                    }

                    lvl.Find( "Lock" ).gameObject.SetActive( false );
                }
            }
        }

        #endregion

        #region [Pause Menu]

        void SetupPauseMenu()
        {
            AddOnClickListener( "Pause_ResumeBtn",   OnClick_PauseMenu_ResumeBtn );
            AddOnClickListener( "Pause_RestartBtn",  OnClick_PauseMenu_RestartBtn );
            AddOnClickListener( "Pause_StoreBtn",    OnClick_PauseMenu_StoreBtn );
            AddOnClickListener( "Pause_SettingsBtn", OnClick_PauseMenu_SettingsBtn );
            AddOnClickListener( "Pause_LevelsBtn",   OnClick_PauseMenu_LevelsMenuBtn );
            AddOnClickListener( "Pause_MainMenuBtn", OnClick_PauseMenu_MainMenuBtn );
        }
        public void OnClick_PauseMenu_ResumeBtn()
        {
            _pauseMenu.ShowRoot( false );
            _gameManager.StartCountDown( false );
        }
        void OnClick_PauseMenu_RestartBtn()
        {
            _pauseMenu.ShowRoot( false );
            DisplayMessageDialog( "Restart Level", "Are you sure you want to restart level?",
                                  MessageDialogAction.OK_Cancel,
                                  ()=> { _gameManager.RestartLevel(); },
                                  ()=> { _pauseMenu.ShowRoot( true ); } );
        }
        void OnClick_PauseMenu_StoreBtn()
        {
            _pauseMenu.ShowRoot( false );
            _gameManager.GoToStoreMenu();
        }
        void OnClick_PauseMenu_SettingsBtn()
        {
            _pauseMenu.ShowRoot( false );
            _gameManager.GoToSettingsMenu();
        }
        void OnClick_PauseMenu_LevelsMenuBtn()
        {
            _pauseMenu.ShowRoot( false );
            DisplayMessageDialog( "End Game", "Are you sure you want to go back to levels menu?",
                                  MessageDialogAction.OK_Cancel,
                                  ()=> { _gameManager.EndGameAndGoBackToLevelsMenu(); },
                                  ()=> { _pauseMenu.ShowRoot( true ); } );
        }
        void OnClick_PauseMenu_MainMenuBtn()
        {
            _pauseMenu.ShowRoot( false );
            DisplayMessageDialog( "End Game", "Are you sure you want to go back to main menu?",
                                  MessageDialogAction.OK_Cancel,
                                  ()=> { _gameManager.EndGameAndGoBackToMainMenu(); },
                                  ()=> { _pauseMenu.ShowRoot( true ); } );
        }
        
        public void DisplayPauseMenu( float time = 0.5f )
        {
            _displayMenuTask = new CoroutineTask( DisplayMenu( _pauseMenu, "Pause", time ) );
        }

        #endregion
        
        #region [StartUpTimer]

        public void StartUpTimer( Action onComplete )
        {
            _displayMenuTask = new CoroutineTask( StartUpTimerCoroutine( onComplete ), true );
        }
        IEnumerator StartUpTimerCoroutine( Action onComplete )
        {
            _startupTimer.SetText( "Counter", "3" );

            OffsetAllSpritesLayerBy( _gameplayMenu.Get( "GP_Border" ), -10 );

            _startupTimer.Get( "Counter" ).transform.localScale = Vector3.zero;

            _startupTimer.ShowRoot( true );

            for ( int i = 2; i >= 0; i-- )
            {
                _gameManager.AudioManagerInstance.Play( AudioLibrary.UI.Cancel );

                _startupTimer.Get( "Counter" ).transform.localScale = Vector3.zero;
                var tween = LeanTween.scale( _startupTimer.Get( "Counter" ), Vector3.one, 0.5f );
                tween.setFrom( Vector3.zero );
                tween.setEaseOutBack();

                yield return new WaitForSeconds( 1.0f );

                _startupTimer.SetText( "Counter", i.ToString() );
            }

            _startupTimer.ShowRoot( false );

            OffsetAllSpritesLayerBy( _gameplayMenu.Get( "GP_Border" ), 10 );

            if ( onComplete != null )
                onComplete.Invoke();
        }

        #endregion

        #region [Gameplay Menu]

        class GamePlayBtn
        {
            public GameObject prefsGameObject;
            public TextMeshPro prefsText;

            public GameObject extraGameObject;
            public TextMeshPro extraText;
        }
        GamePlayBtn[] _paintersBtn;
        GamePlayBtn[] _boostersBtn;
        

        void SetupGameplayMenu()
        {
            AddOnClickListener( "GP_PauseBtn", onClick_GameplayMenu_PauseBtn );
            AddOnClickListener( "GP_B0", onClick_GameplayMenu_CustomBtn );
            AddOnClickListener( "GP_B1", onClick_GameplayMenu_BurnerBtn );
            AddOnClickListener( "GP_B2", onClick_GameplayMenu_BombBtn );

            // Add onClick listeners,
            for ( int i = 0; i < 5; i++ )
            {
                var temp = i;
                AddOnClickListener( "GP_" + temp, ()=> { onClick_GameplayMenu_PainterBtn( temp ); } );
            }

            _paintersBtn = new GamePlayBtn[5];
            for ( int i = 0; i < _paintersBtn.Length; i++ )
            {
                int temp = i;
                _paintersBtn[temp] = new GamePlayBtn();
                _paintersBtn[temp].prefsText =          _gameplayMenu.Get( "GP_" + temp ).transform.Find( "Prefs" ).GetComponent<TextMeshPro>();
                _paintersBtn[temp].prefsGameObject =    _gameplayMenu.Get( "GP_" + temp ).transform.Find( "Original" ).gameObject;
                _paintersBtn[temp].extraText =          _gameplayMenu.Get( "GP_" + temp ).transform.Find( "Extra" ).GetComponent<TextMeshPro>();
                _paintersBtn[temp].extraGameObject =    _gameplayMenu.Get( "GP_" + temp ).transform.Find( "Red" ).gameObject;
            }

            _boostersBtn = new GamePlayBtn[3];
            for ( int i = 0; i < _boostersBtn.Length; i++ )
            {
                int temp = i;
                _boostersBtn[temp] = new GamePlayBtn();
                _boostersBtn[temp].prefsText =          _gameplayMenu.Get( "GP_B" + temp ).transform.Find( "Prefs" ).GetComponent<TextMeshPro>();
                _boostersBtn[temp].prefsGameObject =    _gameplayMenu.Get( "GP_B" + temp ).transform.Find( "Original" ).gameObject;
                _boostersBtn[temp].extraText =          _gameplayMenu.Get( "GP_B" + temp ).transform.Find( "Extra" ).GetComponent<TextMeshPro>();
                _boostersBtn[temp].extraGameObject =    _gameplayMenu.Get( "GP_B" + temp ).transform.Find( "Red" ).gameObject;
            }
        }
        public void onClick_GameplayMenu_PauseBtn()
        {
            _gameplayMenu.ShowRoot( false );
            _gameManager.GoToPauseMenu();
        }
        void onClick_GameplayMenu_CustomBtn()
        {
            _gameManager.OpenCustomBlockDialog();
        }
        void onClick_GameplayMenu_BurnerBtn()
        {
            _gameManager.BurnBlock();
        }
        void onClick_GameplayMenu_BombBtn()
        {
            _gameManager.ConvertBlockToBomb();
        }
        void onClick_GameplayMenu_PainterBtn( int id )
        {
            _gameManager.ChangeBlockColor( id );
        }


        public void HideGameplay()
        {
            _gameplayMenu.ShowRoot( false );
        }

        public void DisplayGameplayMenu( int levelID, UserData data, float time = 0.5f )
        {
            for ( int i = 0; i < data.boosters.Length; i++ )
                SetGameplay_BoosterUIVal( i, data.boosters[i], data.boosterExtra[i] );

            for ( int i = 0; i < data.painters.Length; i++ )
                SetGameplay_PainterUIVal( i, data.painters[i], data.paintersExtra[i] );

            _gameplayMenu.ShowRoot( true );
            _gameplayMenu.SetAllColor( Color.white );
            if ( levelID != -100 )
            {
                _gameplayMenu.ShowItem( "LevelTitle", true );
                _gameplayMenu.SetText( "LevelTitle", "Level " + ( levelID + 1 ).ToString() );
                _gameplayMenu.ShowItem( "LevelBG", true );
                _gameplayMenu.ShowItem( "Timer", true );

                for ( int i = 0; i < 3; i++ )
                    _gameplayMenu.Get( "Star_" + i ).gameObject.SetActive( true );
            }
            else
            {
                _gameplayMenu.ShowItem( "LevelTitle", false );
                _gameplayMenu.ShowItem( "LevelBG", false );
                _gameplayMenu.ShowItem( "Timer", false );

                for ( int i = 0; i < 3; i++ )
                    _gameplayMenu.Get( "Star_" + i ).gameObject.SetActive( false );
            }

            ResetAllGamePlayButtons();
        }

        public void ResetAllGamePlayButtons()
        {
            for ( int i = 0; i < 5; i++ )
            {
                var go = _gameplayMenu.Get( "GP_" + i );
                LeanTween.cancel( go );
                go.transform.localScale = new Vector3( 0.9f, 0.9f, 1.0f );
            }

            var custom = _gameplayMenu.Get( "GP_B0" );
            LeanTween.cancel( custom );
            custom.transform.localScale = new Vector3( 0.9f, 0.9f, 1.0f );

            var burner = _gameplayMenu.Get( "GP_B1" );
            LeanTween.cancel( burner );
            burner.transform.localScale = new Vector3( 0.9f, 0.9f, 1.0f );

            var bomb = _gameplayMenu.Get( "GP_B2" );
            LeanTween.cancel( bomb );
            bomb.transform.localScale = new Vector3( 0.9f, 0.9f, 1.0f );
        }

        
        void SetGameplayValues( GamePlayBtn[] btns, int id, int prefs, int extra )
        {
            if ( prefs > 0 )
            {
                string str = prefs.ToString();
                if ( prefs > 99 )
                    str = "+99";
                btns[id].prefsGameObject.SetActive( true );
                btns[id].prefsText.text = str;
            }
            else
            {
                btns[id].prefsGameObject.SetActive( false );
                btns[id].prefsText.text = string.Empty;
            }

            if ( extra > 0 )
            {
                string str = extra.ToString();
                if ( extra > 99 )
                    str = "+99";
                btns[id].extraGameObject.SetActive( true );
                btns[id].extraText.text = str;
            }
            else
            {
                btns[id].extraGameObject.SetActive( false );
                btns[id].extraText.text = string.Empty;
            }
        }

        public void SetGameplay_PainterUIVal( int id, int prefs, int extra, bool animate = false )
        {
            SetGameplayValues( _paintersBtn, id, prefs, extra );

            if ( animate )
                TweenRewardedButton( "GP_" + id );
        }

        public void SetGameplay_BoosterUIVal( int id, int prefs, int extra, bool animate = false )
        {
            SetGameplayValues( _boostersBtn, id, prefs, extra );

            if ( animate )
                TweenRewardedButton( "GP_B" + id );
        }

        public void SetGameplay_TimerVal( string text )
        {
            _gameplayMenu.SetText( "Timer", text );
        } 

        void TweenRewardedButton( string name )
        {
            var go = _gameplayMenu.Get( name );
            _rewardTweenGOs.Add( go );
            ScaleEaseOutBack( _gameplayMenu, name, Vector3.zero, new Vector3( 0.9f, 0.9f, 1.0f ), 0.5f, 0.0f, ()=> {
                _rewardTweenGOs.Remove( go );
            } );
        }

        public void CheckRewardTween()
        {
            if ( _rewardTweenGOs != null && _rewardTweenGOs.Count > 0 )
            {
                foreach ( var go in _rewardTweenGOs )
                {
                    LeanTween.cancel( go );
                    go.transform.localScale = new Vector3( 0.9f, 0.9f, 1.0f );
                }
                _rewardTweenGOs.Clear();
            }
        } 


        public void SetGameplay_StarsCountUI( int count )
        {
            for ( int i = 0; i < 3; i++ )
            {
                bool show = i <= count - 1;
                _gameplayMenu.Get( "Star_" + i ).transform.GetChild( 0 ).gameObject.SetActive( show );
            }
        }

        #endregion

        #region [Message Dialog]

        public enum MessageDialogAction { OK, OK_Cancel }

        public void DisplayMessageDialog( string title, string msg, 
                                          MessageDialogAction actionBtn, 
                                          Action onClickOK = null, Action onClickCancel = null )
        {
            _gameManager.AudioManagerInstance.Play( AudioLibrary.UI.MessagePositive );

            _onClickUpActions.Remove( "MsgD_OKBtn" );
            _onClickUpActions.Remove( "MsgD_CancelBtn" );

            _messageDialog.SetText( "Title", title );
            _messageDialog.SetText( "Msg", msg );

            var pos = _messageDialog.Get( "MsgD_OKBtn" ).transform.localPosition;

            if ( actionBtn == MessageDialogAction.OK_Cancel )
            {
                _messageDialog.Get( "MsgD_OKBtn" ).transform.localPosition = new Vector3( 1.0f, pos.y, pos.z );
                _messageDialog.ShowItem( "MsgD_CancelBtn", true );
            }
            else if ( actionBtn == MessageDialogAction.OK )
            {
                _messageDialog.Get( "MsgD_OKBtn" ).transform.localPosition = new Vector3( 0.0f, pos.y, pos.z );
                _messageDialog.ShowItem( "MsgD_CancelBtn", false );
            }

            _messageDialog.ShowItem( "MsgD_OKBtn", true );

            Action onClickOK2 = ()=> {
                onClickOK.Invoke();
                _messageDialog.ShowRoot( false );
            };

            Action onClickCancel2 = ()=> {
                onClickCancel.Invoke();
                _messageDialog.ShowRoot( false );
            };

            AddOnClickListener( "MsgD_OKBtn", onClickOK2 );
            AddOnClickListener( "MsgD_CancelBtn", onClickCancel2 );

            _messageDialog.ShowRoot( true );
        }

        public bool IsMessageDialogDisplayed { get { return _messageDialog.IsActive; } }

        #endregion

        #region [GameOver Dialog]

        void SetupGameOverDialog()
        {
            AddOnClickListener( "GameOverDialog_ShareFBBtn", OnClick_GameOverDialog_ShareFBBtn );
            AddOnClickListener( "GameOverDialog_PlayAgainBtn", OnClick_GameOverDialog_PlayAgainBtn );
            AddOnClickListener( "GameOverDialog_NextBtn", OnClick_GameOverDialog_NextBtn );

            AddOnClickListener( "GameOverDialog_HomeBtn", OnClick_GameOverDialog_HomeBtn );
            AddOnClickListener( "GameOverDialog_LevelsBtn", OnClick_GameOverDialog_LevelsBtn );
            AddOnClickListener( "GameOverDialog_LeaderbordBtn", OnClick_GameOverDialog_LeaderbordBtn );
            AddOnClickListener( "GameOverDialog_ShopBtn", OnClick_GameOverDialog_ShopBtn );
        }
        void OnClick_GameOverDialog_ShareFBBtn()
        {
            _gameOverDialog.ShowRoot( false );
            _gameManager.ShareScoreOnFacebook();
        }
        void OnClick_GameOverDialog_PlayAgainBtn()
        {
            _gameOverDialog.ShowRoot( false );
            _gameManager.RestartLevel();
        }
        void OnClick_GameOverDialog_NextBtn()
        {
            _gameOverDialog.ShowRoot( false );
            _gameManager.LoadNextLevel();
        }
        void OnClick_GameOverDialog_HomeBtn()
        {
            _gameOverDialog.ShowRoot( false );
            _gameManager.GoToMainMenu();
        }
        void OnClick_GameOverDialog_LevelsBtn()
        {
            _gameOverDialog.ShowRoot( false );
            _gameManager.GoToLevelsMenu();
        }
        void OnClick_GameOverDialog_LeaderbordBtn()
        {
            _gameOverDialog.ShowRoot( false );
            _gameManager.GoToGameSparksLeaderBoard();
        }
        void OnClick_GameOverDialog_ShopBtn()
        {
            _gameOverDialog.ShowRoot( false );
            _gameManager.GoToStoreMenu();
        }


        public void DisplayGameOverDialog( LevelData data, int totalScore, float time = 0.5f )
        {
            _displayMenuTask = new CoroutineTask( DisplayGameOverDialogCoroutine( data, totalScore, time ) );
        }
        IEnumerator DisplayGameOverDialogCoroutine( LevelData data, int totalScore, float time = 0.5f )
        {
            _gameManager.LockAllInputs( true );

            yield return new WaitForSeconds( time * 2 );

            _gameplayMenu.ShowRoot( false );

            _gameManager.AudioManagerInstance.Play( AudioLibrary.GameOver.WinLevel );

            yield return new WaitForSeconds( 0.1f );

            _gameOverDialog.ShowAllItems( false );
            _gameOverDialog.ShowRoot( true );

            _gameManager.AudioManagerInstance.Play( AudioLibrary.UI.Scaled[0] );

            ScaleEaseOutBack( _gameOverDialog, "Dialog", time, 0.0f, null, null, true );
            ScaleEaseOutBack( _gameOverDialog, "Top", time, 0.0f, null, null, true );
            ScaleEaseOutBack( _gameOverDialog, "TopTitle", time, 0.0f, null, null, true );

            float delay = 1.0f;
            float delayIncrement = 0.05f;

            // Win :)
            if ( data.stars > 0 )
            {
                _gameOverDialog.SetText( "TopTitle", "Level " + ( data.levelID + 1 ) + " Complete" );

                for ( int i = 0; i < 3; i++ )
                {
                    ScaleEaseOutBack( _gameOverDialog, "Star" + i + "BG", time, delay, null, null, true );
                    delay += delayIncrement;
                }

                delayIncrement = 0.2f;

                for ( int i = 0; i < data.stars; i++ )
                {
                    int temp = i;
                    ScaleEaseOutBack( _gameOverDialog, "Star" + temp, time, delay, ()=> { _gameManager.AudioManagerInstance.Play( AudioLibrary.GameOver.StarsShow[temp] ); }, null, true );
                    delay += delayIncrement;
                }

                delay += delayIncrement;
                ScaleEaseOutBack( _gameOverDialog, "DurationLabel", time, delay, null, null, true );
                _gameOverDialog.SetText( "DurationVal", _gameplayMenu.GetText( "Timer" ) );
                ScaleEaseOutBack( _gameOverDialog, "DurationVal", time, delay, ()=> { _gameManager.AudioManagerInstance.Play( AudioLibrary.UI.Scaled[0] ); }, null, true );

                delay += delayIncrement;
                ScaleEaseOutBack( _gameOverDialog, "ScoreLabel", time, delay, null, null, true );
                _gameOverDialog.SetText( "ScoreVal", data.score.ToString() );
                ScaleEaseOutBack( _gameOverDialog, "ScoreVal", time, delay, ()=> { _gameManager.AudioManagerInstance.Play( AudioLibrary.UI.Scaled[0] ); }, null, true );

                delay += delayIncrement;
                ScaleEaseOutBack( _gameOverDialog, "TotalScoreLabel", time, delay, null, null, true );
                _gameOverDialog.SetText( "TotalScoreVal", totalScore.ToString() );
                ScaleEaseOutBack( _gameOverDialog, "TotalScoreVal", time, delay, ()=> { _gameManager.AudioManagerInstance.Play( AudioLibrary.GameOver.CoinsReward[1] ); }, null, true );
            }
            // Lose :(
            else
            {
                _gameOverDialog.SetText( "TopTitle", "Game Over" );

                ScaleEaseOutBack( _gameOverDialog, "FailedMsg", time, delay, ()=> { _gameManager.AudioManagerInstance.Play( AudioLibrary.UI.Scaled[0] ); }, null, true );
            }


            delay += delayIncrement;
            ScaleEaseOutBack( _gameOverDialog, "RewardBG", time, delay, null, null, true );
            ScaleEaseOutBack( _gameOverDialog, "RewardTitle", time, delay, null, null, true );

            delay += delayIncrement;
            ScaleEaseOutBack( _gameOverDialog, "Coins", time, delay, null, null, true );
            _gameOverDialog.SetText( "CoinsVal", data.coinsReward.ToString() );
            ScaleEaseOutBack( _gameOverDialog, "CoinsVal", time, delay, ()=> { _gameManager.AudioManagerInstance.Play( AudioLibrary.GameOver.CoinsReward[0] ); }, null, true );

            if ( data.bonusRewardID >= 0 )
            {
                delay += delayIncrement;
                ScaleEaseOutBack( _gameOverDialog, "BonusRewardIcon" + data.bonusRewardID, time, delay, null, null, true );
                _gameOverDialog.SetText("BonusRewardVal", "1" );
                ScaleEaseOutBack( _gameOverDialog, "BonusRewardVal", time, delay, ()=> { _gameManager.AudioManagerInstance.Play( AudioLibrary.GameOver.BoosterReward ); }, null, true );
            }


            Action action = ()=> { _gameManager.AudioManagerInstance.Play( AudioLibrary.UI.Scaled[0] ); };

            delayIncrement = 0.1f;

            if ( data.stars > 0 )
                ScaleEaseOutBack( _gameOverDialog, "GameOverDialog_ShareFBBtn", time, delay += delayIncrement, action, null, true );
            ScaleEaseOutBack( _gameOverDialog, "GameOverDialog_PlayAgainBtn", time, delay += delayIncrement, action, null, true );
            if ( data.stars > 0 )
                ScaleEaseOutBack( _gameOverDialog, "GameOverDialog_NextBtn", time, delay += delayIncrement, action, null, true );

            ScaleEaseOutBack( _gameOverDialog, "GameOverDialog_HomeBtn", time, delay += delayIncrement, action, null, true ); 
            ScaleEaseOutBack( _gameOverDialog, "GameOverDialog_LevelsBtn", time, delay += delayIncrement, action, null, true );
            ScaleEaseOutBack( _gameOverDialog, "GameOverDialog_LeaderbordBtn", time, delay += delayIncrement, action, null, true );  
            ScaleEaseOutBack( _gameOverDialog, "GameOverDialog_ShopBtn", time, delay += delayIncrement, action, ()=> { _gameManager.LockButtonsInput( false ); }, true ); 
        }

        public void ShowGameOverDialogAgain()
        {
            _gameOverDialog.ShowRoot( true );
        }

        public void HideGameOverDialog()
        {
            _gameOverDialog.ShowRoot( false );
        }

        #endregion

        #region [Win Challenge Level Dialog]

        void SetupWinChallengeLevelDialog()
        {
            AddOnClickListener( "WinChallengeLevelDialog_ShareBtn", OnClick_WinChallengeLevelDialog_ShareBtn );
            AddOnClickListener( "WinChallengeLevelDialog_FacebookMenuBtn", OnClick_WinChallengeLevelDialog_FacebookMenuBtn );
        }
        void OnClick_WinChallengeLevelDialog_ShareBtn()
        {
            _gameManager.ShareWinChallengeLevelOnFacebook();
        }
        void OnClick_WinChallengeLevelDialog_FacebookMenuBtn()
        {
            _winChallengeLevelDialog.ShowRoot( false );
            _gameManager.GoToFacebookMenu();
        }


        public void DisplayWinChallengeLevelDialog( bool rewarded, float time = 0.5f )
        {
            _displayMenuTask = new CoroutineTask( DisplayWinChallengeLevelDialogCoroutine( rewarded, time ) );
        }
        IEnumerator DisplayWinChallengeLevelDialogCoroutine( bool rewarded,float time = 0.5f )
        {
            _gameManager.LockAllInputs( true );

            yield return new WaitForSeconds( time * 2 );

            _gameplayMenu.ShowRoot( false );

            yield return new WaitForSeconds( 0.1f );
            
            _winChallengeLevelDialog.ShowAllItems( false );
            _winChallengeLevelDialog.ShowRoot( true );

            ScaleEaseOutBack( _winChallengeLevelDialog, "Dialog", time, 0.0f, null, null, true );
            ScaleEaseOutBack( _winChallengeLevelDialog, "Top", time, 0.0f, null, null, true );
            ScaleEaseOutBack( _winChallengeLevelDialog, "TopTitle", time, 0.0f, null, null, true );

            float delay = 1.0f;
            float delayIncrement = 0.05f;

            for ( int i = 0; i < 3; i++ )
            {
                ScaleEaseOutBack( _winChallengeLevelDialog, "Star" + i + "BG", time, delay, null, null, true );
                delay += delayIncrement;
            }

            delayIncrement = 0.2f;

            for ( int i = 0; i < 3; i++ )
            {
                ScaleEaseOutBack( _winChallengeLevelDialog, "Star" + i, time, delay, null, null, true );
                delay += delayIncrement;
            }

            if ( rewarded )
            {
                delay += delayIncrement;
                ScaleEaseOutBack( _winChallengeLevelDialog, "RewardBG", time, delay, null, null, true );
                ScaleEaseOutBack( _winChallengeLevelDialog, "RewardTitle", time, delay, null, null, true );

                delay += delayIncrement;
                ScaleEaseOutBack( _winChallengeLevelDialog, "Coins", time, delay, null, null, true );
                _winChallengeLevelDialog.SetText( "CoinsVal", "3" );
                ScaleEaseOutBack( _winChallengeLevelDialog, "CoinsVal", time, delay, null, null, true );
            }

            delayIncrement = 0.1f;

            ScaleEaseOutBack( _winChallengeLevelDialog, "WinChallengeLevelDialog_ShareBtn", time, delay += delayIncrement, null, null, true ); 
            ScaleEaseOutBack( _winChallengeLevelDialog, "WinChallengeLevelDialog_FacebookMenuBtn", time, delay += delayIncrement, null, ()=> { _gameManager.LockButtonsInput( false ); }, true ); 
        }

        public void HideWinChallengeLevelDialog()
        {
            _winChallengeLevelDialog.ShowRoot( false );
        }

        #endregion

        #region [Custom Block Dialog]

        void SetupCustomBlockDialog()
        {
            // Add onClick listeners,
            for ( int i = 0; i < 8; i++ )
            {
                var temp = i;
                AddOnClickListener( "CustomBlockDialog_" + temp, ()=> { OnClick_CustomBlockDialog_IDBtn( temp ); } );
            }
            
            AddOnClickListener( "CustomBlockDialog_CancelBtn", OnClick_CustomBlockDialog_CancelBtn );
        }
        void OnClick_CustomBlockDialog_IDBtn( int id )
        {
            _customBlockDialog.ShowRoot( false );
            _gameManager.GenerateCustomBlock( id );
            _gameManager.StartWithoutCountDown( false );
        }
        public void OnClick_CustomBlockDialog_CancelBtn()
        {
            _customBlockDialog.ShowRoot( false );
            _gameManager.StartCountDown( false );
        }


        public void DisplayCustomBlockDialog( int availableBlock, float time = 0.5f )
        {
            _displayMenuTask = new CoroutineTask( DisplayCustomBlockDialogCoroutine( availableBlock, time ), true );
        }

        IEnumerator DisplayCustomBlockDialogCoroutine( int availableBlock, float time = 0.5f )
        {
            _gameManager.AudioManagerInstance.Play( AudioLibrary.BoostersSFX.OnPressCustomBlock );

            _gameManager.LockButtonsInput( true );

            _gameplayMenu.ShowRoot( false );

            _customBlockDialog.ShowAllItems( false );
            _customBlockDialog.ShowRoot( true );

            _customBlockDialog.ShowItem( "BG", true );
            _customBlockDialog.SetText( "Count", availableBlock.ToString() );

            ScaleEaseOutBack( _customBlockDialog, "Dialog", time, 0.0f, null, null, true );
            ScaleEaseOutBack( _customBlockDialog, "Top", time, 0.0f, null, null, true );
            ScaleEaseOutBack( _customBlockDialog, "Title", time, 0.0f, null, null, true );

            yield return new WaitForSeconds( 0.1f );

            float delay = 0.1f;
            float delayIncrement = 0.05f;

            Action scaleAction = ()=> { _gameManager.AudioManagerInstance.Play( AudioLibrary.UI.Scaled[0] ); };

            ScaleEaseOutBack( _customBlockDialog, "CustomBlockIcon", time, delay += delayIncrement, scaleAction, null, true );
            ScaleEaseOutBack( _customBlockDialog, "Count", time, delay += delayIncrement, scaleAction, null, true );

            Vector3 from = Vector3.zero;
            Vector3 to = new Vector3( 0.25f, 0.25f, 1.0f );

            ScaleEaseOutBack( _customBlockDialog, "CustomBlockDialog_0", from, to, time, delay += delayIncrement, scaleAction, null, true );
            ScaleEaseOutBack( _customBlockDialog, "CustomBlockDialog_1", from, to, time, delay += delayIncrement, null, null, true );
            ScaleEaseOutBack( _customBlockDialog, "CustomBlockDialog_2", from, to, time, delay += delayIncrement, scaleAction, null, true );
            ScaleEaseOutBack( _customBlockDialog, "CustomBlockDialog_7", from, to, time, delay += delayIncrement, null, null, true );
            ScaleEaseOutBack( _customBlockDialog, "CustomBlockDialog_4", from, to, time, delay += delayIncrement, scaleAction, null, true );
            ScaleEaseOutBack( _customBlockDialog, "CustomBlockDialog_3", from, to, time, delay += delayIncrement, null, null, true );
            ScaleEaseOutBack( _customBlockDialog, "CustomBlockDialog_6", from, to, time, delay += delayIncrement, scaleAction, null, true );
            ScaleEaseOutBack( _customBlockDialog, "CustomBlockDialog_5", from, to, time, delay += delayIncrement, null, null, true );

            ScaleEaseOutBack( _customBlockDialog, "CustomBlockDialog_CancelBtn", time, delay += delayIncrement, scaleAction, () => { _gameManager.LockButtonsInput(false); }, true );
        }

        #endregion

        #region [Store Menu]

        float storeButton_SelectedYPos = -4.09f;
        
        void SetupStoreMenu()
        {
            _store_PaintersSubMenu = new GOGroup( _storeMenu.Get( "PaintersStoreSubMenu" ) );
            _store_BoostersSubMenu = new GOGroup( _storeMenu.Get( "BoostersStoreSubMenu" ) );
            _store_CoinsSubMenu = new GOGroup( _storeMenu.Get( "CoinsStoreSubMenu" ) );

            for ( int i = 1; i <= 5; i++ )
            {
                int temp = i;
                AddOnClickListener( "PaintersStoreSubMenu_Offer" + temp + "_BuyBtn", ()=> {
                    _gameManager.BuyPackage( "Painter" + ( temp - 1 ) );
                } );
            }

            for ( int i = 1; i <= 3; i++ )
            {
                int temp = i;
                AddOnClickListener( "BoostersStoreSubMenu_Offer" + temp + "_BuyBtn", ()=> {
                    _gameManager.BuyPackage( "Booster" + ( temp - 1 ) );
                } );
            }

            for ( int i = 1; i <= 5; i++ )
            {
                int temp = i;
                AddOnClickListener( "CoinsStoreSubmenu_Offer" + temp + "_BuyBtn", ()=> { OnClick_StoreSubMenu_CoinBtn( temp - 1 ); } );
            }

            AddOnClickListener( "StoreMenu_PaintersBtn", OnClick_StoreMenu_PaintersBtn );
            AddOnClickListener( "StoreMenu_BoostersBtn", OnClick_StoreMenu_BoostersBtn );
            AddOnClickListener( "StoreMenu_CoinsBtn", OnClick_StoreMenu_CoinsBtn );

            AddOnClickListener( "StoreMenu_BackBtn", OnClick_StoreMenu_BackBtn );
        }
        void OnClick_StoreMenu_PaintersBtn()
        {
            DisplayStore_SubMenu( _store_PaintersSubMenu, 0 );
        }
        void OnClick_StoreMenu_BoostersBtn()
        {
            DisplayStore_SubMenu( _store_BoostersSubMenu, 1 );
        }
        void OnClick_StoreMenu_CoinsBtn()
        {
            DisplayStore_SubMenu( _store_CoinsSubMenu, 2 );
        }
        public void OnClick_StoreMenu_BackBtn()
        {
            _storeMenu.ShowRoot( false );
            _gameManager.GoBackFromStoreMenu();
        }

        void OnClick_StoreSubMenu_CoinBtn( int id )
        {
            _gameManager.BuyCoinsPacakgeGameSparks( id );
        }

        public void DisplayStoreMenu( int coins, int subMenuID = 0, float time = 0.5f )
        {
            _gameManager.LockButtonsInput( true );

            _storeMenu.ShowAllItems( false );
            _storeMenu.ShowRoot( true );

            Action action = null;

            string title = "";
            switch ( subMenuID )
            {
                case 0:
                    title = "Painters";
                    action = () => { DisplayStore_SubMenu( _store_PaintersSubMenu, 0 ); };
                    break;
                case 1:
                    title = "Boosters";
                    action = () => { DisplayStore_SubMenu( _store_BoostersSubMenu, 1 ); };
                    break;
                case 2:
                    title = "Virtual Coins";
                    action = () => { DisplayStore_SubMenu( _store_CoinsSubMenu, 2 ); };
                    break;
            }
            _storeMenu.SetText( "Title", title );

            ScaleEaseOutBack( _storeMenu, "Dialog", time, 0.0f, null, null, true );
            ScaleEaseOutBack( _storeMenu, "Top",    time, 0.0f, null, null, true );
            ScaleEaseOutBack( _storeMenu, "Title",  time, 0.0f, null, null, true );

            _storeMenu.Get( "StoreMenu_PaintersBtn" ).transform.position = new Vector3( -1.1f, storeButton_SelectedYPos, 0.0f );
            _storeMenu.Get( "StoreMenu_BoostersBtn" ).transform.position = new Vector3( 0.0f, storeButton_SelectedYPos, 0.0f );
            _storeMenu.Get( "StoreMenu_CoinsBtn" ).transform.position = new Vector3( 1.1f, storeButton_SelectedYPos, 0.0f );

            ScaleEaseOutBack( _storeMenu, "StoreMenu_PaintersBtn",   time, 0.0f, null, null, true );
            ScaleEaseOutBack( _storeMenu, "StoreMenu_BoostersBtn", time, 0.0f, null, null, true );
            ScaleEaseOutBack( _storeMenu, "StoreMenu_CoinsBtn",    time, 0.0f, null, null, true );


            ScaleEaseOutBack( _storeMenu, "CoinBG", time, 0.0f, null, action, true );
            ScaleEaseOutBack( _storeMenu, "CointCount", time, 0.0f, null, action, true );
            _storeMenu.SetText( "CointCount", coins.ToString() );

            LeanTween.delayedCall( time * 2, () => { _gameManager.AudioManagerInstance.Play( AudioLibrary.UI.TweenUp, 0.3f ); } );
            var backBtnName = "StoreMenu_BackBtn";
            MoveY( _storeMenu, backBtnName, -7.5f, _storeMenu.GetPosY( backBtnName ), time, time * 2 );
        }

        void DisplayStore_SubMenu( GOGroup subMenu, int btnId, float time = 0.5f )
        {
            _gameManager.AudioManagerInstance.Play( AudioLibrary.UI.Scaled[0], 0.8f );

            Vector3 storeButton_UnselectedScale = new Vector3( 0.9f, 0.9f, 1.0f );
            float storeButton_UnselectedYPos = -4.0f;

            List<string> buttonNames = new List<string>();
            buttonNames.Add( "StoreMenu_PaintersBtn" );
            buttonNames.Add( "StoreMenu_BoostersBtn" );
            buttonNames.Add( "StoreMenu_CoinsBtn" );

            string currentBtnName = buttonNames[btnId];
            buttonNames.RemoveAt( btnId );

            string titleName = currentBtnName.Replace( "StoreMenu_", "" );
            titleName = titleName.Replace( "Btn", "" );
            if ( titleName.StartsWith( "C" ) )
                titleName = "Virtual Coins";
            _storeMenu.SetText( "Title", titleName );

            _gameManager.LockButtonsInput( true );

            HideAllStoreSubMenus();
            _storeMenu.SetSpriteColor( currentBtnName, Color.white );
            NullCurrentSprite();

            float delay = 0.0f;
            float delayIncrement = 0.05f;

            float newTime = 0.1f;
            _storeMenu.SetSpriteLayerOrder( currentBtnName, -98 );
            MoveY( _storeMenu, currentBtnName, _storeMenu.GetPosY( currentBtnName ), storeButton_SelectedYPos, newTime, delay, null );
            ScaleEaseOutBack( _storeMenu, currentBtnName, _storeMenu.GetLocalScale( currentBtnName ), Vector3.one, newTime, delay, null );
            foreach ( var btnName in buttonNames )
            {
                _storeMenu.SetSpriteLayerOrder( btnName, -97 );
                MoveY( _storeMenu, btnName, _storeMenu.GetPosY( btnName ), storeButton_UnselectedYPos, newTime, delay, null );
                ScaleEaseOutBack( _storeMenu, btnName,  _storeMenu.GetLocalScale( btnName ), storeButton_UnselectedScale, newTime, delay, null );
            }

            subMenu.ShowAllItems( false );
            subMenu.ShowRoot( true );

            Transform parent = subMenu.GetRoot().transform;
            for ( int i = 0; i < parent.childCount - 1; i++ )
                ScaleEaseOutBack( subMenu, parent.GetChild( i ).name, time, delay += delayIncrement, null, null, true );

            ScaleEaseOutBack( subMenu, parent.GetChild( parent.childCount - 1 ).name, time, delay += delayIncrement , null, () => { _gameManager.LockButtonsInput( false ); }, true );
        }

        void HideAllStoreSubMenus()
        {
            _store_PaintersSubMenu.ShowRoot( false );
            _store_BoostersSubMenu.ShowRoot( false );
            _store_CoinsSubMenu.ShowRoot( false );

            Color grayColor = new Color( 0.9f, 0.9f, 0.9f, 1.0f );
            _storeMenu.SetSpriteColor( "StoreMenu_PaintersBtn", grayColor );
            _storeMenu.SetSpriteColor( "StoreMenu_BoostersBtn", grayColor );
            _storeMenu.SetSpriteColor( "StoreMenu_CoinsBtn", grayColor );
        }

        public void HideStoreMenu()
        {
            _storeMenu.ShowRoot( false );
        }

        public void DisplayNeedMoreCoinsMessage()
        {
            _storeMenu.ShowRoot( false );
            DisplayMessageDialog( "Buy Coins", 
                                  "You don't have enough coins to buy this item, Would you like to buy more coins?", 
                                  MessageDialogAction.OK_Cancel,
                                  ()=> { DisplayStoreMenu( _gameManager.GetTotalCoins(), 2 ); },
                                  ()=> { _storeMenu.ShowRoot( true ); } );
        }

        #endregion

        #region [Facebook Menu]

        void SetupFacebookMenu()
        {
            AddOnClickListener( "FB_LogInBtn", onClick_FB_LogInBtn );
            AddOnClickListener( "FB_LogOutBtn", onClick_FB_LogOutBtn );
            AddOnClickListener( "FB_ShareAppBtn", onClick_FB_ShareAppBtn );
            AddOnClickListener( "FB_ChallengeFriendBtn", onClick_FB_ChallengeFriendBtn );
            AddOnClickListener( "FB_PendingInvitaioinBtn", onClick_FB_PendingInvitaioinBtn );
            AddOnClickListener( "FB_LoginAtStatupBtn", onClick_FB_LogInAtStatupBtn );
            AddOnClickListener( "FB_BackBtn", OnClick_FB_BackBtn );
        }
        void onClick_FB_LogInBtn()
        {
            _gameManager.LogInFacebook();
        }
        void onClick_FB_LogOutBtn()
        {
            _gameManager.LogOutFacebook();
        }
        void onClick_FB_ShareAppBtn()
        {
            _gameManager.InviteFacebookFriends();
        }
        void onClick_FB_ChallengeFriendBtn()
        {
            _facebookMenu.ShowRoot( false );
            _gameManager.GoToInGameLevelBuilderEditor();
        }
        void onClick_FB_PendingInvitaioinBtn()
        {
            _facebookMenu.ShowRoot( false );
            _gameManager.GoToPendingInvitationMenu();
        }
        void onClick_FB_LogInAtStatupBtn()
        {
            _gameManager.EnableFBLogInStartup( !_gameManager.gameSettings.loginFB );
            _facebookMenu.Get( "FB_LoginAtStatupBtn" ).transform.Find( "Disable" ).gameObject.SetActive( !_gameManager.gameSettings.loginFB );
        }
        public void OnClick_FB_BackBtn()
        {
            _facebookMenu.ShowRoot( false );
            _gameManager.GoBackFromFacebookMenu();
        }

        public void DisplayFacebookMenu( float time = 0.5f )
        {
            List<string> toBeIgonred = new List<string>();
            if ( _gameManager.IsFBLoggedIn )
                toBeIgonred.Add( "FB_LogInBtn" );
            else
                toBeIgonred.Add( "FB_LogOutBtn" );

            _facebookMenu.Get( "FB_LoginAtStatupBtn" ).transform.Find( "Disable" ).gameObject.SetActive( !_gameManager.gameSettings.loginFB );

            _displayMenuTask = new CoroutineTask( DisplayMenu( _facebookMenu, "FB", time, 5.0f, toBeIgonred ) );
        }

        public void UpdateFacebookLoginState()
        {
            _facebookMenu.ShowItem( "FB_LogInBtn", !_gameManager.IsFBLoggedIn );
            _facebookMenu.ShowItem( "FB_LogOutBtn", _gameManager.IsFBLoggedIn );
        }

        public void HideFacebookMenu()
        {
            _facebookMenu.ShowRoot( false );
        }

        #endregion

        #region [GameSparks Menu]

        void SetupGameSparksMenu()
        {
            AddOnClickListener( "GameSparksMenu_LogInBtn", OnClick_GameSparksMenu_LogInBtn );
            AddOnClickListener( "GameSparksMenu_LogOutBtn", OnClick_GameSparksMenu_LogOutBtn );
            AddOnClickListener( "GameSparksMenu_ChangePasswordBtn", OnClick_GameSparksMenu_ChangePasswordBtn );
            AddOnClickListener( "GameSparksMenu_LeaderbordBtn", OnClick_GameSparksMenu_LeaderbordBtn );
            AddOnClickListener( "GameSparksMenu_BackBtn", OnClick_GameSparksMenu_BackBtn );
        }
        void OnClick_GameSparksMenu_LogInBtn()
        {
            _gameSpraksMenu.ShowRoot( false );
            _gameManager.GoToGameSparksSignInMenu();
        }
        void OnClick_GameSparksMenu_LogOutBtn()
        {
            _gameSpraksMenu.ShowRoot( false );
            _gameManager.SignOutFromGameSparksAccount();
        }
        void OnClick_GameSparksMenu_ChangePasswordBtn()
        {
            _gameSpraksMenu.ShowRoot( false );
            if ( _gameManager.IsGameSparksAuthenticated )
                _gameManager.GoToGameSparksChangePasswordMenu();
            else
                _gameManager.GoToGameSparksSignInMenu();
        }
        void OnClick_GameSparksMenu_LeaderbordBtn()
        {
            _gameSpraksMenu.ShowRoot( false );
            _gameManager.GoToGameSparksLeaderBoard();
        }
        public void OnClick_GameSparksMenu_BackBtn()
        {
            _gameSpraksMenu.ShowRoot( false );
            _gameManager.GoToMainMenu();
        }


        public void DisplayGameSparksMenu( float time = 0.5f )
        {
            List<string> toBeIgonred = new List<string>();
            if ( _gameManager.IsGameSparksAuthenticated )
                toBeIgonred.Add( "GameSparksMenu_LogInBtn" );
            else
                toBeIgonred.Add( "GameSparksMenu_LogOutBtn" );
            _displayMenuTask = new CoroutineTask( DisplayMenu( _gameSpraksMenu, "GameSparksMenu", time, 5.0f, toBeIgonred ) );
        }

        public void HideGameSparksMenu()
        {
            _gameSpraksMenu.ShowRoot( false );
        }

        #endregion

        #region [Tutorials Menu]

        void SetupTutorialsMenu()
        {
            AddOnClickListener( "Tutorials_ExitBtn", OnClick_Tutorials_ExitBtn );

            for ( int i = 0; i < 4; i++ )
            {
                int temp = i;
                AddOnClickListener( "Tutorials_PageBtn" + temp, ()=> { DisplayTutorialPage( temp ); } );
            }
        }
        public void OnClick_Tutorials_ExitBtn()
        {
            LeanTween.cancelAll();
            _tutorialMenu.ShowRoot( false );
            _gameManager.GoBackFromTutorialsMenu();
        }


        public void DisplayTutorialsMenu( int id, bool procedural = false )
        {
            _tutorialMenu.ShowAllItems( false );
            _tutorialMenu.ShowRoot( true );
            _tutorialMenu.ShowItem( "BG", true );
            _tutorialMenu.ShowItem( "Tutorials_ExitBtn", true );

            DisplayTutorialPage( id, procedural );
        }

        void DisplayTutorialPage( int id, bool procedural = false )
        {
            _gameManager.LockButtonsInput( true );
            LeanTween.cancelAll();

            for ( int i = 0; i < 4; i++ )
                _tutorialMenu.ShowItem( i.ToString(), i == id );

            if ( !procedural )
            {
                for ( int i = 0; i < 4; i++ )
                {
                    _tutorialMenu.ShowItem( "Tutorials_PageBtn" + i, true );
                    _tutorialMenu.SetSpriteColor( "Tutorials_PageBtn" + i, i == id ? Color.white : Color.gray );
                }
            }

            switch ( id )
            {
                case 0:
                    PlayTutorial0( procedural );
                    break;
                case 1:
                    PlayTutorial1( procedural );
                    break;
                case 2:
                    PlayTutorial2( procedural );
                    break;
                case 3:
                    PlayTutorial3( procedural );
                    break;
            }
        }

        Action CreateProceduralAction( int id, bool procedural )
        {
            Action proceduralAction = null;
            if ( procedural )
                proceduralAction = () => { LeanTween.delayedCall( 0.5f, () => { DisplayTutorialsMenu( id, true ); } ); };
            return proceduralAction;
        }

        void PlayTutorial0( bool procedural = false )
        {
            GameObject blockGO = _tutorialMenu.Get( "0" ).transform.Find( "Block" ).gameObject;
            GameObject touchGO = _tutorialMenu.Get( "0" ).transform.Find( "Touch" ).gameObject;

            touchGO.SetActive( false );
            blockGO.SetActive( false );

            Action proceduralAction = CreateProceduralAction( 1, procedural );

            Action action3 = ()=> { MoveX( blockGO, 0.5f, 0.0f, 0.5f, 0.0f, proceduralAction ); }; 
            Action action2 = ()=> { MoveX( touchGO, 1.5f, 0.0f, 0.5f, 0.0f, action3 ); }; 
            Action action1 = ()=> { MoveY( blockGO, 1.4f, 0.5f, 1.5f, 0.0f, action2 ); };
            Action action0 = ()=> { MoveX( blockGO, 0.0f, 0.5f, 0.5f, 0.0f, action1 ); }; 
            MoveY( blockGO, 2.65f, 1.4f, 1.5f, 0.5f, null );
            MoveX( touchGO, 0.0f, 1.5f, 0.5f, 1.5f, action0 );

            blockGO.transform.localPosition = new Vector3(  0, 2.65f, 0 );
            touchGO.transform.localPosition = new Vector3(  0.0f, -1.06f, 0 );

            if ( !procedural )
                TutorialDisplayPageFinalAction( 0 );
            else
                _gameManager.LockButtonsInput( false );
        }
        void PlayTutorial1( bool procedural = false )
        {
            var idStr = "1";
            GameObject blockGO = _tutorialMenu.Get( idStr ).transform.Find( "Block" ).gameObject;
            GameObject touchGO = _tutorialMenu.Get( idStr ).transform.Find( "Touch" ).gameObject;
            GameObject tapGO = _tutorialMenu.Get( idStr ).transform.Find( "Tap" ).gameObject;

            touchGO.SetActive( true );
            blockGO.SetActive( false );
            tapGO.SetActive( false );
            
            Action proceduralAction = CreateProceduralAction( 2, procedural );

            Action action0 = ()=> {
                tapGO.transform.localPosition = touchGO.transform.localPosition;
                touchGO.SetActive( false );
                tapGO.SetActive( true );

                LeanTween.delayedCall( 0.1f, ()=> {
                    touchGO.SetActive( true );
                    tapGO.SetActive( false );
                    var tween = LeanTween.rotateZ( blockGO, 0.0f, 1.0f );
                    tween.setOnComplete( proceduralAction );
                } );
            }; 
            MoveY( blockGO, 2.65f, 1.4f, 1.5f, 0.5f, action0 );

            blockGO.transform.localPosition = new Vector3(  0, 2.65f, 0 );
            blockGO.transform.localRotation = Quaternion.Euler( 0, 0, 90.0f );
            touchGO.transform.localPosition = new Vector3(  0.0f, -1.06f, 0 );

            if ( !procedural )
                TutorialDisplayPageFinalAction( 1 );
            else
                _gameManager.LockButtonsInput( false );
        }
        void PlayTutorial2( bool procedural = false )
        {
            var idStr = "2";
            GameObject blockGO = _tutorialMenu.Get( idStr ).transform.Find( "Block" ).gameObject;
            GameObject touchGO = _tutorialMenu.Get( idStr ).transform.Find( "Touch" ).gameObject;
            GameObject tapGO = _tutorialMenu.Get( idStr ).transform.Find( "Tap" ).gameObject;
            GameObject block2GO = _tutorialMenu.Get( idStr ).transform.Find( "Block2" ).gameObject;

            touchGO.SetActive( true );
            blockGO.SetActive( false );
            tapGO.SetActive( false );
            block2GO.SetActive( false );
            
            Action proceduralAction = CreateProceduralAction( 3, procedural );

            Action action0 = ()=> {
                tapGO.transform.localPosition = touchGO.transform.localPosition;
                touchGO.SetActive( false );
                tapGO.SetActive( true );

                LeanTween.delayedCall( 0.1f, ()=> {
                    touchGO.SetActive( true );
                    tapGO.SetActive( false );

                    block2GO.transform.localPosition = blockGO.transform.localPosition;
                    blockGO.SetActive( false );
                    block2GO.SetActive( true );

                    LeanTween.delayedCall( 0.1f, proceduralAction );
                } );
            }; 
            MoveY( blockGO, 2.65f, 1.4f, 1.5f, 0.5f, null );
            touchGO.transform.localPosition = new Vector2( 0.0f, 0.6f );
            var tween = LeanTween.move( touchGO, new Vector2( -1.05f, -1.9f ), 1.5f );
            tween.setOnComplete( action0 );

            blockGO.transform.localPosition = new Vector3(  0, 2.65f, 0 );

            if ( !procedural )
                TutorialDisplayPageFinalAction( 2 );
            else
                _gameManager.LockButtonsInput( false );
        }
        void PlayTutorial3( bool procedural = false )
        {
            var idStr = "3";
            GameObject blockGO = _tutorialMenu.Get( idStr ).transform.Find( "Block" ).gameObject;
            GameObject touchGO = _tutorialMenu.Get( idStr ).transform.Find( "Touch" ).gameObject;
            GameObject tapGO = _tutorialMenu.Get( idStr ).transform.Find( "Tap" ).gameObject;
            GameObject block2GO = _tutorialMenu.Get( idStr ).transform.Find( "Block2" ).gameObject;

            touchGO.SetActive( true );
            blockGO.SetActive( false );
            tapGO.SetActive( false );
            block2GO.SetActive( false );
            
            Transform row = _tutorialMenu.Get( idStr ).transform.Find( "Row" );
            Transform explosion = _tutorialMenu.Get( idStr ).transform.Find( "Explosion" );
            _tutorialMenu.Get( idStr ).transform.Find( "HTop" ).gameObject.SetActive( false );
            for ( int i = 0; i < row.childCount; i++ )
            {
                row.GetChild( i ).gameObject.SetActive( true );
                explosion.GetChild( i ).gameObject.SetActive( false );
            } 
            for ( int i = 0; i < 3; i++ )
                row.Find( "H"+i ).gameObject.SetActive( false );

            Action proceduralAction = null;
            if ( procedural )
                proceduralAction = () => { LeanTween.delayedCall( 0.5f, OnClick_Tutorials_ExitBtn ); };

            Action action2 = ()=> {
                block2GO.SetActive( false );
                _tutorialMenu.Get( idStr ).transform.Find( "HTop" ).gameObject.SetActive( true );
                for ( int i = 0; i < 3; i++ )
                    row.Find( "H"+i ).gameObject.SetActive( true );

                float delay = 0.0f;
                float delayInc = 0.1f;
                for ( int i = 0; i < row.childCount; i++ )
                {
                    int temp = i;
                    LeanTween.delayedCall( delay+=delayInc, ()=> {
                        row.GetChild( temp ).gameObject.SetActive( false );
                        explosion.GetChild( temp ).gameObject.SetActive( true );

                        LeanTween.delayedCall( 1.0f, proceduralAction );
                    } );
                }
            };
            Action action1 = ()=> {
                MoveY( block2GO, 1.4f, 0.08f, 1.5f, 0.0f, action2 );
            };
            Action action0 = ()=> {
                tapGO.transform.localPosition = touchGO.transform.localPosition;
                touchGO.SetActive( false );
                tapGO.SetActive( true );

                LeanTween.delayedCall( 0.1f, ()=> {
                    touchGO.SetActive( true );
                    tapGO.SetActive( false );

                    block2GO.transform.localPosition = blockGO.transform.localPosition;
                    blockGO.SetActive( false );
                    block2GO.SetActive( true );

                    action1();
                } );
            }; 
            MoveY( blockGO, 2.65f, 1.4f, 1.5f, 0.5f, null );
            touchGO.transform.localPosition = new Vector2( 0.0f, 0.6f );
            var tween = LeanTween.move( touchGO, new Vector2( -1.05f, -1.9f ), 1.5f );
            tween.setOnComplete( action0 );

            blockGO.transform.localPosition = new Vector3(  0.04f, 2.65f, 0 );

            if ( !procedural )
                TutorialDisplayPageFinalAction( 3 );
            else
                _gameManager.LockButtonsInput( false );
        }

        void TutorialDisplayPageFinalAction( int id )
        {
            LeanTween.delayedCall( 0.1f, ()=> {
                _tutorialMenu.ShowItem( "Tutorials_PageBtn" + id, true );
                _tutorialMenu.SetSpriteColor( "Tutorials_PageBtn" + id, Color.white );
                NullCurrentSprite();
                _gameManager.LockButtonsInput( false );
            } );
        }

        #endregion

        #region [Notification Menu]

        public void DisplayNewInvitationNotification( string name, Sprite sprite )
        {
            GameObject[] hiddenText = null;
            if ( _gameManager.IsStatisticsState )
            {
                hiddenText = new GameObject[2];
                hiddenText[0] = _mainMenu.Get( "UserProfileBtn" ).transform.Find( "Name" ).gameObject;
                hiddenText[0].SetActive( false );
                hiddenText[1] = _statisticsMenu.Get( "Name" );
                hiddenText[1].SetActive( false );
            }
            else if ( _gameManager.IsFBPendingState )
            {
                hiddenText = new GameObject[1];
                hiddenText[0] = _gameManager.GetTitleGOFBPendingInvitationMenu();
                hiddenText[0].SetActive( false );
            }
            else if ( _gameManager.IsGameplayState )
            {
                hiddenText = new GameObject[7];

                hiddenText[0] = _mainMenu.Get( "GP_B0" ).transform.Find( "Prefs" ).gameObject;
                hiddenText[0].SetActive( false );
                hiddenText[1] = _mainMenu.Get( "GP_B0" ).transform.Find( "Extra" ).gameObject;
                hiddenText[1].SetActive( false );

                hiddenText[2] = _mainMenu.Get( "GP_B1" ).transform.Find( "Prefs" ).gameObject;
                hiddenText[2].SetActive( false );
                hiddenText[3] = _mainMenu.Get( "GP_B1" ).transform.Find( "Extra" ).gameObject;
                hiddenText[3].SetActive( false );

                hiddenText[4] = _mainMenu.Get( "GP_B2" ).transform.Find( "Prefs" ).gameObject;
                hiddenText[4].SetActive( false );
                hiddenText[5] = _mainMenu.Get( "GP_B2" ).transform.Find( "Extra" ).gameObject;
                hiddenText[5].SetActive( false );

                hiddenText[6] = _statisticsMenu.Get( "LevelTitle" );
                hiddenText[6].SetActive( false );
            }

            _notificationMenu.SetText( "Name", name );
            _notificationMenu.SetSprite( "Img", sprite );
            _notificationMenu.ShowRoot( true );
            new CoroutineTask( HideNewInvitationNotification( hiddenText ) );
        }

        IEnumerator HideNewInvitationNotification( GameObject[] hiddenTexts )
        {
            yield return new WaitForSeconds( 5.0f );
            _notificationMenu.ShowRoot( false );
            foreach ( var item in hiddenTexts )
                item.SetActive( true );
        }


        public void DisplayNewLandNotification( int id )
        {
            // Set new background img.
            _rootMenu.SetSprite( "BG", _landSprites[id] );

            // Show notification.
            _newLandNotificationMenu.SetText( "Name", LAND_NAMES[id] );
            _newLandNotificationMenu.ShowRoot( true );
            new CoroutineTask( HideNewLandNotification() );
        }

        IEnumerator HideNewLandNotification()
        {
            yield return new WaitForSeconds( 5.0f );
            _newLandNotificationMenu.ShowRoot( false );
        }

        #endregion

        // ******************************************************* //

        #region [Tween Fuctions]

        void ScaleEaseOutBack( GOGroup group, string elementName, float time = 1.0f, float delay = 0.0f, Action onStart = null, Action onComplete = null, bool forceScaleImmediately = false )
        {
            ScaleEaseOutBack( group, elementName, Vector3.zero, Vector3.one, time, delay, onStart, onComplete, forceScaleImmediately );
        }

        void ScaleEaseOutBack( GOGroup group, string elementName, Vector3 from, Vector3 to, float time = 1.0f, float delay = 0.0f, Action onStart = null, Action onComplete = null, bool forceScaleImmediately = false )
        {
            if ( forceScaleImmediately )
                group.Get( elementName ).transform.localScale = from;

            LTDescr tween = LeanTween.scale( group.Get( elementName ), to, time );
            if ( onStart != null )
                LeanTween.delayedCall( delay, onStart );

            tween.setDelay( delay );
            tween.setFrom( from );
            tween.setEaseOutBack();

            if ( onComplete != null )
                tween.setOnComplete( onComplete );

            group.ShowItem( elementName, true );
        }


        void MoveXEaseOutBack( GOGroup group, string elementName, float from, float to, float time = 1.0f, float delay = 0.0f, Action onStart = null, Action onComplete = null )
        {
            LTDescr tween = LeanTween.moveLocalX(group.Get(elementName), to, time);
            tween.setDelay(delay);
            tween.setFrom(from);
            tween.setEaseOutBack();

            if ( onStart != null )
                LeanTween.delayedCall( delay, onStart );

            if (onComplete != null)
                tween.setOnComplete(onComplete);

            group.SetPosX(elementName, from);
            group.ShowItem(elementName, true);
        }

        void MoveXEaseOutBack( GameObject go, float from, float to, float time = 1.0f, float delay = 0.0f, Action onStart = null, Action onComplete = null )
        {
            LTDescr tween = LeanTween.moveLocalX( go, to, time );
            tween.setDelay( delay );
            tween.setFrom( from );
            tween.setEaseOutBack();

            if ( onStart != null )
                LeanTween.delayedCall( delay, onStart );

            if ( onComplete != null )
                tween.setOnComplete(onComplete);

            go.transform.position = new Vector3( from, go.transform.position.y, go.transform.position.z );
            go.SetActive( true );
        }


        void MoveX( GOGroup group, string elementName, float from, float to, float time = 1.0f, float delay = 0.0f, Action onComplete = null )
        {
            LTDescr tween = LeanTween.moveLocalX(group.Get(elementName), to, time);
            tween.setFrom(from);
            tween.setDelay(delay);

            if (onComplete != null)
                tween.setOnComplete(onComplete);

            group.SetPosX(elementName, from);
            group.ShowItem(elementName, true);
        }

        void MoveX( GameObject go, float from, float to, float time = 1.0f, float delay = 0.0f, Action onComplete = null )
        {
            LTDescr tween = LeanTween.moveLocalX( go, to, time );
            tween.setFrom( from );
            tween.setDelay( delay );

            if ( onComplete != null )
                tween.setOnComplete( onComplete );

            var pos = go.transform.localPosition;
            pos.x = from;
            go.transform.localPosition = pos;
            go.SetActive( true );
        }

        void MoveY( GOGroup group, string elementName, float from, float to, float time = 1.0f, float delay = 0.0f, Action onComplete = null )
        {
            LTDescr tween = LeanTween.moveLocalY(group.Get(elementName), to, time);
            tween.setFrom( from );
            tween.setDelay( delay );

            if (onComplete != null)
                tween.setOnComplete(onComplete);

            group.SetPosY(elementName, from);
            group.ShowItem(elementName, true);
        }

        void MoveY( GameObject go, float from, float to, float time = 1.0f, float delay = 0.0f, Action onComplete = null )
        {
            LTDescr tween = LeanTween.moveLocalY( go, to, time );
            tween.setFrom( from );
            tween.setDelay( delay );

            if ( onComplete != null )
                tween.setOnComplete( onComplete );

            var pos = go.transform.localPosition;
            pos.y = from;
            go.transform.localPosition = pos;
            go.SetActive( true );
        }

        IEnumerator DisplayMenu( GOGroup menu, string menuName, float time = 1.0f, float startFromX = 5.0f, 
                                 List<string> toBeIgnored = null, Action onCompleteAction = null )
        {
            _gameManager.LockButtonsInput( true );

            menu.ShowAllItems( false );
            menu.ShowRoot( true );
            
            _gameManager.AudioManagerInstance.Play( AudioLibrary.UI.Scaled[0] );

            ScaleEaseOutBack( menu, "Dialog",  time, 0.0f, null, null, true );
            ScaleEaseOutBack( menu, "Top",     time, 0.0f, null, null, true );
            ScaleEaseOutBack( menu, "Title",   time, 0.0f, null, null, true );

            yield return new WaitForSeconds( time * 0.5f );

            float delay = 0.2f;
            float delayInc = 0.0f;

            Action moveXAction = ()=> { _gameManager.AudioManagerInstance.Play( AudioLibrary.BlockSFX.SwipeLeftRight[1], 0.5f ); };

            foreach ( var item in menu.Items )
            {
                if ( ( toBeIgnored == null || !toBeIgnored.Contains( item.Key ) ) &&
                     !item.Key.EndsWith( "_BackBtn" ) &&
                     item.Key.StartsWith( menuName + "_" ) && 
                     item.Key.EndsWith( "Btn" ) )
                {
                    MoveXEaseOutBack( item.Value, startFromX, menu.GetPosX( item.Key ), time, time * ( delay += delayInc ), moveXAction );
                    delayInc = 0.2f;
                }   
            }

            var backBtnName = menuName + "_BackBtn";
            if ( menu.Contains( backBtnName ) )
            {
                LeanTween.delayedCall( time * ( delay + delayInc ), ()=> { _gameManager.AudioManagerInstance.Play( AudioLibrary.UI.TweenUp, 0.3f ); } );

                MoveY( menu, backBtnName, -7.5f, menu.GetPosY( backBtnName ), time, time * ( delay += delayInc ), () => {
                    _gameManager.LockButtonsInput( false );
                    if ( onCompleteAction != null )
                        onCompleteAction();
                } );
            } 
            else
                LeanTween.delayedCall( delay, () => {
                    _gameManager.LockButtonsInput( false );
                    if ( onCompleteAction != null )
                        onCompleteAction();
                } );
        }

        #endregion

        #region [Utilities]

        void OffsetAllSpritesLayerBy( GameObject root, int val )
        {
            SpriteRenderer[] renderers = root.GetComponentsInChildren<SpriteRenderer>();
            foreach ( var render in renderers )
                render.sortingOrder += val;
        }
        
        public Collider2D GetBorderCollider()
        {
            return _gameplayMenu.Get( "GP_Border" ).GetComponent<Collider2D>();
        }

        #endregion

        #region [Click Button]

        public void AddOnClickListener( string btnName, Action onClickAction )
        {
            if ( _onClickUpActions.ContainsKey( btnName ) )
                _onClickUpActions[btnName] = onClickAction;
            else
                _onClickUpActions.Add( btnName, onClickAction );
        }

        public void OnClickButtonStart( SpriteRenderer render )
        {
            _currentSprite = new ClickedSprite(render.name, render, render.color);
            render.color = _onClickStartColor;
        }

        public void OnClickButtonEnd( string btnName )
        {
            if ( _currentSprite != null && _currentSprite.name == btnName )
            {
                Action action = null;
                if ( _onClickUpActions.TryGetValue( btnName, out action ) )
                {
                    _gameManager.AudioManagerInstance.Play( AudioLibrary.UI.Click );
                    action.Invoke();
                } 
            }

            ResetCurrentSpriteColor();
        }

        public void ResetCurrentSpriteColor()
        {
            if ( _currentSprite != null && 
                 _currentSprite.render )
            {
                _currentSprite.render.color = _currentSprite.originalColor;
            }
        }

        public void NullCurrentSprite()
        {
            _currentSprite = null;
        }

        public void DebugNullCurrentSprite()
        {
            Debug.Log( _currentSprite.name );
        }

        #endregion


        public GameObject GetMenuGO( string name )
        {
            return _rootMenu.Get( name );
        }

        //*********************************************
        public void StartGameplayTest()
        {
            _rootMenu.ShowItem( "BG", true );
            _gameplayMenu.ShowRoot( true );
        }
    }
}