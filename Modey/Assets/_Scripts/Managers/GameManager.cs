using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MANA3DGames.Utilities.Coroutine;
using MANA3DGames.Utilities.Services;
using GameSparks.Core;

namespace MANA3DGames
{
    public class GameManager : MonoBehaviour
    {   
        #region [Enumaration]

        enum GameState
        {
            Startup,
            MainMenu,
            MainMenu_Settings,
            MainMenu_Settings_Tutorials,
            MainMenu_FB,
            MainMenu_FB_LevelBuilder,
            MainMenu_FB_PendingInvitations,
            MainMenu_Info,
            MainMenu_Statistics,
            MainMenu_Store,
            MainMenu_GameSpraks,
            MainMenu_GameSpraks_Leaderboard,
            LevelsMenu,
            LevelsMenu_Store,
            LevelsMenu_Leaderboard,
            CountDownStart,
            Gameplay,
            Gameplay_Tutorials,
            Gameplay_Store,
            CustomBlock,
            Paused,
            Paused_Settings,
            Paused_Settings_Tutorials,
            Paused_Store,
            GameOver,
            GameOver_Store,
            GameOver_Failed,
            GameOver_Leaderboard,
            WinChallengeLevel
        }

        #endregion


        #region [Variables]

        MANAServices                    _mana3dServices;
        UIManager                       _uiManager;
        InputManager                    _inputManager;
        LevelManager                    _levelManager;
        LocalGameDatabase               _database;
        ActiveUser                      _activeUser;
        IAPManager                      _inAppPurchase;
        GameSettings                    _gameSettings;
        GameState                       _gameState;
        FacebookManager                 _fbManager;
        GameSparksManager               _gameSparks;
        GameSparksSignInMenu            _gameSparksSignInMenu;
        GameSparksCreateNewAccountMenu  _gameSparksCreateNewAccount;
        GameSparksChangePasswordMenu    _gameSparksChangePasswordDialog;
        GameSparksLeaderboardMenu       _gameSparksLeaderboardMenu;
        InGameLevelBuilder              _levelBuilder;
        PendingInvitationsMenu          _pendingInvitationMenu;
        AudioManager                    _audioManager;

        public AudioManager AudioManagerInstance { get { return _audioManager; } }
        public GameSettings gameSettings { get { return _gameSettings; } }

        public bool         IsMainMenuState { get { return _gameState == GameState.MainMenu; } }
        public bool         IsStatisticsState { get { return _gameState == GameState.MainMenu_Statistics; } }
        public bool         IsFBPendingState { get { return _gameState == GameState.MainMenu_FB_PendingInvitations; } }
        public bool         IsPausedState { get { return _gameState == GameState.Paused || _gameState == GameState.Paused_Settings; } }
        public bool         IsStartupTimerState { get { return _gameState == GameState.CountDownStart; } }
        public bool         IsGameplayState { get { return _gameState == GameState.Gameplay; } }
        public bool         IsFBLoggedIn { get { return _fbManager.IsLoggedIn; } }
        public bool         IsGameSparksAuthenticated { get { return _gameSparks.IsModeyAuthenticated; } }//_gameSparks.IsAuthenticated; } }

        public const string CLOUDFILENAME_REGISTERDUSER = "RegisteredUserFile";
        public const string CLOUDFILENAME_USERDATA      = "UserDataFile";
        public const string CLOUDFILENAME_LANDSDATA     = "LandsDataFile";

        public const string LB_TOPSCORES_ID = "CgkI6Y2q0LkJEAIQAQ";

        float landImgPos2;
        public float Land_Img_Pos2 { get { return landImgPos2; } }
        float landImgPos3;
        public float Land_Img_Pos3 { get { return landImgPos3; } }

        Camera _cam;

        CoroutineTask _timerTask;

        #endregion


        #region [MonoBehavior]

        void Awake()
        {
            Init();
        }

        void Update()
        {
            CheckInput();
        }

        private void OnApplicationQuit()
        {
            if ( _gameSparks != null )
            {
                _gameSparks.EndSession();
                _gameSparks.Disconnect();
            }
        }

        #endregion


        #region [Startup]

        void Init()
        {
            // Config Android screen resolution.
            ConfigResoultion();

            _cam = Camera.main;

            // Init all manager instances.
            _uiManager      = new UIManager( this );
            _inputManager   = new InputManager( this );
            _levelManager   = new LevelManager( this );
            _mana3dServices = new MANAServices();
            _fbManager      = new FacebookManager( this );
            _database       = new LocalGameDatabase();
            
            _fbManager.Init();

            if ( _database.IsFirstTime() )
                _database.ApplyFirstTimeAction();

            _gameSettings   = _database.LoadGameSettings();
            _audioManager   = new AudioManager( _uiManager.GetMenuGO( "AudioSources" ).transform, this );

            _levelBuilder           = new InGameLevelBuilder( _uiManager.GetMenuGO( "InGameLevelBuilderEditor" ), _uiManager, this );
            _pendingInvitationMenu  = new PendingInvitationsMenu( _uiManager, this, _fbManager );

            _gameSparksSignInMenu           = new GameSparksSignInMenu( this, _uiManager );
            _gameSparksCreateNewAccount     = new GameSparksCreateNewAccountMenu( this, _uiManager );
            _gameSparksChangePasswordDialog = new GameSparksChangePasswordMenu( this, _uiManager );
            _gameSparksLeaderboardMenu      = new GameSparksLeaderboardMenu( _uiManager, this );

            _inAppPurchase          = GetComponent<IAPManager>();
            InitStore();

            _inputManager.SetBorderCollider( _uiManager.GetBorderCollider() );

            EnableBGM( _gameSettings.bgm, false );
            EnableFBLogInStartup( _gameSettings.loginFB, false );

            _gameSparks = new GameSparksManager();

            _uiManager.ShowGameSparksLogo( true );

            // Start the app
            StartupLogos();
        }

        void StartupLogos()
        {
            //_uiManager.FadeOutGameSparksLogo( ()=> {
            //    _uiManager.ShowMANA3DGamesLogo( true );
            //    _uiManager.FadeOutMANA3DGamesLogo( ()=> {
            //        _uiManager.ShowMANA3DGamesLogo( false );
            //        _uiManager.DisplayLoadingScreen( "Checking \nInternet Connection" );
            //        _mana3dServices.CheckInternetConnection( ( isConnected )=> {
            //            if ( isConnected )
            //                CheckNewGameUpdate();
            //            else
            //                StartGameWithLocalUser();
            //        } );
            //    }, 1.0f );
            //}, 1.0f );
            
            _uiManager.FadeOutGameSparksLogo( ()=> {
                _uiManager.ShowGameSparksLogo( false );
                _uiManager.DisplayLoadingScreen( "Checking \nInternet Connection" );
                _mana3dServices.CheckInternetConnection( ( isConnected )=> {
                    if ( isConnected )
                        CheckNewGameUpdate();
                    else
                        StartGameWithLocalUser();
                } );
            }, 1.0f );
        }

        void StartGame()
        {
            _uiManager.FadeOutLoadingScreen( ()=> {
                _audioManager.PlayBGM();
                GoToMainMenu( 1.0f );
            }, 0.5f );
        }

        #endregion


        #region [Local User]

        void StartGameWithLocalUser()
        {
            SetGuestUser();
            StartGame();
        }

        void SetGuestUser()
        {
            _activeUser = new ActiveUser();
            _activeUser.userData = _database.LoadUserData();

            _uiManager.SetUserProfileBtn_Name( "Guest User" );
            _uiManager.SetUserProfileBtn_Avatar( _uiManager.GameSparksSprite );
            _uiManager.DisplayMainMenuUserProfileBtn();
        }

        #endregion


        #region [Input Manager]

