using UnityEngine;
using MANA3DGames.Utilities.Security;

namespace MANA3DGames
{
    public class LocalGameDatabase
    {
        PlayerPrefs _prefs;


        public LocalGameDatabase()
        {
            PlayerPrefsExtensions.SetKeys_test();
            _prefs = new PlayerPrefs();
            _prefs.Initialize();
        }


        public void SaveGameSettings( GameSettings settings )
        {
            _prefs.SetSecure( "Settings_SFX", settings.sfx ? 1 : 0 );
            _prefs.SetSecure( "Settings_BGM", settings.bgm ? 1 : 0 );
            _prefs.SetSecure( "Settings_StartupTimer", settings.startupTimer ? 1 : 0 );
            _prefs.SetSecure( "Settings_FB_LogInStartup", settings.loginFB ? 1 : 0 );
        }
        public GameSettings LoadGameSettings()
        {
            GameSettings settings = new GameSettings();
            settings.sfx = _prefs.GetSecureInt( "Settings_SFX" ) == 1;
            settings.bgm = _prefs.GetSecureInt( "Settings_BGM" ) == 1;
            settings.startupTimer = _prefs.GetSecureInt( "Settings_StartupTimer" ) == 1;
            settings.loginFB = _prefs.GetSecureInt( "Settings_FB_LogInStartup" ) == 1;
            return settings;
        }


        public void SaveLevelData( LevelData data )
        {
            string unLockedKey  = "land_" + data.landID + "_lvl_" + data.levelID + "_unlocked";
            string starsKey     = "land_" + data.landID + "_lvl_" + data.levelID + "_stars";
            string scoreKey     = "land_" + data.landID + "_lvl_" + data.levelID + "_score";

            _prefs.SetSecure( unLockedKey, data.unlocked );

            if ( _prefs.GetSecureInt( starsKey ) < data.stars )
                _prefs.SetSecure( starsKey, data.stars );

            if ( _prefs.GetSecureInt( scoreKey ) < data.score )
                _prefs.SetSecure( scoreKey, data.score );


            if ( data.coinsReward > 0 )
            {
                UserData userData = LoadUserData();
                userData.coins += data.coinsReward;

                if ( data.bonusRewardID >= 0 )
                    userData.boosters[data.bonusRewardID]++;

                SaveUserData( userData );
            }
        }
        public LevelData LoadLevelData( int landID, int levelID )
        {
            LevelData data = new LevelData();
            data.levelID = levelID;
            data.unlocked = _prefs.GetSecureInt( "land_" + landID + "_lvl_" + levelID + "_unlocked" );
            data.stars = _prefs.GetSecureInt( "land_" + landID + "_lvl_" + levelID + "_stars" );
            data.score = _prefs.GetSecureInt( "land_" + landID + "_lvl_" + levelID + "_score" );
            return data;
        }

        public void UnlockLevel( int landID, int levelID )
        {
            if ( _prefs.GetSecureInt( "land_" + landID + "_lvl_" + levelID + "_unlocked" ) > 0 ) 
                return;

            LevelData data = new LevelData();
            data.landID = landID;
            data.levelID = levelID;
            data.score = 0;
            data.stars = 0;
            data.unlocked = 1;
            SaveLevelData( data );
        }


        public void SaveUserData( UserData data )
        {
            _prefs.SetSecure( "UserCoins", data.coins );
            for ( int i = 0; i < data.boosters.Length; i++ )
            {
                var temp = i;
                _prefs.SetSecure( "UserBoosters_" + temp, data.boosters[temp] );
            }

            for ( int i = 0; i < data.painters.Length; i++ )
            {
                var temp = i;
                _prefs.SetSecure( "UserColorBlocks_" + temp, data.painters[temp] );
            }
        }
        public UserData LoadUserData()
        {
            UserData data = new UserData();

            data.coins = _prefs.GetSecureInt( "UserCoins" );
            data.boosters = new int[3];
            for ( int i = 0; i < data.boosters.Length; i++ )
            {
                var temp = i;
                data.boosters[temp] = _prefs.GetSecureInt( "UserBoosters_" + temp );
            }

            data.painters = new int[5];
            for ( int i = 0; i < data.painters.Length; i++ )
            {
                var temp = i;
                data.painters[temp] = _prefs.GetSecureInt( "UserColorBlocks_" + temp );
            }

            return data;
        }


