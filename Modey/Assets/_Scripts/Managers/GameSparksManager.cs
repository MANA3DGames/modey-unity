using System;
using System.Collections;
using System.Collections.Generic;
using GameSparks.Core;
using GameSparks.Api.Requests;
using GameSparks.Api.Responses;
using GameSparks.Api.Messages;
using MANA3DGames.Utilities.Coroutine;
using UnityEngine;
using Facebook.Unity;

namespace MANA3DGames
{
    public class GameSparksManager 
    {
        #region [Variables]

        Action<string> _onUploadCompleteAction;

        CoroutineTask _authenticationTask;

        int _timeOutFrameCount;

        bool _receivedConnectionStatus;
        public bool ReceivedConnectionStatus { get { return _receivedConnectionStatus; } }
        public bool IsAvailable { get { return GS.Available; } }
        public bool IsModeyAuthenticated { get { return _response != null; } }

        AuthenticationResponse _response;
        public string DisplayName { get { return _response.DisplayName; } }

        #endregion


        #region [Constructor]

        public GameSparksManager()
	    {
		    GS.GameSparksAvailable += OnGameSparksAvailable;
            GS.GameSparksAuthenticated += OnGameSparksAuthenticated;
            UploadCompleteMessage.Listener += GetUploadMessage;
	    }

        public void EndSession()
        {
            new EndSessionRequest()
                .Send( ( response ) =>
                {
                    GSData scriptData = response.ScriptData;
                    long? sessionDuration = response.SessionDuration;
                } );
            GS.Reset();
            _response = null;
        }

        public void Disconnect()
        {
            GS.Disconnect();
        }

        #endregion


        #region [Event Listeners]

        void OnGameSparksAvailable( bool isConnected )
        {
            _receivedConnectionStatus = true;
        }

        void OnGameSparksAuthenticated( string str )
        {
            //Debug.Log( str );
        }

        #endregion


        #region [Authentication]

        public void SignIn( string userName, string password,
                            Action onCompleteAction, Action onFailedAction,
                            UIManager uiManager, GameSparksSignInMenu signInMenu )
        {
            new AuthenticationRequest()
                .SetUserName( userName )//set the username for login
                .SetPassword( password )//set the password for login
                .Send( ( auth_response )=> { //send the authentication request
                    if ( !auth_response.HasErrors )
                    { 
                        // for the next part, check to see if we have any errors i.e. Authentication failed
                        _response = auth_response;
                        onCompleteAction();
                    }
                    else
                    {
                        var details = auth_response.Errors.GetString( "DETAILS" );

                        Action tryAgainAction = ()=> { SignIn( userName, password, onCompleteAction, onFailedAction, uiManager, signInMenu ); };

                        // if we get this error it means we are not registered, so let's register the user instead
                        if ( details == "UNRECOGNISED" )
                        { 
                            OnGameSparksProcessFailed( "", "Wrong user name or password", false, 
                                                       tryAgainAction, 
                                                       onFailedAction, 
                                                       uiManager, signInMenu );
                        }
                        else if ( details == "LOCKED" )
                        {
                            OnGameSparksProcessFailed( 
                                "Sign In Error",
                                "There have been too many failed login attempts with these details and the account has been locked temporarily, try again?", 
                                true,
                                tryAgainAction, 
                                onFailedAction, 
                                uiManager, signInMenu );
                        }
                        else
                        {
                            Debug.Log( "Errors: " + auth_response.Errors.JSON );
                            string error = "Sign In Error!";
                            if ( auth_response.Errors.JSON.Contains( "timeout" ) )
                                error += ", try again later";
                            OnGameSparksProcessFailed( "", error, false, tryAgainAction, onFailedAction, uiManager, signInMenu );
                        }
                    }
            } );
        }