        void CheckInput()
        {
            if ( Input.GetKeyDown( KeyCode.K ) )
            {
                _gameSparks.DownloadUserData( ( data ) => { Debug.Log( data.JSON ); }, ()=> { Debug.Log( "Failed" ); }, _uiManager );
            }
            if ( Input.GetKeyDown( KeyCode.P ) )
            {
                var userData = new UserData();
                userData = new UserData();
                userData.coins = 0;
                userData.boosters = new int[3];
                userData.painters = new int[5];

                var cloudLandsData = new CloudLandsData();
                cloudLandsData.UnlockLevel( 0, 0 );

                var user = new ActiveUser();
                user.userData = userData;
                user.landsData = cloudLandsData;

                _gameSparks.UploadUserData( user, ()=> { Debug.Log( "Succeed" ); }, ()=> { Debug.Log( "Failed" ); }, _uiManager );
            }



            if ( Input.GetKeyDown( KeyCode.Escape ) ) 
                EscAction();

            _inputManager.Update();

            if ( _gameSparksSignInMenu.IsActive )
                _gameSparksSignInMenu.Update();
            else if ( _gameSparksCreateNewAccount.IsActive )
                _gameSparksCreateNewAccount.Update();
            else if ( _gameSparksChangePasswordDialog.IsActive )
                _gameSparksChangePasswordDialog.Update();

            if ( _gameState == GameState.Gameplay )
                _levelManager.Update();
        }

        void EscAction()
        {
            if ( _inputManager.IsLockButtons ||
                 _uiManager.IsMessageDialogDisplayed ||
                 _uiManager.IsLoadingScreen )
                return;

            switch ( _gameState )
            {
                case GameState.MainMenu:
                    _mana3dServices.MinimizeApp();
                    break;
                case GameState.MainMenu_Statistics:
                    _uiManager.OnClick_StatisticsMenu_BackBtn();
                    break;
                case GameState.MainMenu_Settings:
                    _uiManager.OnClick_SettingsMenu_BackBtn();
                    break;
                case GameState.MainMenu_Settings_Tutorials:
                    _uiManager.OnClick_Tutorials_ExitBtn();
                    break;
                case GameState.MainMenu_FB:
                    _uiManager.OnClick_FB_BackBtn();
                    break;
                case GameState.MainMenu_FB_PendingInvitations:
                    _pendingInvitationMenu.OnClickBackBtn();
                    break;
                case GameState.MainMenu_FB_LevelBuilder:
                    _levelBuilder.OnClickBackBtn();
                    break;
                case GameState.MainMenu_Info:
                    _uiManager.OnClick_InfoMenu_BackBtn();
                    break;
                case GameState.MainMenu_Store:
                    _uiManager.OnClick_StoreMenu_BackBtn();
                    break;
                case GameState.MainMenu_GameSpraks:
                    _gameSparksChangePasswordDialog.Close();
                    _gameSparksCreateNewAccount.Close();
                    _gameSparksLeaderboardMenu.Close();
                    _gameSparksSignInMenu.Close();
                    _uiManager.OnClick_GameSparksMenu_BackBtn();
                    break;
                case GameState.LevelsMenu:
                    _uiManager.OnClickLevelsMenu_HomeBtn();
                    break;
                case GameState.LevelsMenu_Store:
                    _uiManager.OnClick_StoreMenu_BackBtn();
                    break;
                case GameState.Gameplay:
                    _uiManager.onClick_GameplayMenu_PauseBtn();
                    break;
                case GameState.Gameplay_Store:
                    _uiManager.OnClick_StoreMenu_BackBtn();
                    break;
                case GameState.CustomBlock:
                    _uiManager.OnClick_CustomBlockDialog_CancelBtn();
                    break;
                case GameState.Paused:
                    _uiManager.OnClick_PauseMenu_ResumeBtn();
                    break;
                case GameState.Paused_Settings:
                    _uiManager.OnClick_SettingsMenu_BackBtn();
                    break;
                case GameState.Paused_Settings_Tutorials:
                    _uiManager.OnClick_Tutorials_ExitBtn();
                    break;
                case GameState.Paused_Store:
                    _uiManager.OnClick_StoreMenu_BackBtn();
                    break;
                case GameState.GameOver:
                    _mana3dServices.MinimizeApp();
                    break;
                case GameState.GameOver_Store:
                    _uiManager.OnClick_StoreMenu_BackBtn();
                    break;
            }
        }


        public void LockButtonsInput( bool lockBtn )
        {
            _inputManager.LockButtons( lockBtn );
        }

        public void LockAllInputs( bool val, bool resetCurrentSpriteColor = true )
        {
            _inputManager.LockAll( val );
            if ( resetCurrentSpriteColor )
                _uiManager.ResetCurrentSpriteColor();
        }

        #endregion


        #region [UI Manager]

        public void OnClickButtonStart( SpriteRenderer render )
        {
            _uiManager.OnClickButtonStart( render );
        }
        public void OnClickButtonEnd( string btnName )
        {
            _uiManager.OnClickButtonEnd( btnName );
        }

        public void ResetCurrentSpriteColor()
        {
            _uiManager.ResetCurrentSpriteColor();
        }

        #endregion


        #region [Game Settings]

        public void GoToSettingsMenu()
        {
            if ( _gameState == GameState.MainMenu )
                _gameState = GameState.MainMenu_Settings;
            else if ( _gameState == GameState.Paused )
                _gameState = GameState.Paused_Settings;

            _uiManager.DisplaySettingsMenu();
        }
        public void GoBackFromSettingsMenu()
        {
            if ( _gameState == GameState.MainMenu_Settings )
            {
                GoToMainMenu( 0.5f );
            }
            else if ( _gameState == GameState.Paused_Settings )
            {
                _uiManager.DisplayPauseMenu( 0.5f );
                _gameState = GameState.Paused;
            }
        }

        void ConfigResoultion()
        {
            landImgPos2 = 7.2f;
            landImgPos3 = 14.4f;

            int resolutionSum = Screen.width + Screen.height;
            if ( resolutionSum >= 3000 )
                Screen.SetResolution( 1080, 1920, true );
            else if ( resolutionSum >= 2000 )
                Screen.SetResolution( 720, 1280, true );
            else if ( resolutionSum >= 1500 )
            {
                Screen.SetResolution( 540, 960, true );
            }
            else if ( resolutionSum >= 800 )
            {
                Screen.SetResolution( 320, 480, true );

                var scale = new Vector3( 1.2f, 1.0f, 1.0f );
                transform.Find( "BG" ).localScale = scale;

                var LevelsMenuTrans = transform.Find( "LevelsMenu" );
                for ( int i = 1; i <= 3; i++ )
                    LevelsMenuTrans.Find( "LandImg" + i ).localScale = scale;

                landImgPos2 = 8.585f;
                landImgPos3 = 17.22f;
            }
            else
                Application.Quit();
        }

        public void EnableSFX( bool enable, bool save = true )
        {
            _gameSettings.sfx = enable;

            if ( save )
            {
                _gameSettings.sfx = enable;
                _database.SaveGameSettings( _gameSettings );
            }
        }

        public void EnableBGM( bool enable, bool save = true )
        {
            _gameSettings.bgm = enable;
            _audioManager.EnableBGM( enable );

            if ( save )
            {
                _gameSettings.bgm = enable;
                _database.SaveGameSettings( _gameSettings );
            }
        }

        public void EnableStartupTimer( bool enable, bool save = true )
        {
            _gameSettings.startupTimer = enable;

            if ( save )
            {
                _gameSettings.startupTimer = enable;
                _database.SaveGameSettings( _gameSettings );
            }
        }

        public void EnableFBLogInStartup( bool enable, bool save = true )
        {
            _gameSettings.loginFB = enable;

            if ( save )
            {
                _gameSettings.loginFB = enable;
                _database.SaveGameSettings( _gameSettings );
            }
        }

        public void ApplyDefaultSettings()
        {
            _database.ApplyDefaultSettings();
            _gameSettings   = _database.LoadGameSettings();
            _audioManager.EnableBGM( true );
        }

        #endregion

        
        #region [Main Menu]
        
        public void GoToMainMenu( float time = 0.5f )
        {
            _gameState = GameState.MainMenu;
            _uiManager.DisplayMainMenu( time );
        }

