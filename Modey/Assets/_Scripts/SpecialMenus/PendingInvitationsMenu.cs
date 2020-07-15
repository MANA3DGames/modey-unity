using UnityEngine;
using System.Collections.Generic;

namespace MANA3DGames
{
    public class PendingInvitationsMenu
    {
        public class Invitation
        {
            public string senderID;
            public string gameSparksID;
            public GameObject buttonGameObject;


            public void SetProfilePicture( Sprite sprite )
            {
                buttonGameObject.transform.Find( "img" ).GetComponent<SpriteRenderer>().sprite = sprite;
            }

            public void Destroy()
            {
                GameObject.Destroy( buttonGameObject );
                senderID = string.Empty;
                gameSparksID = string.Empty;
            }
        }

        GameManager     _gameManager;
        UIManager       _uiManager;
        FacebookManager _fbManager;

        GOGroup _group;
        Dictionary<string, Invitation> _invitations;
        List<List<GameObject>> _pages;

        GameObject _btnPrefab;

        const int MAX_TOTAL_INVITATION_NUM = 24;
        const int MAX_INVITATION_IN_PAGE_NUM = 6;
        int _pageCount;
        int _currentPageID;
        float Y_POS = 4.1f;



        public PendingInvitationsMenu( UIManager uiManager, GameManager gameManager, FacebookManager fbManager )
        {
            _gameManager = gameManager;
            _uiManager = uiManager;
            _fbManager = fbManager;

            _group = new GOGroup( uiManager.GetMenuGO( "PendingInvitationMenu" ) );
            _btnPrefab = _group.Get( "InvitationBtnPrefab" );

            uiManager.AddOnClickListener( "PendingInvitationMenu_BackBtn", OnClickBackBtn );

            for ( int i = 0; i < 4; i++ )
            {
                int temp = i;
                uiManager.AddOnClickListener( "PendingInvitationMenu_PageBtn" + temp, ()=> { DisplayPage( temp ); } );
            }
        }



        void DisplayPage( int id )
        {
            if ( _currentPageID == id )
                return;

            //_gameManager.LockButtonsInput( true );
            _gameManager.LockAllInputs( true );
            LeanTween.cancelAll();

            for ( int i = 0; i <= _pageCount; i++ )
            {
                _group.ShowItem( "PendingInvitationMenu_PageBtn" + i, true );
                _group.SetSpriteColor( "PendingInvitationMenu_PageBtn" + i, Color.gray );
                foreach ( var item in _pages[i] )
                    item.SetActive( false );
            }

            System.Action action = () => {
                _group.SetSpriteColor( "PendingInvitationMenu_PageBtn" + id, Color.white );
                foreach ( var item in _pages[id] )
                    item.SetActive( true );
                //_gameManager.LockButtonsInput( false );
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
            if ( _invitations != null )
            {
                foreach ( string key in _invitations.Keys )
                    _invitations[key].Destroy();

                _invitations.Clear();
                _invitations = null;
            }

            _group.ShowRoot( false );
        }

        public void OnClickBackBtn()
        {
            Close();
            _uiManager.DisplayFacebookMenu();
        }

        List<object> FillFakeInvitations()
        {
            var invitations = new List<object>();

            for ( int i = 0; i < MAX_TOTAL_INVITATION_NUM; i++ )
            {
                Dictionary<string, object> reqConvertTest = new Dictionary<string, object>();
                Dictionary<string, object> fromStringTest = new Dictionary<string, object>();
                fromStringTest.Add( "name", "name" + i );
                fromStringTest.Add( "id", "fromID0000" + i );
                reqConvertTest.Add( "from", fromStringTest );
                reqConvertTest.Add( "data", "gameSprtksID0000" + i );
                invitations.Add( reqConvertTest );
            }

            return invitations;
        }

        public void Display( List<object> invitations )
        {
            //// ****************************************************
            //invitations = FillFakeInvitations();
            //// ****************************************************

            _group.ShowAllItems( false );
            _group.ShowRoot( true );
            _group.ShowItem( "BG", true );
            _group.ShowItem( "Title", true );
            _group.ShowItem( "Top", true );
            _group.ShowItem( "PendingInvitationMenu_BackBtn", true );

            if ( invitations == null || invitations.Count == 0 )
            {
                _group.ShowItem( "MSG", true );
                return;
            }

            _invitations = new Dictionary<string, Invitation>();

            if ( _pages != null )
            {
                foreach ( var page in _pages )
                    page.Clear();
                _pages.Clear();
            }
            _pages = new List<List<GameObject>>();
            _pages.Add( new List<GameObject>() );

            int invitationCountInPage = 0;
            _pageCount = 0;
            float yPos = Y_POS;

            // For every item in newObj is a separate request, so iterate on each of them separately.
            for ( int i = invitations.Count - 1; i >= 0; i-- )
            {
                Dictionary<string, object> reqConvert = invitations[i] as Dictionary<string, object>;
                Dictionary<string, object> fromString = reqConvert["from"] as Dictionary<string, object>;
                //Dictionary<string, object> toString = reqConvert["to"] as Dictionary<string, object>;

                string fromID = fromString["id"] as string;
                string fromName = fromString["name"] as string;
                //string obID = reqConvert["id"] as string;
                //string message = reqConvert["message"] as string;
                //string toName = toString["name"] as string;
                //string toID = toString["id"] as string;

                //Debug.Log( "Object ID: " + obID );
                //Debug.Log( "Sender message: " + message );
                //Debug.Log( "Sender name: " + fromName );
                //Debug.Log( "Sender ID: " + fromID );
                //Debug.Log( "Recipient name: " + toName );
                //Debug.Log( "Recipient ID: " + toID );
                //Debug.Log( "Data : " + reqConvert["data"] as string );

                if ( !_invitations.ContainsKey( fromID ) )
                {
                    var btnName = "PendingInvitationBtn_" + _invitations.Count;
                    var invitation = new Invitation();
                    invitation.senderID = fromID;
                    invitation.gameSparksID = reqConvert["data"] as string;
                    invitation.buttonGameObject = GameObject.Instantiate( _btnPrefab, _group.GetRoot().transform );
                    invitation.buttonGameObject.transform.Find( "btn" ).gameObject.name = btnName;
                    invitation.buttonGameObject.transform.localPosition = new Vector3( 0, yPos, 0 );
                    invitation.buttonGameObject.SetActive( true );
                    var text = invitation.buttonGameObject.transform.Find( "Description" ).GetComponent<TMPro.TextMeshPro>();
                    var date = reqConvert["created_time"] as string;
                    date = date.Substring( 0, 16 );
                    date = date.Replace( "T", "  " );
                    //text.text = fromName + "\n" + date;
                    _uiManager.SetTextFromOutSide( text.transform, fromName );
                    text.text += "\n" + date;
                    _invitations.Add( fromID, invitation );
                    yPos -= 1.6f;

                    _uiManager.AddOnClickListener( btnName, ()=> { StartLevel( fromName, invitation.gameSparksID ); } );

                    _pages[_pageCount].Add( invitation.buttonGameObject );

                    invitationCountInPage++;


                    _fbManager.GetFrinedProfilePicture( fromID, ( sprite )=> {
                        invitation.SetProfilePicture( sprite );
                    } );
                }

                if ( _invitations.Count >= MAX_TOTAL_INVITATION_NUM )
                    break;

                if ( invitationCountInPage >= MAX_INVITATION_IN_PAGE_NUM )
                {
                    _pages.Add( new List<GameObject>() );
                    _pageCount++;
                    yPos = Y_POS;
                    invitationCountInPage = 0;
                }
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