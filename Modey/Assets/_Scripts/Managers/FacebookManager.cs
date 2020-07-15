using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facebook.Unity;
using Facebook.MiniJSON;
using MANA3DGames.Utilities.Coroutine;

namespace MANA3DGames
{
    public class FacebookManager
    {
        GameManager _manager;

        bool _callLoginAgain;
        bool _isCheckingPendingInvitationByRequest;
        Action _onLoginAction;

        public bool IsInitialized { get { return FB.IsInitialized; } }
        public bool IsLoggedIn { get { return FB.IsLoggedIn; } }

        CoroutineTask _checkInvitationTask;



        public FacebookManager( GameManager manager )
        {
            _manager = manager;
        }

        public void Init()
        {
            FB.Init( OnInitComplete, OnHideUnity );
        }
        void OnInitComplete()
        {
            //_manager.OnInitFacebook();
            if ( _callLoginAgain )
                Login( _onLoginAction );
        }


        public void Login( Action onLoginAction = null )
        {
            _callLoginAgain = false;
            if ( FB.IsInitialized )
            {
                _onLoginAction = onLoginAction;
                FB.LogInWithPublishPermissions( new List<string>() { "publish_actions" }, OnLogInComplete );
            }
            else
            {
                _callLoginAgain = true;
                Init();
            }
        }
        void OnLogInComplete( IResult result )
        {
            if ( result == null )
            {
                _manager.OnFacebookFailedToLogIn();
                return;
            }

            if ( !string.IsNullOrEmpty( result.Error ) )
            {
                _manager.OnFacebookFailedToLogIn();
            }
            else if ( result.Cancelled )
            {
                _manager.OnFacebookFailedToLogIn();
            }
            else if ( !string.IsNullOrEmpty( result.RawResult ) )
            {
                _manager.OnFacebookLogIn( _onLoginAction );
                
                StopCheckNewInvitationCoroutine();
                _checkInvitationTask = new CoroutineTask( CheckPendingInvitationsCoroutine() );
            }
            else
            {
                _manager.OnFacebookFailedToLogIn();
            }
        }

        public void LogOut()
        {
            if ( FB.IsInitialized && FB.IsLoggedIn )
            {
                StopCheckNewInvitationCoroutine();
                FB.LogOut();
            }
        }


        public void ShareScore( int score )
        {
            if ( FB.IsLoggedIn )
                FB.ShareLink( new System.Uri( "http://www.mana3dgames.com/modey.html" ), 
                              "MODEY", "My New Score " + score + " Points!", 
                              new System.Uri( "http://www.mana3dgames.com/uploads/4/4/7/2/44722523/modey-icon-128_orig.png" ),
                              HandleOnShareScore );
            else
                Login();
        }
        void HandleOnShareScore( IResult result )
        {
            if ( result == null ||
                 !string.IsNullOrEmpty( result.Error ) ||
                 result.Cancelled )
            {
                _manager.OnFaildToShareScore();
                return;
            }

            if ( !string.IsNullOrEmpty( result.RawResult ) )
            {
                _manager.OnShareScoreComplete();
            }
            else
            {
                _manager.OnFaildToShareScore();
            }
        }


        public void ShareWinChallengeLevel( string fromName )
        {
            if ( FB.IsLoggedIn )
                FB.ShareLink( new System.Uri( "http://www.mana3dgames.com/dead-space-battle.html" ), 
                              "MODEY", fromName + " I beat your level!", 
                              new System.Uri( "http://www.mana3dgames.com/uploads/4/4/7/2/44722523/icon-128_orig.png" ),
                              OnShareWinChallengeLevelComplete );
            else
                Login();
        }
        void OnShareWinChallengeLevelComplete( IResult result )
        {

        }

        public void InviteFriends()
        {
            if ( FB.IsLoggedIn )
            {
                FB.Mobile.AppInvite( new System.Uri( "market://details?id=" + Application.identifier ), 
                                     null,
                                     OnInviteFriendsComplete );
            }
            else
                Login();
        }
        void OnInviteFriendsComplete( IResult result )
        {
        }