        public void GoToStatisticsMenu()
        {
            _gameState = GameState.MainMenu_Statistics;

            StatisticsData data = new StatisticsData();

            if ( IsGameSparksAuthenticated )
            {
                _uiManager.DisplayLoadingScreen( "Loading User Data" );

                _gameSparks.RetrieveUserScore( ( score, rank ) => {
                    // Fill in statistics info.
                    data.name = _gameSparks.DisplayName;
                    data.score = (int)score;
                    data.rank = rank;
                    data.coins = _activeUser.userData.coins;
                    data.painters = _activeUser.userData.painters;
                    data.boosters = _activeUser.userData.boosters;

                    data.land = _activeUser.landsData.GetCurrentLand();
                    data.level = _activeUser.landsData.GetCurrentLevel( data.land );
                    data.starsCount = _activeUser.landsData.GetStarsCount();

                    _uiManager.FadeOutLoadingScreen( () =>
                    {
                        _uiManager.DisplayStatisticsMenu( data );
                    }, 0.1f );

                    } );
            }
            else
            {
                _activeUser.userData = _database.LoadUserData();

                // Fill in statistics info.
                data.name = "Guest User";
                data.score = _database.CalculateTotalScore();
                data.rank = -1;
                data.coins = _activeUser.userData.coins;
                data.painters = _activeUser.userData.painters;
                data.boosters = _activeUser.userData.boosters;
                data.land = _database.GetCurrentLand();
                data.level = _database.GetCurrentLevel( data.land );
                data.starsCount = _database.GetStarsCount();

                _uiManager.DisplayStatisticsMenu( data );
            }
        }

        public void GoToInfoMenu()
        {
            _gameState = GameState.MainMenu_Info;
            _uiManager.DisplayInfoMenu();
        }

        #endregion

        
        #region [Tutorials]
 
        public void GoToTutorialsMenu( int subMenuId = 0 )
        {
            if ( _gameState == GameState.MainMenu_Settings )
                _gameState = GameState.MainMenu_Settings_Tutorials;
            else if ( _gameState == GameState.Paused_Settings )
                _gameState = GameState.Paused_Settings_Tutorials;

            _uiManager.DisplayTutorialsMenu( subMenuId );
        }
        public void GoBackFromTutorialsMenu()
        {
            if ( _gameState == GameState.MainMenu_Settings_Tutorials )
            {
                _gameState = GameState.MainMenu;
                GoToSettingsMenu();
            }
            else if ( _gameState == GameState.Paused_Settings_Tutorials )
            {
                _gameState = GameState.Paused_Settings;
                GoToSettingsMenu();
            }
            else if ( _gameState == GameState.Gameplay_Tutorials )
            {
                LoadLevel( 0, 0 );
            }
        }

        #endregion


        #region [Pause Game]

        public void GoToPauseMenu()
        {
            PauseGame();
            _uiManager.DisplayPauseMenu();
            _gameState = GameState.Paused;
        }

        public void PauseGame()
        {
            LeanTween.pauseAll();
            _levelManager.OnGamePaused();
            _inputManager.LockSwipe( true );
            if ( _timerTask != null )
                _timerTask.pause();
        }

        void ResumeGame()
        {
            LeanTween.resumeAll();
            _levelManager.OnGameResumed();
            _inputManager.LockSwipe( false );
            _gameState = GameState.Gameplay;
            if ( _timerTask != null )
                _timerTask.unPause();
        }

        #endregion


        #region [Level & Gameplay]

        public void GoToLevelsMenu()
        {
            if ( PlayerPrefs.GetInt( "ShowTutorial" ) == 0 )
            {
                PlayerPrefs.SetInt( "ShowTutorial", 1 );
                _gameState = GameState.Gameplay_Tutorials;
                _uiManager.DisplayTutorialsMenu( 0, true );
                return;
            }

            DisplayLevelsMenu();
        }
        void DisplayLevelsMenu()
        {
            _gameState = GameState.LevelsMenu;
            _uiManager.DisplayLevelsMenu();
        }

        public void LoadLevel( int landID, int levelID )
        {
            if ( !IsGameSparksAuthenticated )
                _activeUser.userData = _database.LoadUserData();

            _levelManager.LoadLevel( landID, levelID );
            _levelManager.CalulcateDurationLimit();

            InitTimer();
            StartCountDown( true );
        }
        void StartLevelChallenage( string senderName, string levelJosn )
        {
            _uiManager.FadeOutLoadingScreen( ()=> {
                if ( !IsGameSparksAuthenticated )
                    _activeUser.userData = _database.LoadUserData();

                _levelManager.LoadLevel( new LevelManager.ChallengeLevelData( senderName, levelJosn ) );
                InitTimer();
                StartCountDown( true );
            }, 0.1f );
        }
        void RestartChallengeLevel()
        {
            if ( !IsGameSparksAuthenticated )
                _activeUser.userData = _database.LoadUserData();

            _levelManager.RestartChallengeLevel();
            StartCountDown( true );
        }


        public void LoadNextLevel()
        {
            int landID = _levelManager.LandID;
            int levelID = _levelManager.LevelID;

            if ( levelID == 29 )
            {
                if ( landID < 2 )
                {
                    landID++;
                    levelID = 0;
                    _uiManager.DisplayNewLandNotification( landID );
                }
                else
                {
                    // This is the Final Level.
                    _uiManager.DisplayMessageDialog( "Congratulation!", 
                                                     "You've saved all MODEYS!\n\nStay tuned for more awesome levels!", 
                                                     UIManager.MessageDialogAction.OK, 
                                                     ()=> { GoToMainMenu(); } );
                    return;
                }
            }
            else
                levelID++;

            LoadLevel( landID, levelID );
        }

        public void StartCountDown( bool yahooSFX )
        {
            if ( !_gameSettings.startupTimer )
            {
                StartWithoutCountDown( yahooSFX );
                return;
            }

            _gameState = GameState.CountDownStart;

            _inputManager.LockButtons( true );

            _uiManager.StartUpTimer( ()=> { OnCountDownComplete( yahooSFX ); } );

            _levelManager.ShowAllSprites( true );
            _levelManager.OffsetAllSpritesLayerBy( -10 );

            _uiManager.DisplayGameplayMenu( _levelManager.LevelID, _activeUser.userData );
        }

        void OnCountDownComplete( bool yahooSFX )
        {
            if ( yahooSFX )
                _audioManager.Play( AudioLibrary.ModeySFX.WhileSaving[0] );

            _levelManager.OffsetAllSpritesLayerBy( 10 );
            _inputManager.LockButtons( false );
            ResumeGame();
        }

        public void StartWithoutCountDown( bool yahooSFX )
        {
            if ( yahooSFX )
                _audioManager.Play( AudioLibrary.ModeySFX.WhileSaving[0] );

            _levelManager.ShowAllSprites( true );
            _uiManager.DisplayGameplayMenu( _levelManager.LevelID, _activeUser.userData );
            _inputManager.LockButtons( false );
            ResumeGame();
        }


        public void RestartLevel()
        {
            EndGame();
            
            if ( _levelManager.LevelID == InGameLevelBuilder.INDEX )
                RestartChallengeLevel();
            else
                LoadLevel( _levelManager.LandID, _levelManager.LevelID );
        }

        public void EndGameAndGoBackToLevelsMenu()
        {
            EndGame();
            DisplayLevelsMenu();
        }
        public void EndGameAndGoBackToMainMenu()
        {
            EndGame();
            GoToMainMenu( 0.5f );
        }


        void EndGame()
        {
            _uiManager.CheckRewardTween();
            _uiManager.ResetAllGamePlayButtons();
            LeanTween.cancelAll();
            _levelManager.OnEndLevel();
            _inputManager.LockSwipe( false );
            StopTimer();
        }
        void EndGameAndHideGamePlay()
        {
            _uiManager.HideGameplay();
            EndGame();
        }

        public void OnLevelEnd( LevelData data )
        {
            EndGame();

            if ( data.coinsReward > 0 )
                UpdateUserAndLandsData( data );
            else
                GoToGameOverMenu( data, 0 );
        }
        public void OnWinChallengeLevel()
        {
            _gameState = GameState.WinChallengeLevel;
            EndGameAndHideGamePlay();

            if ( IsGameSparksAuthenticated )
            {
                _activeUser.userData.coins += 3;
                _uiManager.DisplayLoadingScreen( "Submitting \nUser Cloud Data" );
                _gameSparks.UploadUserData( _activeUser, 
                    () => { _uiManager.FadeOutLoadingScreen( () => { _uiManager.DisplayWinChallengeLevelDialog( true ); }, 0.1f ); },
                    () => { _uiManager.FadeOutLoadingScreen( () => { GoToMainMenu(); }, 0.1f ); },
                    _uiManager
                );
            }
            else
                _uiManager.DisplayWinChallengeLevelDialog( false );
        }
        public void OnLoseChallengeLevel( string senderName )
        {
            EndGameAndHideGamePlay();
            _uiManager.DisplayMessageDialog( "Game Over", 
                                             "You lose to " + senderName + "\n\nTry Again?",
                                             UIManager.MessageDialogAction.OK_Cancel,
                                             RestartChallengeLevel,
                                             ()=> { GoToMainMenu(); } );
        }


