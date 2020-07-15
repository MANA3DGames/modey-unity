using UnityEngine;
using System.Collections.Generic;
using GameSparks.Api.Responses;
using TMPro;

namespace MANA3DGames
{
    public class GameSparksLeaderboardMenu
    {
        GameManager         _gameManager;
        UIManager           _uiManager;

        GOGroup _group;

        List<List<GameObject>>  _pages;
        List<GameObject>        _scoresGOs;

        GameObject _scorePrefab;

        const int MAX_TOTAL_SCORES_NUM = 50;
        const int MAX_SCORES_IN_PAGE_NUM = 10;
        int _pageCount;
        int _currentPageID;
        float Y_POS = 4.1f;



        public GameSparksLeaderboardMenu( UIManager uiManager, GameManager gameManager )
        {
            _gameManager = gameManager;
            _uiManager = uiManager;

            _group = new GOGroup( uiManager.GetMenuGO( "GameSparksLeaderboardMenu" ) );
            _scorePrefab = _group.Get( "ScorePrefab" );

            uiManager.AddOnClickListener( "GameSparksLeaderboardMenu_BackBtn", OnClickBackBtn );

            for ( int i = 0; i < 5; i++ )
            {
                int temp = i;
                uiManager.AddOnClickListener( "GameSparksLeaderboardMenu_PageBtn" + temp, ()=> { DisplayPage( temp ); } );
            }
        }



        void DisplayPage( int id )
        {
            if ( _currentPageID == id )
                return;

            _gameManager.LockAllInputs( true );
            LeanTween.cancelAll();

            for ( int i = 0; i <= _pageCount; i++ )
            {
                _group.ShowItem( "GameSparksLeaderboardMenu_PageBtn" + i, true );
                _group.SetSpriteColor( "GameSparksLeaderboardMenu_PageBtn" + i, Color.gray );
                foreach ( var item in _pages[i] )
                    item.SetActive( false );
            }

            System.Action action = () => {
                _group.SetSpriteColor( "GameSparksLeaderboardMenu_PageBtn" + id, Color.white );
                foreach ( var item in _pages[id] )
                    item.SetActive( true );

                _gameManager.LockAllInputs( false, false );
                _currentPageID = id;
            };

            float delay = 0.0f;
            float delayInc = 0.1f;

            for ( int i = 0; i < _pages[id].Count; i++ )
            {
                var x = _pages[id][i].transform.localPosition.x;
                var pos = _pages[id][i].transform.localPosition;
                pos.x = _currentPageID < id ? pos.x + 10 : pos.x - 10;
                _pages[id][i].transform.localPosition = pos;
                var tween = LeanTween.moveX( _pages[id][i], x, 0.3f );
                tween.setDelay( delay+=delayInc );
                _pages[id][i].SetActive( true );

                if ( i == _pages[id].Count - 1 )
                    tween.setOnComplete( action );
            }
        }

        public void NextPage()
        {
            if ( _currentPageID < _pageCount )
                DisplayPage( _currentPageID + 1 );
            else
                DisplayPage( 0 );
        }
        public void PreviousPage()
        {
            if ( _currentPageID > 0 )
                DisplayPage( _currentPageID - 1 );
            else
                DisplayPage( _pageCount );
        }




        public void Close()
        {
            if ( _scoresGOs != null )
            {
                while ( _scoresGOs.Count > 0 )
                {
                    GameObject.Destroy( _scoresGOs[0] );
                    _scoresGOs.RemoveAt( 0 );
                }

                _scoresGOs.Clear();
                _scoresGOs = null;
            }

            _group.ShowRoot( false );
        }

        public void OnClickBackBtn()
        {
            Close();
            _gameManager.GoBackFromGameSparksLeaderboard();
        }

        public void Display( LeaderboardDataResponse leaderBoardData )
        {
            _group.ShowAllItems( false );
            _group.ShowRoot( true );
            _group.ShowItem( "BG", true );
            _group.ShowItem( "Title", true );
            _group.ShowItem( "Top", true );
            _group.ShowItem( "GameSparksLeaderboardMenu_BackBtn", true );

            if ( leaderBoardData == null || leaderBoardData.Data == null )
            {
                return;
            }

            _scoresGOs = new List<GameObject>();

            if ( _pages != null )
            {
                foreach ( var page in _pages )
                    page.Clear();
                _pages.Clear();
            }
            _pages = new List<List<GameObject>>();
            _pages.Add( new List<GameObject>() );

            int scoreCountInPage = 0;
            _pageCount = 0;
            float yPos = Y_POS;

            foreach ( LeaderboardDataResponse._LeaderboardData entry in leaderBoardData.Data )
            {
                int rank = (int)entry.Rank;
                string playerName = entry.UserName;
                string score = entry.JSONData["score"].ToString();

                var go = GameObject.Instantiate( _scorePrefab, _group.GetRoot().transform );
                go.transform.localPosition = new Vector3( 0, yPos, 0 );
                go.transform.Find( "Rank" ).GetComponent<TextMeshPro>().text = rank.ToString();
                //go.transform.FindChild( "Name" ).GetComponent<TextMeshPro>().text = playerName;
                _uiManager.SetTextFromOutSide( go.transform.Find( "Name" ), playerName );
                go.transform.Find( "Score" ).GetComponent<TextMeshPro>().text = score;
                go.SetActive( true );
                
                _scoresGOs.Add( go );

                yPos -= 0.9f;

                _pages[_pageCount].Add( go );
                scoreCountInPage++;

                if ( _scoresGOs.Count >= MAX_TOTAL_SCORES_NUM )
                    break;

                if ( scoreCountInPage >= MAX_SCORES_IN_PAGE_NUM )
                {
                    _pages.Add( new List<GameObject>() );
                    _pageCount++;
                    yPos = Y_POS;
                    scoreCountInPage = 0;
                }
            }

            if ( _pages[_pages.Count-1].Count == 0 )
            {
                _pages.RemoveAt( _pages.Count-1 );
                _pageCount--;
            }

            _group.ShowRoot( true );

            _currentPageID = -1;
            DisplayPage( 0 );
        }

        public void ShowAgain()
        {
            _group.ShowRoot( true );
        }

        void StartLevel( string senderName, string gameSparksID )
        {
            _group.ShowRoot( false );
            _gameManager.DownloadJSONToGameSparks( senderName, gameSparksID );
        }


        public GameObject GetTitleGameObject()
        {
            return _group.Get( "Title" );
        }
    }
}