        public int CalculateTotalScore()
        {
            int totalScore = 0;
            for ( int land = 0; land < LevelManager.LAND_COUNT; land++ )
            {
                for ( int lvl = 0; lvl < LevelManager.LEVEL_COUNT; lvl++ )
                    totalScore += _prefs.GetSecureInt( "land_" + land + "_lvl_" + lvl + "_score" );
            }
            return totalScore;
        }

        public int GetCurrentLand()
        {
            for ( int i = LevelManager.LAND_COUNT - 1; i >= 0; i-- )
            {
                if ( LoadLevelData( i, 0 ).unlocked >= 1 )
                    return i;
            }
            return 0;
        }

        public int GetCurrentLevel( int landID )
        {
            for ( int lvl = 0; lvl < LevelManager.LEVEL_COUNT; lvl++ )
            {
                if ( LoadLevelData( landID, lvl ).unlocked < 1 )
                    return lvl-1;
            }
            return 29;
        }

        public int[] GetStarsCount()
        {
            int[] stars = new int[3];
            for ( int land = 0; land < LevelManager.LAND_COUNT; land++ )
            {
                for ( int lvl = 0; lvl < LevelManager.LEVEL_COUNT; lvl++ )
                {
                    string starsKey = "land_" + land + "_lvl_" + lvl + "_stars";
                    int starCount = _prefs.GetSecureInt( starsKey );
                    if ( starCount > 0 )
                        stars[starCount-1]++;
                    else
                        break;
                }
            }
            return stars;
        }


        public bool IsFirstTime()
        {
            return !_prefs.HasSecureKey( "NotFirstTime" );
        }

        public void ApplyFirstTimeAction()
        {
            ApplyDefaultSettings();
            UnlockLevel( 0, 0 );
            _prefs.SetSecure( "NotFirstTime", 1 );
        }

        public void ApplyDefaultSettings()
        {
            GameSettings settings = new GameSettings();
            settings.sfx = settings.bgm = settings.startupTimer = true;
            settings.loginFB = false;
            SaveGameSettings( settings );
        }


        public void SavePendingData( string id )
        {
            _prefs.SetSecure( "PurchaseProcessingResult.Pending", id );
        }
        public string CheckPendingData()
        {
            if ( _prefs.HasSecureKey( "PurchaseProcessingResult.Pending" ) )
               return _prefs.GetSecureString( "PurchaseProcessingResult.Pending" );
            else
                return string.Empty;
        }
        public void RemovePendingData()
        {
            _prefs.SetSecure( "PurchaseProcessingResult.Pending", "none" );
        }



        public void SetGameSparksWithFacebookSignedInUserInfo( string userName )
        {
            _prefs.SetSecure( "FacebookSignedInUserWasRegistered", userName );
        }
        public bool CheckGameSparksWithFacebookSignedInUserInfo()
        {
            return _prefs.GetSecureString( "FacebookSignedInUserWasRegistered" ) != string.Empty;
        }

        public void SetGameSparksSignedInUserInfo( string userName, string password )
        {
            _prefs.SetSecure( "GameSparksUserName", userName );
            _prefs.SetSecure( "GameSparksUserPassword", password );
        }
        public string[] GetGameSparksSignedInUserInfo()
        {
            var userName = _prefs.GetSecureString( "GameSparksUserName" );
            var password = _prefs.GetSecureString( "GameSparksUserPassword" );

            if ( string.IsNullOrEmpty( userName ) || string.IsNullOrEmpty( password ) )
                return null;

            return new string[] { userName, password };
        }

        public void OnGameSparksSignOut()
        {
            _prefs.SetSecure( "GameSparksUserName", "" );
            _prefs.SetSecure( "GameSparksUserPassword", "" );
            _prefs.SetSecure( "FacebookSignedInUserWasRegistered", "" );
        }
    }
}