        void UpdateUserAndLandsData( LevelData data )
        {
            if ( IsGameSparksAuthenticated )
                SubmitUserData( data );
            else
                SaveGameDataLocal( data );
        }
        void SaveGameDataLocal( LevelData data )
        {
            _database.SaveLevelData( data );
            int totalScore = 0;
            // Check if win level.
            if ( data.stars > 0 )
            {
                totalScore = _database.CalculateTotalScore();

                // Unlock next level/land.
                if ( data.levelID + 1 < LevelManager.LEVEL_COUNT )
                    _database.UnlockLevel( data.landID, data.levelID + 1 );
                else if ( data.landID + 1 < LevelManager.LAND_COUNT )
                    _database.UnlockLevel( data.landID + 1, 0 );
            }

            GoToGameOverMenu( data, totalScore );
        }


        public GameObject GetBorder()
        {
            return _uiManager.GetBorderCollider().gameObject;
        }

        public LevelData GetLevelData( int landID, int levelID )
        {
            if ( IsGameSparksAuthenticated )
            {
                if ( _activeUser.landsData.lands[landID] == null ||
                     _activeUser.landsData.lands[landID].levels == null )
                    return null;
                else
                    return _activeUser.landsData.lands[landID].levels[levelID];
            }
            else
                return _database.LoadLevelData( landID, levelID );
        }

        public void GoToGameOverMenu( LevelData data, int totalScore )
        {
            _uiManager.DisplayGameOverDialog( data, totalScore );
            _gameState = GameState.GameOver;
        }


        IEnumerator TimerCoroutine()
        {
            int seconds = 0;
            int minutes = 0;

            while ( true )
            {
                yield return new WaitForSeconds( 1.0f );
                seconds++;

                if ( seconds >= 60 )
                {
                    seconds = 0;
                    minutes++;
                }

                string secondsStr = seconds.ToString();
                string minutesStr = minutes.ToString();

                if ( minutes > 99 )
                    minutesStr = "+99";

                if ( seconds < 10 )
                    secondsStr = "0" + secondsStr;
                if ( minutes < 10 )
                    minutesStr = "0" + minutesStr;

                _uiManager.SetGameplay_TimerVal( minutesStr + ":" + secondsStr );
                int totalSec = ( minutes * 60 ) + seconds;
                _levelManager.UpdateGameDuration( totalSec );
            }
        }
        
        void InitTimer()
        {
            StopTimer();
            _timerTask = new CoroutineTask( TimerCoroutine() );
            _timerTask.pause();
            _uiManager.SetGameplay_TimerVal( "00:00" );
        }
        void StopTimer()
        {
            if ( _timerTask != null )
            {
                _timerTask.kill();
                _timerTask = null;
            }
        }


        public void SetUIStarsCount( int count )
        {
            _uiManager.SetGameplay_StarsCountUI( count );
        }

        #endregion


        #region [Input/Block Movements]

        public void OnLeftSwipe()
        {
            if ( _gameState == GameState.Gameplay )
                _levelManager.MoveX( -1 );
            else if ( _gameState == GameState.LevelsMenu )
                _uiManager.NextLand();
            else if ( _gameState == GameState.MainMenu_FB_PendingInvitations )
                _pendingInvitationMenu.NextPage();
        }
        public void OnRightSwipe()
        {
            if ( _gameState == GameState.Gameplay )
                _levelManager.MoveX( 1 );
             else if ( _gameState == GameState.LevelsMenu )
                _uiManager.PreviousLand();
            else if ( _gameState == GameState.MainMenu_FB_PendingInvitations )
                _pendingInvitationMenu.PreviousPage();
        }
        public void SpeedUpMoveY()
        {
            _levelManager.SpeedUpMoveY();
        }
        public void PushDown()
        {
            _levelManager.PushDown();
        }
        public void RotateBlock()
        {
            _levelManager.RotateBlock();
        }

        public void OnBlockLanded()
        {
            _inputManager.OnBlockLanded();
        }

        #endregion


        #region [Painters & Boosters]

        public void SetExtraItemsForCurrentLevel( Level level )
        {
            // Init user arraies.
            _activeUser.userData.paintersExtra = new int[level.ColorBlocksCount.Length];
            _activeUser.userData.boosterExtra = new int[level.BoostersCount.Length];

            // Copy arries contents.
            level.ColorBlocksCount.CopyTo( _activeUser.userData.paintersExtra, 0 );
            level.BoostersCount.CopyTo( _activeUser.userData.boosterExtra, 0 );

            // Extra Block Colors.
            int length = _activeUser.userData.paintersExtra.Length;
            for ( int i = 0; i < length; i++ )
                _uiManager.SetGameplay_PainterUIVal( i, _activeUser.userData.painters[i], _activeUser.userData.paintersExtra[i] );

            // Extra Boosters.
            for ( int i = 0; i < _activeUser.userData.boosters.Length; i++ )
                _uiManager.SetGameplay_BoosterUIVal( i, _activeUser.userData.boosters[i], _activeUser.userData.boosterExtra[i] );
        }


        public void ChangeBlockColor( int id )
        {
            if ( _activeUser.userData.paintersExtra[id] + _activeUser.userData.painters[id] > 0 )
                _levelManager.ChangeBlockColor( id );
            else
            {
                _audioManager.Play( AudioLibrary.PainterSFX.Error );
                GoToStoreFromGameplay( 0 );
            }
        }

        public int GetFirstAvailablePainterID()
        {
            for ( int i = 0; i < 5; i++ )
            {
                if ( _activeUser.userData.paintersExtra[i] + _activeUser.userData.painters[i] > 0 )
                    return i;
            }

            _levelManager.LoseLevel();
            return -1;
        }

        public void UseOnePainter( int id )
        {
            if ( _activeUser.userData.paintersExtra[id] > 0 )
                _activeUser.userData.paintersExtra[id]--;
            else if ( _activeUser.userData.painters[id] > 0 )
            {
                _activeUser.userData.painters[id]--;

                if ( !IsGameSparksAuthenticated )
                    _database.SaveUserData( _activeUser.userData );
            }
            else
            {
                //Debug.Log( "This case should not happen once I do the picker logic" );
                // Error...
            }

            _uiManager.SetGameplay_PainterUIVal( id, _activeUser.userData.painters[id], _activeUser.userData.paintersExtra[id] );
        }


        public void OpenCustomBlockDialog()
        {
            if ( _activeUser.userData.boosters[0] <= 0 && 
                 _activeUser.userData.boosterExtra[0] <= 0 )
            {
                _audioManager.Play( AudioLibrary.BoostersSFX.Error );
                GoToStoreFromGameplay( 1 );
                return;
            }

            if ( !_levelManager.CanUseBooster() )
            {
                _audioManager.Play( AudioLibrary.BoostersSFX.Error );
                return;
            }

            PauseGame();
            _gameState = GameState.CustomBlock;
            _uiManager.DisplayCustomBlockDialog( _activeUser.userData.boosters[0] + _activeUser.userData.boosterExtra[0] );
        }

        public void GenerateCustomBlock( int id )
        {
            if ( _activeUser.userData.boosterExtra[0] > 0 )
                _activeUser.userData.boosterExtra[0]--;
            else
                _activeUser.userData.boosters[0]--;

            if ( !IsGameSparksAuthenticated )
                _database.SaveUserData( _activeUser.userData );

            _uiManager.SetGameplay_BoosterUIVal( 0, _activeUser.userData.boosters[0], _activeUser.userData.boosterExtra[0] );
            _levelManager.GenerateCustomBlock( id );
        }

        public void BurnBlock()
        {
            if ( _activeUser.userData.boosters[1] <= 0 &&
                 _activeUser.userData.boosterExtra[1] <= 0 )
            {
                GoToStoreFromGameplay( 1 );
                return;
            }

            if ( !_levelManager.CanUseBooster() )
            {
                _audioManager.Play( AudioLibrary.BoostersSFX.Error );
                return;
            }

            if ( _activeUser.userData.boosterExtra[1] > 0 )
                _activeUser.userData.boosterExtra[1]--;
            else
                _activeUser.userData.boosters[1]--;

            if ( !IsGameSparksAuthenticated )
                _database.SaveUserData( _activeUser.userData );

            _uiManager.SetGameplay_BoosterUIVal( 1, _activeUser.userData.boosters[1], _activeUser.userData.boosterExtra[1] );
            _levelManager.BurnBlock();
        }

