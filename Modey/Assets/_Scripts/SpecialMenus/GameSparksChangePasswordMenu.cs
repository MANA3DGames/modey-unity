
namespace MANA3DGames
{
    public class GameSparksChangePasswordMenu : GameSparksMenu
    {
        public GameSparksChangePasswordMenu( GameManager gameManager, UIManager uiManager )
        {
            _gameManager = gameManager;
            _group = new GOGroup( uiManager.GetMenuGO( "GameSparksChangePasswordDialog" ) );

            _filedText = new string[3];

            for ( int i = 0; i < _filedText.Length; i++ )
            {
                int temp = i;
                uiManager.AddOnClickListener( "GameSparksChangePasswordDialog_InputField" + temp, ()=> { OpenKeyboard( temp, true ); } );
            }

            uiManager.AddOnClickListener( "GameSparksChangePasswordDialog_CancelBtn", OnClickCancelXBtn );
            uiManager.AddOnClickListener( "GameSparksChangePasswordDialog_SubmitBtn", OnClickGameSparksChangePasswordDialog_SubmitBtnBtn );
        }

        void OnClickCancelXBtn()
        {
            _group.ShowRoot( false );
            _gameManager.GoToGameSparksMenu();
        }
        void OnClickGameSparksChangePasswordDialog_SubmitBtnBtn()
        {
            bool missingField = CheckMissingField();

            if ( _filedText[1] != _filedText[2] )
            {
                missingField = true;
                ShowErrorMsg( "* Passwords don't match" );
            }
            else if ( string.IsNullOrEmpty( _filedText[1] ) || string.IsNullOrEmpty( _filedText[2] ) )
            {
                ShowRequiredFieldLabel( 1, true );
                ShowRequiredFieldLabel( 2, true );
                missingField = true;
            }
            else if ( _filedText[1].Length < 3 || _filedText[2].Length < 3 )
            {
                ShowErrorMsg( "* Too short password" );
                ShowRequiredFieldLabel( 1, true );
                ShowRequiredFieldLabel( 2, true );
                missingField = true;
            }

            if ( !missingField )
            {
                _group.ShowRoot( false );
                _gameManager.ChangeGameSparksPassword( _filedText[0], _filedText[1] );
            }
        }
    }
}