        public void SendLevelToFriends( string levelID )
        {
            // "app_non_users" }, 
            if ( FB.IsLoggedIn )
            {
                FB.AppRequest( "I dare you to beat this level!", 
                                null, null, null, 0, 
                                levelID, 
                                "Custom Level Challenge!", 
                                OnSendLevelToFriendsComplete );
            }
            else
                Login();
        }
        void OnSendLevelToFriendsComplete( IResult result )
        {
            if ( result == null ||
                 !string.IsNullOrEmpty( result.Error ) ||
                 result.Cancelled )
            {
                _manager.OnFailedInviteFriendsToLevelChallenage();
                return;
            }

            if ( !string.IsNullOrEmpty( result.RawResult ) )
            {
                _manager.OnInviteFriendToLevelChallenageSuccessed();
            }
            else
            {
                _manager.OnFailedInviteFriendsToLevelChallenage();
            }
        }


        void StopCheckNewInvitationCoroutine()
        {
            if ( _checkInvitationTask != null )
            {
                _checkInvitationTask.kill();
                _checkInvitationTask = null;
            }
        }

        IEnumerator CheckPendingInvitationsCoroutine()
        {
            yield return new WaitForSeconds( 20 );
            CheckPendingChallenge( false );
        }

        public void CheckPendingChallenge( bool isByRequest )
        {
            if ( _isCheckingPendingInvitationByRequest )
                return;

            _isCheckingPendingInvitationByRequest = isByRequest;

            FB.API( "/me/apprequests", HttpMethod.GET, OnCheckPendingChallengesComplete );
        }
        public void OnCheckPendingChallengesComplete( IGraphResult result )
        {
            if ( result.Error == null )
            {
                // Grab all requests in the form of  a dictionary.
                Dictionary<string, object> reqResult = Json.Deserialize( result.RawResult ) as Dictionary<string, object>;
                List<object> invitations = reqResult["data"] as List<object>;

                if ( _isCheckingPendingInvitationByRequest )
                {
                    // Grab 'data' and put it in a list of objects.
                    _manager.OnCheckPendingChallengesComplete( invitations );
                }
                else
                {
                    CheckNewInvitation( invitations );
                }
            }
            else
            {
                //_manager.testText.gameObject.SetActive( true );
                //_manager.testText.text += "\nData: Error-> " + result.Error;
                _manager.OnFailedToCheckPendingChallenges();
            }

            _isCheckingPendingInvitationByRequest = false;

            new CoroutineTask( CheckPendingInvitationsCoroutine() );
        }

        void CheckNewInvitation( List<object> invitations )
        {
            if ( invitations == null || invitations.Count == 0 )
                return;

            Dictionary<string, object> reqConvert = invitations[0] as Dictionary<string, object>;
            var date = reqConvert["created_time"] as string;
            if ( PlayerPrefs.GetString( "LastInvitationDate" ) != date )
            {
                PlayerPrefs.SetString( "LastInvitationDate", date );

                Dictionary<string, object> fromString = reqConvert["from"] as Dictionary<string, object>;
                string fromID = fromString["id"] as string;
                string fromName = fromString["name"] as string;

                GetFrinedProfilePicture( fromID, ( sprite )=> {
                    _manager.ShowNewInvitationNotification( fromName, sprite );
                    Handheld.Vibrate();
                } );
            }
        }


        public void GetFrinedProfilePicture( string id, Action<Sprite> onComplete )
        {
            FB.API( "https" + "://graph.facebook.com/" + id + "/picture?type=large", 
                    HttpMethod.GET, 
                    delegate( IGraphResult result )
                    {
                        Sprite sprite = Sprite.Create( result.Texture, 
                                                       new Rect( 0, 0, result.Texture.width , result.Texture.height ), 
                                                       new Vector2( 0.5f, 0.5f ), 
                                                       200 );
                        onComplete( sprite );
                    });
        }

        void OnHideUnity( bool isGameShown )
        {
        }
    }
}