        public void ConvertBlockToBomb()
        {
            if ( _activeUser.userData.boosters[2] <= 0 &&
                 _activeUser.userData.boosterExtra[2] <= 0 )
            {
                GoToStoreFromGameplay( 1 );
                return;
            }

            if ( !_levelManager.CanUseBooster( 2 ) )
            {
                _audioManager.Play( AudioLibrary.BoostersSFX.Error );
                return;
            }

            if ( _activeUser.userData.boosterExtra[2] > 0 )
                _activeUser.userData.boosterExtra[2]--;
            else
                _activeUser.userData.boosters[2]--;

            if ( !IsGameSparksAuthenticated )
                _database.SaveUserData( _activeUser.userData );

            _uiManager.SetGameplay_BoosterUIVal( 2, _activeUser.userData.boosters[2], _activeUser.userData.boosterExtra[2] );
            _levelManager.ConvertBlockToBomb();
        }


        public void RewardPainter( int id )
        {
            _activeUser.userData.paintersExtra[id]++;
            _uiManager.SetGameplay_PainterUIVal( id, _activeUser.userData.painters[id], _activeUser.userData.paintersExtra[id], true );
        } 

        public void RewardBooster( int typeID )
        {
            _activeUser.userData.boosterExtra[typeID]++;
            _uiManager.SetGameplay_BoosterUIVal( typeID, _activeUser.userData.boosters[typeID], _activeUser.userData.boosterExtra[typeID], true );
        }

        #endregion


        #region [Facebook]

        public void LogInFacebook( Action onLoginAction = null )
        {
            _uiManager.HideFacebookMenu();
            _uiManager.DisplayLoadingScreen( "Siginning In to Facebook Account..." );
            _fbManager.Login( onLoginAction );
        }
        public void OnFacebookLogIn( Action onLoginAction )
        {
            _uiManager.FadeOutLoadingScreen( ()=> {
                if ( onLoginAction != null )
                    onLoginAction();
                else
                    GoToFacebookMenu();
            }, 0.1f );
        }

        public void LogOutFacebook()
        {
            _fbManager.LogOut();
            OnFacebookLogOut();
        }
        public void OnFacebookLogOut()
        {
            _uiManager.FadeOutLoadingScreen( ()=> {
                GoToFacebookMenu();
            }, 0.1f );
        }

        public void OnFacebookFailedToLogIn()
        {
            _uiManager.FadeOutLoadingScreen( ()=> {
                if ( _gameState == GameState.Startup )
                {
                    _audioManager.PlayBGM();
                    GoToMainMenu( 1.0f );
                }
                else if ( _gameState == GameState.MainMenu_FB )
                {
                    GoToFacebookMenu();
                }
                else if ( _gameState == GameState.WinChallengeLevel )
                {
                    _uiManager.HideWinChallengeLevelDialog();
                    GoToFacebookMenu();
                }
                else if ( _gameState == GameState.GameOver )
                {
                    _uiManager.ShowGameOverDialogAgain();
                }
            }, 0.1f );
        }


        public void InviteFacebookFriends()
        {
            if ( _fbManager.IsLoggedIn )
                _fbManager.InviteFriends();
            else
            {
                LogInFacebook( ()=> { GoToFacebookMenu(); _fbManager.InviteFriends(); } );
            }
        }

        public void ShareScoreOnFacebook()
        {
            if ( _activeUser != null )
            {
                if ( _fbManager.IsLoggedIn )
                    _fbManager.ShareScore( _activeUser.tempScore );
                else
                    LogInFacebook( ()=> { _fbManager.ShareScore( _activeUser.tempScore ); } );
            }
        }
        public void OnShareScoreComplete()
        {
            _uiManager.ShowGameOverDialogAgain();
        }
        public void OnFaildToShareScore()
        {
            _uiManager.ShowGameOverDialogAgain();
        }

        public void ShareWinChallengeLevelOnFacebook()
        {
            if ( _fbManager.IsLoggedIn )
                _fbManager.ShareWinChallengeLevel( _levelManager.LevelSenderName );
            else
                LogInFacebook( ()=> { _fbManager.ShareWinChallengeLevel( _levelManager.LevelSenderName ); } );
        }


        public void GoToFacebookMenu()
        {
            _gameState = GameState.MainMenu_FB;
            _uiManager.DisplayFacebookMenu();
        }
        public void GoBackFromFacebookMenu()
        {
            GoToMainMenu( 0.5f );
        }
        public void GoToInGameLevelBuilderEditor()
        {
            if ( !IsGameSparksAuthenticated )
            {
                _uiManager.DisplayMessageDialog( "GameSparks", 
                                                 "You have to be signed in to your GameSparks account in order to build a level.\n\nWould you like to Sign In?",
                                                 UIManager.MessageDialogAction.OK_Cancel, 
                                                 ()=> {
                                                     _gameState = GameState.MainMenu_GameSpraks;
                                                     GoToGameSparksSignInMenu();
                                                 },
                                                 GoToFacebookMenu );
                return;
            }

            Action action = ()=> {
                _gameState = GameState.MainMenu_FB_LevelBuilder;
                _levelBuilder.Display();
            };

            if ( _fbManager.IsLoggedIn )
                action();
            else
                LogInFacebook( action );
        }
        public void GoToPendingInvitationMenu()
        {
            if ( _fbManager.IsLoggedIn )
            {
                _uiManager.DisplayLoadingScreen( "Checking Pending Invitations" );
                _fbManager.CheckPendingChallenge( true );
                _gameState = GameState.MainMenu_FB_PendingInvitations;
            }
            else
                LogInFacebook( GoToPendingInvitationMenu );
        }

        public void ShowNewInvitationNotification( string name, Sprite sprite )
        {
            _uiManager.DisplayNewInvitationNotification( name, sprite );
        }

        #endregion


        #region [Store Functions]

        Dictionary<string, StoreItem> _storeItems;

        void InitStore()
        {
            _storeItems = new Dictionary<string, StoreItem>();

            _storeItems.Add( "Painter0", new StoreItem( "Painter0", 10, 10, 0 ) );
            _storeItems.Add( "Painter1", new StoreItem( "Painter1", 10, 10, 1 ) );
            _storeItems.Add( "Painter2", new StoreItem( "Painter2", 10, 15, 2 ) );
            _storeItems.Add( "Painter3", new StoreItem( "Painter3", 10, 15, 3 ) );
            _storeItems.Add( "Painter4", new StoreItem( "Painter4", 10, 20, 4 ) );

            _storeItems.Add( "Booster0", new StoreItem( "Booster0", 5, 25, 0 ) );
            _storeItems.Add( "Booster1", new StoreItem( "Booster1", 5, 15, 1 ) );
            _storeItems.Add( "Booster2", new StoreItem( "Booster2", 5, 20, 2 ) );
        }

        public void GoToStoreMenu( int subMenuId = 0 )
        {
            if ( _gameState == GameState.LevelsMenu )
                _gameState = GameState.LevelsMenu_Store;
            else if ( _gameState == GameState.Paused )
                _gameState = GameState.Paused_Store;
            else if ( _gameState == GameState.Gameplay )
                _gameState = GameState.Gameplay_Store;
            else if ( _gameState == GameState.MainMenu )
                _gameState = GameState.MainMenu_Store;
            else if ( _gameState == GameState.GameOver )
                _gameState = GameState.GameOver_Store;

            _uiManager.DisplayStoreMenu( GetTotalCoins(), subMenuId );
        }
        void GoToStoreFromGameplay( int subMenuID )
        {
            PauseGame();
            _uiManager.HideGameplay();
            GoToStoreMenu( subMenuID );
        }
        public void GoBackFromStoreMenu()
        {
            if ( _gameState == GameState.Paused_Store )
            {
                _uiManager.DisplayPauseMenu();
                _gameState = GameState.Paused;
            }
            else if ( _gameState == GameState.Gameplay_Store )
            {
                if ( !IsGameSparksAuthenticated )
                    _database.SaveUserData( _activeUser.userData );

                _uiManager.DisplayGameplayMenu( _levelManager.LevelID, _activeUser.userData );
                StartCountDown( false );
            }
            else if ( _gameState == GameState.MainMenu_Store )
            {
                GoToMainMenu();
            }
            else if ( _gameState == GameState.LevelsMenu_Store )
            {
                GoToLevelsMenu();
            }
            else if ( _gameState == GameState.GameOver_Store )
            {
                _uiManager.ShowGameOverDialogAgain();
                _gameState = GameState.GameOver;
            }
        }


