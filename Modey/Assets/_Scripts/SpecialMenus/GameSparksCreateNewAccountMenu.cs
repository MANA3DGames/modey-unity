using System.Text.RegularExpressions;

namespace MANA3DGames
{
    public class GameSparksCreateNewAccountMenu : GameSparksMenu
    {
        public GameSparksCreateNewAccountMenu( GameManager gameManager, UIManager uiManager )
        {
            _gameManager = gameManager;
            _group = new GOGroup( uiManager.GetMenuGO( "GameSparksCreateNewAccountDialog" ) );

            _filedText = new string[4];

            for ( int i = 0; i < _filedText.Length; i++ )
            {
                int temp = i;
                uiManager.AddOnClickListener( "GameSparksCreateNewAccountDialog_InputField" + temp, ()=> { OpenKeyboard( temp, temp == 1 || temp == 2 ); } );
            }

            uiManager.AddOnClickListener( "GameSparksCreateNewAccountDialog_CancelBtn", OnClickCancelXBtn );
            uiManager.AddOnClickListener( "GameSparksCreateNewAccountDialog_CreateBtn", OnClickCreateBtn );
        }

        void OnClickCancelXBtn()
        {
            _group.ShowRoot( false );
            _gameManager.GoToGameSparksSignInMenu();
        }

        void OnClickCreateBtn()
        {
            bool missingField = CheckMissingField();

            bool isEmail = false;
            if ( !string.IsNullOrEmpty( _filedText[0] ) )
                isEmail = Regex.IsMatch( _filedText[0], @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase );
            if ( !isEmail )
            {
                ShowErrorMsg( "* Invaild email" );
                ShowRequiredFieldLabel( 0, true );
                missingField = true;
            }
            else if ( _filedText[1] != _filedText[2] )
            {
                missingField = true;
                ShowErrorMsg( "* Passwords don't match" );
            }
            else if ( _filedText[1].Length < 3 || _filedText[2].Length < 3 )
            {
                ShowErrorMsg( "* Too short password" );
                ShowRequiredFieldLabel( 1, true );
                ShowRequiredFieldLabel( 2, true );
                missingField = true;
            }
            else if ( _filedText[3].Length < 3 )
            {
                ShowErrorMsg( "* Too short Name" );
                ShowRequiredFieldLabel( 3, true );
                missingField = true;
            }

            if ( !missingField )
            {
                _group.ShowRoot( false );
                _gameManager.CreateNewGameSparksAccount( _filedText[3], _filedText[0], _filedText[1] );
            }   
        }
    }
}