        public void SignInWithFB( Action onCompleteAction, Action onFailedAction,
                                  UIManager uiManager, GameSparksSignInMenu signInMenu )
        {
            Action tryAgainAction = ()=> { SignInWithFB( onCompleteAction, onFailedAction, uiManager, signInMenu ); };
            if ( FB.IsInitialized )
            {
                FB.ActivateApp();
                var perms = new List<string>(){ "public_profile", "email", "user_friends" };
                FB.LogInWithReadPermissions( perms,( result ) => {
                    if ( FB.IsLoggedIn )
                    {
                        new FacebookConnectRequest()
                            .SetAccessToken( AccessToken.CurrentAccessToken.TokenString )
                            .SetDoNotLinkToCurrentPlayer( false )// we don't want to create a new account so link to the player that is currently logged in
                            .SetSwitchIfPossible( true )//this will switch to the player with this FB account id they already have an account from a separate login
                            .Send( ( fbauth_response ) => {
                                if ( !fbauth_response.HasErrors )
                                {
                                    _response = fbauth_response;
                                    onCompleteAction();
                                }
                                else
                                {
                                    Debug.Log( fbauth_response.Errors.JSON ); //if we have errors, print them out
                                    OnGameSparksProcessFailed( "", "Sign In Error!", false, tryAgainAction, onFailedAction, uiManager, signInMenu );
                                }
                        } );
                    }
                    else
                    {
                        Debug.Log( "Facebook Login Failed:" + result.Error );
                        OnGameSparksProcessFailed( "Sign In Error",
                                "Sign Into your facebook account, try again?", 
                                true, tryAgainAction, onFailedAction ,uiManager, signInMenu );
                    }
                } );// lastly call another method to login, and when logged in we have a callback
            }
            else
            {
                // if we are still not connected, then try to process again
                OnGameSparksProcessFailed( "", "Sign In Error!", false, tryAgainAction, onFailedAction, uiManager, signInMenu );
            }
        }

        public void SignOut()
        {
            EndSession();
        }


        public void ChangePassword( string oldPassword, string newPassword,
                                    Action onCompleteAction, Action onFailedAction,
                                    UIManager uiManager, GameSparksMenu menu )
        {
            new ChangeUserDetailsRequest()
                .SetNewPassword( newPassword )
                .SetOldPassword( oldPassword )
                .Send( ( response ) => {
                    if ( !response.HasErrors )
                    {
                        uiManager.DisplayMessageDialog( "Change Password", 
                                                        "Your password has been changed successfully",
                                                        UIManager.MessageDialogAction.OK,
                                                        onCompleteAction );
                    }
                    else
                    {
                        Debug.Log( "Error: " + response.Errors.ToString() );
                        OnGameSparksProcessFailed( "", "Failed to change password", false,
                                                    () =>
                                                    {
                                                        ChangePassword( oldPassword, newPassword,
                                                                        onCompleteAction, onFailedAction,
                                                                        uiManager, menu );
                                                    },
                                                    onFailedAction, 
                                                    uiManager, menu );
                    }
                } 
            );
        }

        #endregion


        #region [Registration]

        public void Register( string displayName, string userName, string password,
                              Action onCompleteAction, Action onFailedAction,
                              UIManager uiManager, GameSparksCreateNewAccountMenu menu )
        {
            Action tryAgainAction = ()=> { Register( displayName, userName, password, onCompleteAction, onFailedAction, uiManager, menu ); };
            new RegistrationRequest(  )
                .SetDisplayName( displayName )
                .SetUserName( userName )
                .SetPassword( password )
                .Send( ( reg_response ) => {
                    if ( !reg_response.HasErrors )
                    {
                        _response = new AuthenticationResponse( reg_response.BaseData );

                        ActiveUser user;
                        InitiateUserDataWithDefaultValues( out user );
                        UploadUserData( user, onCompleteAction, onFailedAction, uiManager );

                        // Submit new score.
                        SubmitScore( 0 );
                    }
                    else
                    {
                        if ( reg_response.Errors.GetString( "USERNAME" ) == "TAKEN" )
                        {
                           OnGameSparksProcessFailed( "", "The userName is already in use", false, tryAgainAction, onFailedAction, uiManager, menu ); 
                        }
                    }
            } );
        }

        #endregion


        #region [Level JOSN]

        public void UploadLevelJSON( string json, Action<string> onCompleteAction ) 
	    {
            _onUploadCompleteAction = onCompleteAction;

		    new GetUploadUrlRequest()
                .Send( ( response ) =>
		        {
                    if ( response.HasErrors )
                    {
                        Debug.Log( "response.HasErrors: " + response.Errors.JSON );
                        _onUploadCompleteAction( string.Empty );
                        return;
                    }

			        //Start coroutine and pass in the upload url
			        new CoroutineTask( UploadLevelJSONCoroutine( response.Url, json ) );  
		        });
	    }
	    
	    IEnumerator UploadLevelJSONCoroutine( string uploadUrl, string json )
	    {
		    yield return new WaitForEndOfFrame();

		    byte[] bytes = System.Text.Encoding.UTF8.GetBytes( json );

		    // Create a Web Form, this will be our POST method's data
		    var form = new WWWForm();
		    form.AddBinaryData( "file", bytes, "jsonTest.json", "text" );

		    //POST the screenshot to GameSparks
		    WWW w = new WWW( uploadUrl, form ); 
		    yield return w;

		    if ( w.error != null )
		    {
			    Debug.Log( w.error );
		    }
		    else
		    {
			    //Debug.Log( "No Error: " + w.text );
		    }
	    }