        public void BuyPackage( string id )
        {
            if ( IsGameSparksAuthenticated )
                BuyPackageGameSparks( id );
            else
                BuyPackageLocally( id );
        }

        void BuyPackageLocally( string id )
        {
            int subMenuID = 0;

            if ( _activeUser.userData.coins < _storeItems[id].price )
                _uiManager.DisplayNeedMoreCoinsMessage();
            else
            {
                _activeUser.userData.coins -= _storeItems[id].price;
                if ( id.StartsWith( "P" ) )
                    _activeUser.userData.painters[_storeItems[id].itemIndex] += _storeItems[id].count;
                else if ( id.StartsWith( "B" ) )
                {
                    subMenuID = 1;
                    _activeUser.userData.boosters[_storeItems[id].itemIndex] += _storeItems[id].count;
                }
                _database.SaveUserData( _activeUser.userData );
                GoToStoreMenu( subMenuID );
            }
        }
        void BuyPackageGameSparks( string id )
        {
            int subMenuID = 0;

            _uiManager.HideStoreMenu();
            
            _gameSparks.DownloadUserData( 
                ( gsData )=> {
                    if ( _activeUser.userData.coins < _storeItems[id].price )
                        _uiManager.DisplayNeedMoreCoinsMessage();
                    else
                    {
                        // Just a backup.
                        UserData tempData = new UserData( _activeUser.userData );

                        _activeUser.userData.coins -= _storeItems[id].price;
                        if ( id.StartsWith( "P" ) )
                        {
                            if ( _activeUser.userData.painters == null )
                                _activeUser.userData.painters = new int[5];

                            _activeUser.userData.painters[_storeItems[id].itemIndex] += _storeItems[id].count;
                        }
                        else if ( id.StartsWith( "B" ) )
                        {
                            if ( _activeUser.userData.boosters == null )
                                _activeUser.userData.boosters = new int[3];

                            subMenuID = 1;
                            _activeUser.userData.boosters[_storeItems[id].itemIndex] += _storeItems[id].count;
                        }
                     
                        SubmitUserCloudDataAfterBuyPackage( tempData, subMenuID );
                    }
                },
                ()=> {
                    _uiManager.FadeOutLoadingScreen( () => { GoToStoreMenu( subMenuID ); }, 0.1f );
                },
                _uiManager
            );
        }
        void SubmitUserCloudDataAfterBuyPackage( UserData tempData, int subMenuID )
        {
            _uiManager.DisplayLoadingScreen( "Submitting \nUser Cloud Data" );
            _gameSparks.UploadUserData( _activeUser, 
                ()=> {
                    _uiManager.FadeOutLoadingScreen( () =>
                    {
                        GoToStoreMenu( subMenuID );
                    }, 0.1f );
                },
                ()=> {
                    _uiManager.FadeOutLoadingScreen( () =>
                    {
                        _activeUser.userData = new UserData( tempData );
                        GoToStoreMenu( subMenuID );
                    }, 0.1f );
                },
                _uiManager 
            );
        }


        public void BuyCoinsPacakgeGameSparks( int id )
        {
            if ( !_inAppPurchase.IsInitialized )
            {
                _inAppPurchase.InitializePurchasing();
                _uiManager.HideStoreMenu();
                _uiManager.DisplayMessageDialog( "Error", 
                                                 "Could not process purchase, please try again later.",  
                                                 UIManager.MessageDialogAction.OK,
                                                 ()=> { GoToStoreMenu( 2 ); } );
                return;
            }

            if ( !IsGameSparksAuthenticated )
            {
                _uiManager.HideStoreMenu();
                _uiManager.DisplayMessageDialog( "GameSparks Sign In",
                                                 "Please sign in to your GameSparks account before any purchase porcess",
                                                 UIManager.MessageDialogAction.OK,
                                                 () => { GoToStoreMenu( 2 ); } );
                return;
            }

            _uiManager.HideStoreMenu();
            _uiManager.DisplayLoadingScreen( "Processing your order..." );
            _inAppPurchase.BuyProductID( id );
        }
        public void OnBuyCoinsSucceed( MANAProduct package )
        {
            _gameSparks.DownloadUserData( 
                ( gsData )=> {
                    UpdateActiveUserData( gsData );
                    SubmitUserCloudDataAfterPurchase( package );
                },
                ()=> { SubmitUserCloudDataAfterPurchase( package ); },
                _uiManager 
            );
        }
        void SubmitUserCloudDataAfterPurchase( MANAProduct package )
        {
            _uiManager.DisplayLoadingScreen( "Submitting \nUser Cloud Data" );
            // Give coins now
            _activeUser.userData.coins += package.count;
            _gameSparks.UploadUserData( _activeUser, 
                ()=> {
                    _uiManager.FadeOutLoadingScreen( () =>
                    {
                        OnSubmitUserCloudDataAfterPurchaseComplete( package );
                    }, 0.1f );
                },
                ()=> {
                    _uiManager.FadeOutLoadingScreen( () =>
                    {
                        // Save package
                        _database.SavePendingData( package.id );
                        GoToStoreMenu( 2 );
                    }, 0.1f );
                },
                _uiManager
            );
        }
        void OnSubmitUserCloudDataAfterPurchaseComplete( MANAProduct package )
        {
            // Display success message
            _uiManager.FadeOutLoadingScreen( ()=> {
                _uiManager.DisplayMessageDialog( "Congratulation!", 
                                                 "Your " + package.count + " Coins purchase is being processed",  
                                                 UIManager.MessageDialogAction.OK,
                                                 ()=> { GoToStoreMenu( 2 ); } );
            }, 0.1f );
        }

        public void OnBuyCoinsFailed()
        {
            // Display success message
            _uiManager.FadeOutLoadingScreen( ()=> {
                _uiManager.DisplayMessageDialog( "Error!", 
                                                 "Failed to process your purchase order, please try again later",  
                                                 UIManager.MessageDialogAction.OK,
                                                 ()=> { GoToStoreMenu( 2 ); } );
            }, 0.1f );
        }

        public int GetTotalCoins()
        {
            if ( !IsGameSparksAuthenticated && _activeUser.userData == null )
                _activeUser.userData = _database.LoadUserData();

            return _activeUser.userData == null ? 0 : _activeUser.userData.coins;
        }

        #endregion


        #region [MANA3DGames Services]

        void CheckNewGameUpdate()
        {
            string link = "http://www.mana3dgames.com/uploads/4/4/7/2/44722523/modey_version.txt";

            _uiManager.DisplayLoadingScreen( "Checking\nNew Game Update" );
            _mana3dServices.CheckBuildVersion( link, ( newUpdate ) => {
                if ( newUpdate )
                {
                    _uiManager.FadeOutLoadingScreen( ()=> {
                        _uiManager.DisplayMessageDialog( "Game Update", 
                                                         "Found a new game update. It's highly recommended to update. \nWould you like to update the game?", 
                                                         UIManager.MessageDialogAction.OK_Cancel, 
                                                         ()=> {
                                                             StartCoroutine( OnAcceptUpdateGame() );
                                                         }, 
                                                         ()=> {
                                                             //_fbManager.Init();
                                                             StartCoroutine( CheckGameSparksAvailability() );
                                                         } );
                    }, 0.1f );
                }
                else
                {
                    //_fbManager.Init();
                    StartCoroutine( CheckGameSparksAvailability() );
                }
            } );
        }

        IEnumerator OnAcceptUpdateGame()
        {
            yield return new WaitForSeconds( 1.0f );
            _uiManager.DisplayMessageDialog( "Game Update", "Please Restart The Game", UIManager.MessageDialogAction.OK, Application.Quit, null );
            Application.OpenURL( "market://details?id=" + Application.identifier );
        }

        public void OpenMANA3DGames()
        {
            _mana3dServices.OpenMANA3DGames();
        }

        public void OpenGooglePlayStore()
        {
            _mana3dServices.OpenGooglePlayStore();
        }

        #endregion
        

        #region [GameSparks]

