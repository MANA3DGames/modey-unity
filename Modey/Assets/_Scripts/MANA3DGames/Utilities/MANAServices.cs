using System;
using System.Collections;
using UnityEngine;
using MANA3DGames.Utilities.Coroutine;

namespace MANA3DGames.Utilities.Services
{
    public class MANAServices
    {
        CoroutineTask _checkBuildTask;

        public MANAServices()
        {

        }


        public void OpenMANA3DGames()
        {
            //_leaderboardManager.LogEvent( PlayGamesPlatformManager.EVENT_ACCESS_MANA3DGAMES );
            Application.OpenURL( "http://mana3dgames.com" );
        }


        public void CheckBuildVersion( string link, Action<bool> onCompleteAction )
        {
            _checkBuildTask = new CoroutineTask( CheckBuildVersionCoroutine( link, onCompleteAction ), true );
        }
        IEnumerator CheckBuildVersionCoroutine( string link, Action<bool> onCompleteAction )
        {
            WWW www = new WWW( link );
            yield return www;

            if ( www.error != null )
            {
                Debug.Log( "Error: " + www.error );
                // for example, often 'Error .. 404 Not Found'
               
                if ( onCompleteAction != null )
                    onCompleteAction.Invoke( false );
            }
            else if ( www.text != Application.version )
            {
                if ( onCompleteAction != null )
                    onCompleteAction.Invoke( true );
            }
            else
            {
                if ( onCompleteAction != null )
                    onCompleteAction.Invoke( false );
            }
        }

        public bool IsCheckingBuildVersion()
        {
            return _checkBuildTask != null && _checkBuildTask.IsRunning;
        }


        public void OpenGooglePlayStore()
        {
            Application.OpenURL( "market://details?id=" + Application.identifier ); 
        }

        
        CoroutineTask _checkInternetConnectionTask;
        float _timeOut;
        public void CheckInternetConnection( Action<bool> action )
        {
            _timeOut = Time.time;
            _checkInternetConnectionTask = new CoroutineTask( CheckInternetConnectionCoroutine( action ) );
        }
        
        IEnumerator CheckInternetConnectionCoroutine( Action<bool> action )
        {
            WWW www = new WWW( "http://google.com" );
            //yield return www;
            while ( !www.isDone )
            {
                if ( Time.time > _timeOut + 10.0f )
                {
                    Debug.Log( "TimeOut" );
                    action( false );
                    _checkInternetConnectionTask.kill();
                    _checkInternetConnectionTask = null;
                }
                yield return null;
            }
            action( www.error == null );
        } 



        public void MinimizeApp()
        {
            AndroidJavaObject activity = new AndroidJavaClass( "com.unity3d.player.UnityPlayer" ).GetStatic<AndroidJavaObject>( "currentActivity" );
            activity.Call<bool>( "moveTaskToBack", true );
        }
    }
}