	    //This will be our message listener, this will be triggered when we successfully upload a file
	    public void GetUploadMessage( GSMessage message )
	    {
            if ( _onUploadCompleteAction != null )
                _onUploadCompleteAction( message.BaseData.GetString( "uploadId" ) );
	    }


        public void DownloadLevelJSON( string customID, Action<string> onCompleteAction )
	    {
		    new GetUploadedRequest()
                .SetUploadId( customID ).Send( ( response ) =>
		        {
                    if ( response.HasErrors )
                    {
                        onCompleteAction( string.Empty );
                        return;
                    }

			        //pass the url to our coroutine that will accept the data
			        new CoroutineTask( DownloadLevelJOSNCoroutine( response.Url, onCompleteAction ) );
		        } );
	    }
	    IEnumerator DownloadLevelJOSNCoroutine( string downloadUrl, Action<string> onCompleteAction )
	    {
		    var www = new WWW( downloadUrl );
		    yield return www;

            if ( onCompleteAction != null )
                onCompleteAction( www.text );
	    }

        public void RemovedJSONFile( string id )
        {
            //Spark.getFiles().uploadedJson( "myUploadId" );
        }

        #endregion


        #region [User Data]

        public void UploadUserData( ActiveUser user, 
                                    Action onCompleteAction, Action onFailedAction,
                                    UIManager uiManager )
        {
            //We create a GSRequestData variable by using jsonDataToSend.Add() 
            //we can add in any variable we choose and they will be converted to JSON
			GSRequestData jsonDataToSend = new GSRequestData();
            jsonDataToSend.AddString( "userData", JsonUtility.ToJson( user.userData ) );
            jsonDataToSend.AddString( "landsData", JsonUtility.ToJson( user.landsData ) );

            //We then send our LogEventRequest with the event shortcode and the event attrirbute
			new LogEventRequest()
                .SetEventKey( "setPlayerDataJSON" )
			    .SetEventAttribute( "JSONData", jsonDataToSend )
			    .Send( ( response ) =>
			    {
                    if ( !response.HasErrors )
                    {
                        onCompleteAction();
                    } 
                    else
                        OnGameSparksProcessFailed( "Upload Error", "Failed to sumbit your progress, try again?", true, 
                                                   ()=> { UploadUserData( user, onCompleteAction, onFailedAction, uiManager ); }, 
                                                   onFailedAction, uiManager, null );
			    } ); 
        }

        public void DownloadUserData( Action<GSData> onCompleteAction, Action onFailedAction,
                                      UIManager uiManager )
        {
            Action tryAgainAction = ()=> { DownloadUserData( onCompleteAction, onFailedAction, uiManager ); };

            //Send our logEventRequest to retrive information from the collection
			new LogEventRequest()
                .SetEventKey( "getPlayerData" )
                .SetEventAttribute( "userId", _response.UserId )
                .Send( ( getPlayerStateResponse ) =>
			    {
				    if ( !getPlayerStateResponse.HasErrors )
				    {
                        //Assign the variables from our document to our instance
                        if ( getPlayerStateResponse.ScriptData == null )
                        {
                            Debug.Log( "\nScriptData is NULL!!!" );
                            OnGameSparksProcessFailed( "Error", "Failed to download user data, try again?", true, 
                                                       tryAgainAction, onFailedAction, uiManager, null );
                        }
                        else if ( getPlayerStateResponse.ScriptData.GetGSData( "playerState" ) == null ||
                                  !getPlayerStateResponse.ScriptData.GetGSData( "playerState" ).ContainsKey( "userData" ) )
                        {
                            Debug.Log( "GetGSData( playerState ) is NULL!!!" );
                            CreateGameDataFile( 
                                ()=> { new LogEventRequest()
                                        .SetEventKey( "getPlayerData" )
                                        .SetEventAttribute( "userId", _response.UserId )
                                        .Send( ( response2 ) =>
			                            {
				                            if ( !getPlayerStateResponse.HasErrors )
				                            {
                                                onCompleteAction( response2.ScriptData );
                                            }
                                            else
                                            {
                                                OnGameSparksProcessFailed( "Error", "Failed to download user data, try again?", true, 
                                                                            tryAgainAction, onFailedAction, uiManager, null );
				                            }
			                            } );
                                }, 
                                onFailedAction, 
                                uiManager );
                        }
                        else
                            onCompleteAction( getPlayerStateResponse.ScriptData );
                    }
                    else
                    {
                        OnGameSparksProcessFailed( "Error", "Failed to download user data, try again?", true, 
                                                    tryAgainAction, onFailedAction, uiManager, null );
				    }
			    } );
        }