        IEnumerator CheckGameSparksAvailability()
        {
            int frameCount = 0;
            _uiManager.DisplayLoadingScreen( "Connecting to GameSparks" );
            while ( !_gameSparks.ReceivedConnectionStatus )
            {
                yield return new WaitForEndOfFrame();
                frameCount++;
                if ( frameCount >= 500 )
                    break;
            }

            if ( !_gameSparks.IsAvailable )
                StartGameWithLocalUser();
            else
                StartGameSparksAuthenticationProcess();
        }
        void StartGameSparksAuthenticationProcess()
        {
            // Check if already registered/signed in user with FB account.
            if ( _database.CheckGameSparksWithFacebookSignedInUserInfo() )
            {
                // auto sign in user.
                AuthenticateGameSparksWithFacebook();
                return;
            }

            // Or check if already registered/signed in user with GS account.
            string[] gameSparksInfo = _database.GetGameSparksSignedInUserInfo();
            if ( gameSparksInfo != null )
            {
                // auto sign in user.
                AuthenticateGameSparks( gameSparksInfo[0], gameSparksInfo[1] );
                return;
            }

            // Display Sign In Dialog.
            _uiManager.FadeOutLoadingScreen( _gameSparksSignInMenu.Display, 0.1f ); 
        }

        public void OnCancelGameSparksAuthentication()
        {
            if ( _gameState == GameState.Startup )
                OnCancelGameSparksAuthenticationAtStartup();
            else if ( _gameState == GameState.MainMenu_GameSpraks )
                GoToGameSparksMenu();

        }
        void        OnCancelGameSparksAuthenticationAtStartup()
        {
            var count = PlayerPrefs.GetInt( "GPGsMsgDisplayedCount" );
            if ( PlayerPrefs.GetInt( "GPGsMsgDisplayed" ) == 1 &&
                 count < 3 )
            {
                _uiManager.FadeOutLoadingScreen( ()=> {
                    // Failed to check Game data file.
                    _uiManager.DisplayMessageDialog( "Failed to Sign In",  
                                                     "It's highly recommended to sign in/up to your Game Sparks Account to have the best game experience.\nTry Again?", 
                                                     UIManager.MessageDialogAction.OK_Cancel, 
                                                     _gameSparksSignInMenu.Display,
                                                     StartGameWithLocalUser );
                }, 
                0.1f );

                count++;
                PlayerPrefs.SetInt( "GPGsMsgDisplayedCount", count );
            }
            else
            {
                PlayerPrefs.SetInt( "GPGsMsgDisplayed", 1 );
                StartGameWithLocalUser();
            }
        }

        public void AuthenticateGameSparks( string userName, string password )
        {
            if ( IsGameSparksAuthenticated )
                Debug.Log( "GameSparks is authenticated already!!!" );

            _uiManager.DisplayLoadingScreen( "Signing In to GameSparks Account" );
            _gameSparks.SignIn( userName, password, 
                                ()=> {
                                    OnGameSparksAuthenticationSucceed();
                                    _database.OnGameSparksSignOut();
                                    _database.SetGameSparksSignedInUserInfo( userName, password );
                                }, 
                                _gameSparksSignInMenu.Display, _uiManager, _gameSparksSignInMenu );
        }
        public void AuthenticateGameSparksWithFacebook()
        {
            if ( IsGameSparksAuthenticated )
                Debug.Log( "GameSparks is authenticated already!!!" );

            _uiManager.DisplayLoadingScreen( "Signing In to GameSparks With Facebook" );
            _gameSparks.SignInWithFB(   ()=> {
                                            OnGameSparksAuthenticationSucceed();
                                            _database.OnGameSparksSignOut();
                                            _database.SetGameSparksWithFacebookSignedInUserInfo( _gameSparks.DisplayName );
                                        }, 
                                        _gameSparksSignInMenu.Display, _uiManager, _gameSparksSignInMenu );
        }
        public void OnGameSparksAuthenticationSucceed()
        {
            _gameSparks.DownloadUserData( 
                ( gsData ) => {
                    OnGameSparksDownloadUserDataSucceed( gsData );
                }, 
                null, _uiManager 
            );
        }
        public void OnGameSparksAuthenticationFailed( string msg, bool showMsgDialog )
        {
            _uiManager.FadeOutLoadingScreen( ()=> {
                if ( showMsgDialog )
                {
                    _uiManager.DisplayMessageDialog( "Failed to Sign In", 
                                                     msg, 
                                                     UIManager.MessageDialogAction.OK, 
                                                     _gameSparksSignInMenu.Display );
                }
                else
                {
                    _gameSparksSignInMenu.Display();
                    _gameSparksSignInMenu.ShowErrorMsg( msg );
                }

            }, 0.1f );
        }


        public void CreateNewGameSparksAccount( string displayName, string userName, string password )
        {
            _gameSparks.Register( displayName, userName, password, 
                                  ()=> {
                                      AuthenticateGameSparks( userName, password );
                                  }, 
                                  _gameSparksSignInMenu.Display, 
                                  _uiManager, _gameSparksCreateNewAccount );
        }

        void UpdateActiveUserData( GSData gsData )
        {
            if ( _activeUser == null )
                _activeUser = new ActiveUser();

            if ( gsData.GetGSData( "playerState" ) != null &&
                 !gsData.GetGSData( "playerState" ).ContainsKey( "userData" ) )
            {
                Debug.Log( "ERRORORORORR : Null userData!!!!!" );
                _gameSparks.InitiateUserDataWithDefaultValues( out _activeUser );
            }
            else
            {
                _activeUser.userData = JsonUtility.FromJson<UserData>( gsData.GetGSData( "playerState" ).GetString( "userData" ).ToString() );
                _activeUser.landsData = JsonUtility.FromJson<CloudLandsData>( gsData.GetGSData( "playerState" ).GetString( "landsData" ).ToString() );
            }

            if ( _activeUser.userData.boosters.Length == 0 )
                _activeUser.userData.boosters = new int[3];
            if ( _activeUser.userData.painters.Length == 0 )
                _activeUser.userData.painters = new int[5];

            if ( _activeUser.landsData.lands[0].levels[0].unlocked == 0 )
                _activeUser.landsData.UnlockLevel( 0, 0 );
        }


        public void OnGameSparksDownloadUserDataSucceed( GSData gsData )
        {
            UpdateActiveUserData( gsData );
            CheckPendingUserCloudData();
        }

        void CheckPendingUserCloudData()
        {
            var id = _database.CheckPendingData();
            if ( string.IsNullOrEmpty( id ) || id == "none" )
            {
                // There is no pending data.
                StartGameWithGameSparksUser();
                return;
            }

            MANAProduct product = _inAppPurchase.GetProduct( id );
            if ( product != null )
            {
                _activeUser.userData.coins += product.count;
                _uiManager.DisplayLoadingScreen( "Submitting \nUser Cloud Data" );
                _gameSparks.UploadUserData( _activeUser, 
                    () =>
                    {
                        _database.RemovePendingData();
                        StartGameWithGameSparksUser();
                    },
                    () =>
                    {
                        StartGameWithGameSparksUser();
                    },
                    _uiManager 
                );
            }
            else
                StartGameWithGameSparksUser();
        }

        void StartGameWithGameSparksUser()
        {
            _uiManager.FadeOutLoadingScreen( () =>
            {
                SetGameSparksUser();
                if ( _gameState == GameState.Startup )
                    StartGame();
                else if ( _gameState == GameState.MainMenu_GameSpraks )
                    GoToGameSparksMenu();
            },
            0.1f );
        }

        void SetGameSparksUser()
        {
            _uiManager.SetUserProfileBtn_Name( _gameSparks.DisplayName );
            // CreateAndSetUserAvatar ?????
            _uiManager.DisplayMainMenuUserProfileBtn();
        }


        public void GoToGameSparksMenu()
        {
            _gameState = GameState.MainMenu_GameSpraks;
            _uiManager.DisplayGameSparksMenu();
        }

        public void GoToGameSparksSignInMenu()
        {
            _gameSparksSignInMenu.Display();
        }
        public void GoToGameSparksCreateNewUserMenu()
        {
            _gameSparksCreateNewAccount.Display();
        }
        public void GoToGameSparksChangePasswordMenu()
        {
            _gameSparksChangePasswordDialog.Display();
        }

