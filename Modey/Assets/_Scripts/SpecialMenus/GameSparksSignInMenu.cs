using UnityEngine;

namespace MANA3DGames
{
    public class GameSparksSignInMenu : GameSparksMenu
    {
        public GameSparksSignInMenu( GameManager gameManager, UIManager uiManager )
        {
            _gameManager = gameManager;
            _group = new GOGroup( uiManager.GetMenuGO( "GameSparksSignInDialog" ) );

            _filedText = new string[2];

            for ( int i = 0; i < _filedText.Length; i++ )
            {
                int temp = i;
                uiManager.AddOnClickListener( "GameSparksSignInDialog_InputField" + temp, () => { OpenKeyboard( temp, temp == 1 ); } );
            }

            uiManager.AddOnClickListener( "GameSparksSignInDialog_CancelBtn", OnClickCancelXBtn );
            uiManager.AddOnClickListener( "GameSparksSignInDialog_SignInBtn", OnClickSignInBtn );
            uiManager.AddOnClickListener( "GameSparksSignInDialog_SignInWithFBBtn", OnClickSignInWithFBBtn );
            uiManager.AddOnClickListener( "GameSparksSignInDialog_CreateNewAccountBtn", OnClickCreateNewAccountBtn );
        }

        void OnClickCancelXBtn()
        {
            _group.ShowRoot( false );
            _gameManager.OnCancelGameSparksAuthentication();
        }
        void OnClickSignInBtn()
        {
            bool missingField = CheckMissingField();

            if ( !missingField )
            {
                _group.ShowRoot( false );
                _gameManager.AuthenticateGameSparks( _filedText[0], _filedText[1] );
            }   
        }
        void OnClickSignInWithFBBtn()
        {
            _group.ShowRoot( false );
            _gameManager.AuthenticateGameSparksWithFacebook(); 
        }
        void OnClickCreateNewAccountBtn()
        {
            _group.ShowRoot( false );
            _gameManager.GoToGameSparksCreateNewUserMenu();
        }
    }
}