        void CreateGameDataFile( Action onCompleteAction, Action onFailedAction,
                                 UIManager uiManager )
        {
            new LogEventRequest()
                .SetEventKey( "createGameDataFile" )
		        .Send( ( response ) =>
		        {
                    if ( !response.HasErrors )
                    {
                        Debug.Log( "Successfully created game data file : " + response.JSONString );
                        ActiveUser user;
                        InitiateUserDataWithDefaultValues( out user );
                        UploadUserData( user, onCompleteAction, onFailedAction, uiManager );
                    } 
                    else
                    {
                        Debug.Log( "Error: " + response.Errors.ToString() );
                        CreateGameDataFile( onCompleteAction, onFailedAction, uiManager );
                    }
		        } ); 
        }

        public void InitiateUserDataWithDefaultValues( out ActiveUser user )
        {
            // 1. Init
            var userData = new UserData();
            userData = new UserData();
            userData.coins = 0;
            userData.boosters = new int[3];
            userData.painters = new int[5];

            var cloudLandsData = new CloudLandsData();
            cloudLandsData.UnlockLevel( 0, 0 );

            user = new ActiveUser();
            user.userData = userData;
            user.landsData = cloudLandsData;
        }

        #endregion


        #region [Leaderboard]

        public void SubmitScore( long score )
        {
            new LogEventRequest()
                .SetEventKey( "submitScore" )
                .SetEventAttribute( "score", score )
                .Send( ( response ) => {
                    if ( !response.HasErrors )
                    {
                        //Debug.Log( "Score Posted Successfully..." );
                    }
                    else
                    {
                        //Debug.Log( "Error Posting Score..." );
                    }
                } );
        }

        public void RetrieveUserScore( Action<long, int> onComplete )
        {
            new GetLeaderboardEntriesRequest()
                .SetPlayer( _response.UserId )
                .SetLeaderboards( new List<string>( new string[] { "SCORE_LEADERBOARD" } ) )
                .Send( ( response ) => {
                    if ( !response.HasErrors )
                    {
                        GSData data = response.JSONData["SCORE_LEADERBOARD"] as GSData;
                        if ( string.IsNullOrEmpty( data.JSON ) || data.JSON == "{}" )
                        {
                            onComplete( 0, 0 );
                        }
                        else
                        {                       
                            onComplete( (long)data.GetNumber( "score" ), (int)data.GetNumber( "rank" ) );
                        }

                    }
                    else
                    {
                        Debug.Log("Error Retrieving Player Rank ...");
                        if ( onComplete != null )
                            onComplete( -100, -100 );  
                    }
                });
        }

        public void RetrieveScoreLeaderboard( long count, 
                                              Action<LeaderboardDataResponse> onCompleteAction, Action onFailedAction,
                                              UIManager uiManager )
        {
            new LeaderboardDataRequest()
                .SetLeaderboardShortCode( "SCORE_LEADERBOARD" )
                .SetEntryCount( count )
                .Send( ( response ) => {
                    if ( !response.HasErrors )
                    {
                        onCompleteAction( response );
                    }
                    else
                    {
                        Debug.Log( "Error Retrieving Leaderboard Data..." );
                        uiManager.DisplayMessageDialog( "Error", "Failed to retrieve Leaderboard Data. Try Again?", 
                                                        UIManager.MessageDialogAction.OK_Cancel,
                                                        ()=> { RetrieveScoreLeaderboard( count, onCompleteAction, onFailedAction, uiManager ); },
                                                        onFailedAction );
                    }
                });
        }

        #endregion


        void OnGameSparksProcessFailed( string title, string msg, 
                                        bool showMsgDialog,
                                        Action onTryAgainAction, Action onFailedAction,
                                        UIManager uiManager, GameSparksMenu menu )
        {
            uiManager.FadeOutLoadingScreen( ()=> {
                if ( showMsgDialog )
                {
                    uiManager.DisplayMessageDialog( title, 
                                                    msg, 
                                                    UIManager.MessageDialogAction.OK_Cancel, 
                                                    onTryAgainAction,
                                                    onFailedAction
                                                    );
                }
                else
                {
                    menu.Display();
                    menu.ShowErrorMsg( msg );
                }

            }, 0.1f );
        }
    }
}