        // Change it to onCompleteAction.
        void SubmitUserData( LevelData data )
        {
            _uiManager.HideGameplay();
            _uiManager.DisplayLoadingScreen( "Saving Game Data" );

            // Get the current land.
            var land = _activeUser.landsData.lands[data.landID];
            if ( land == null )
                land = new LandData();

            if ( land.levels == null )
                land.levels = new LevelData[LevelManager.LEVEL_COUNT];

            // Get current level.
            var lvl = land.levels[data.levelID];

            // Save current level score and stars count temporarily.
            int oldScore = lvl.score;
            int oldStars = lvl.stars;

            lvl.CopyValues( data );

            if ( lvl.score < oldScore )
                lvl.score = oldScore;
            
            if ( lvl.stars < oldStars )
                lvl.stars = oldStars;

            // Check if win level.
            if ( data.stars > 0 )
            {
                // Unlock next level/land.
                if ( data.levelID + 1 < LevelManager.LEVEL_COUNT )
                    _activeUser.landsData.UnlockLevel( data.landID, data.levelID + 1 );
                else if ( data.landID + 1 < LevelManager.LAND_COUNT )
                    _activeUser.landsData.UnlockLevel( data.landID + 1, 0 );

                _activeUser.tempScore = _activeUser.landsData.CalculateTotalScore();

                // Sumbit score
                _gameSparks.SubmitScore( _activeUser.tempScore );
            }

            if ( data.coinsReward > 0 )
            {
                _activeUser.userData.coins += data.coinsReward;

                if ( data.bonusRewardID >= 0 )
                    _activeUser.userData.boosters[data.bonusRewardID]++;
            }

            _gameSparks.UploadUserData( _activeUser, 
                () => {
                    _uiManager.FadeOutLoadingScreen( () =>
                    {
                        _uiManager.DisplayGameOverDialog( data, _activeUser.tempScore );
                        _gameState = GameState.GameOver;
                    }, 0.1f );
                },
                () => {
                    _uiManager.FadeOutLoadingScreen( () =>
                    {
                        _uiManager.DisplayGameOverDialog( data, _activeUser.tempScore );
                        _gameState = GameState.GameOver;
                    }, 0.1f );
                },
                _uiManager
            );
        }


        public void GoToGameSparksLeaderBoard()
        {
            _gameSparks.RetrieveScoreLeaderboard( 50, 
                                                  ( response )=> {
                                                        switch ( _gameState )
                                                        {
                                                            case GameState.MainMenu_GameSpraks:
                                                              _gameState = GameState.MainMenu_GameSpraks_Leaderboard;
                                                                break;
                                                            case GameState.LevelsMenu:
                                                              _gameState = GameState.LevelsMenu_Leaderboard;
                                                                break;
                                                            case GameState.GameOver:
                                                              _gameState = GameState.GameOver_Leaderboard;
                                                                break;
                                                        }
                                                        _gameSparksLeaderboardMenu.Display( response );
                                                  }, 
                                                  GoBackFromGameSparksLeaderboard, 
                                                  _uiManager );
        }
        public void GoBackFromGameSparksLeaderboard()
        {
            switch ( _gameState )
            {
                case GameState.MainMenu_GameSpraks:
                case GameState.MainMenu_GameSpraks_Leaderboard:
                    GoToGameSparksMenu();
                    break;
                case GameState.LevelsMenu:
                case GameState.LevelsMenu_Leaderboard:
                    GoToLevelsMenu();
                    break;
                case GameState.GameOver:
                case GameState.GameOver_Leaderboard:
                    _uiManager.ShowGameOverDialogAgain();
                    break;
            }
        }


        public void SignOutFromGameSparksAccount()
        {
            _gameSparks.SignOut();
            _database.OnGameSparksSignOut();
            SetGuestUser();
            GoToGameSparksMenu();
        }


        public void ChangeGameSparksPassword( string oldPassword, string newPassword )
        {
            _gameSparks.ChangePassword( oldPassword, newPassword,
                                        GoToGameSparksMenu,
                                        GoToGameSparksChangePasswordMenu, 
                                        _uiManager, _gameSparksChangePasswordDialog );
        }

        #endregion
        

        #region [InGame Level Builder]

        public void UploadJSONToGameSparks( string json )
        {
            if ( IsGameSparksAuthenticated )
            {
                // 1. Submit to GameSparks
                _gameSparks.UploadLevelJSON( json, ( idStr ) =>
                {
                    // 2. Get the JSON ID
                    if ( !string.IsNullOrEmpty( idStr ) )
                    {
                        // 3. Send AppRequest to Friend with data (JSON ID)
                        _uiManager.DisplayLoadingScreen( "Sending Level to Friend(s)\nPlease Wait..." );
                        _fbManager.SendLevelToFriends( idStr );
                    }
                    else
                    {
                        OnFailedInviteFriendsToLevelChallenage();
                    }
                } );
            }
            else
            {
                OnFailedInviteFriendsToLevelChallenage();
            }
        }

        public void OnFailedInviteFriendsToLevelChallenage()
        {
            _uiManager.FadeOutLoadingScreen( ()=> {
                _uiManager.DisplayMessageDialog( "Server Error", 
                                                 "Failed to upload level \nPlease try again later", 
                                                 UIManager.MessageDialogAction.OK,
                                                 ()=> { GoToInGameLevelBuilderEditor(); } );
            }, 0.1f );
        }

        public void OnInviteFriendToLevelChallenageSuccessed()
        {
            _uiManager.FadeOutLoadingScreen( ()=> { GoToInGameLevelBuilderEditor(); }, 0.1f );
        }

        public void OnCheckPendingChallengesComplete( List<object> invitations )
        {
            _uiManager.FadeOutLoadingScreen( ()=> {
                _gameState = GameState.MainMenu_FB_PendingInvitations;
                _pendingInvitationMenu.Display( invitations );
            }, 0.1f );
        }
        public void OnFailedToCheckPendingChallenges()
        {
            _uiManager.FadeOutLoadingScreen( GoToFacebookMenu, 0.1f );
        }


        public void DownloadJSONToGameSparks( string senderName, string id )
        {
            if ( IsGameSparksAuthenticated )
            {
                _gameSparks.DownloadLevelJSON( id, ( levelJosn ) =>
                {
                    if ( !string.IsNullOrEmpty( levelJosn ) )
                    {
                        _pendingInvitationMenu.Close();
                        StartLevelChallenage( senderName, levelJosn );
                    }
                    else
                    {
                        OnFailedToLoadLevelChallenage();
                    }
                } );
            }
            else
            {
                OnFailedToLoadLevelChallenage();
            }
        }

        

        void OnFailedToLoadLevelChallenage()
        {
            _uiManager.FadeOutLoadingScreen( ()=> {
                _uiManager.DisplayMessageDialog( "Server Error", 
                                                 "Failed to download level \nPlease try again later", 
                                                 UIManager.MessageDialogAction.OK,
                                                 _pendingInvitationMenu.ShowAgain );
            }, 0.1f );
        }

        public GameObject GetTitleGOFBPendingInvitationMenu()
        {
            return _pendingInvitationMenu.GetTitleGameObject();
        }

        #endregion


        #region [Camera]

        public void ResetCamera()
        {
            _cam.orthographicSize = 6.4f;
            _cam.transform.position = new Vector3( 0, 0, -10 );
        }

        public void ZoomInCamera( Vector2 pos, float time = 0.5f )
        {
            _uiManager.GetMenuGO( "BlackLayer" ).SetActive( true );
            _uiManager.ResetCurrentSpriteColor();
            _uiManager.NullCurrentSprite();
            _inputManager.LockButtons( true );
            pos.x = Mathf.Clamp( pos.x, -2.75f, 2.75f );
            Action<float> action = ( val )=> { _cam.orthographicSize = val; };
            LeanTween.value( _cam.gameObject, action, 6.4f, 1.5f, time );
            LeanTween.move( _cam.gameObject, pos, time );
        }

        public void ZoomOutCamera( float time = 0.5f )
        {
            _uiManager.GetMenuGO( "BlackLayer" ).SetActive( false );
            Action<float> action = ( val )=> { _cam.orthographicSize = val; };
            LeanTween.value( _cam.gameObject, action, 1.5f, 6.4f, time );
            var tween = LeanTween.move( _cam.gameObject, Vector2.zero, time );
            tween.setOnComplete( ()=> { _inputManager.LockButtons( false ); } );
        }

        void FeeFaceEffect()
        {
            _inputManager.LockButtons( true );
        }

        #endregion
    }
}