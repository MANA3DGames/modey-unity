using UnityEngine;

namespace MANA3DGames
{
    public class GameSparksMenu
    {
        protected GameManager _gameManager;

        protected GOGroup _group;

        protected TouchScreenKeyboard _keyboard;

        protected int _activeField;
        protected string[] _filedText;

        public bool IsActive { get { return _group.IsActive; } }

        bool _isSecure;


        public virtual void Display()
        {
            for ( int i = 0; i < _filedText.Length; i++ )
                ShowRequiredFieldLabel( i, false );
            _group.ShowItem( "ErrorMsg", false );
            _group.ShowRoot( true );
        }

        public virtual void ShowErrorMsg( string msg )
        {
            _group.SetText( "ErrorMsg", msg );
            _group.ShowItem( "ErrorMsg", true );
        }

        protected virtual string GenerateStarsString( int length )
        {
            string str = string.Empty;
            for ( int i = 0; i < length; i++ )
                str += "*";
            return str;
        }

        protected virtual void ShowRequiredFieldLabel( int id, bool show )
        {
            _group.Get( _group.GetRoot().name + "_InputField" + id ).transform.Find( "RequiredField" ).gameObject.SetActive( show );
        }

        protected void OpenKeyboard( int activeField, bool secure )
        {
            Debug.Log( activeField + "  " + secure );
            _activeField = activeField;
            _keyboard = TouchScreenKeyboard.Open( _filedText[activeField], TouchScreenKeyboardType.Default, false, false, secure );
            _isSecure = secure;
        }

        protected bool CheckMissingField()
        {
            bool missingField = false;
            for ( int i = 0; i < _filedText.Length; i++ )
            {
                if ( string.IsNullOrEmpty( _filedText[i] ) )
                {
                    ShowRequiredFieldLabel( i, true );
                    missingField = true;
                }
                else
                    ShowRequiredFieldLabel( i, false );
            }
            return missingField;
        }


        void TestingUpdate()
        {
            if ( Input.GetKeyDown( KeyCode.L ) )
            {
                _filedText[0] = "m.acaca";
                _group.SetInnerText( _group.GetRoot().name + "_InputField" + 0, "m.acaca" );

                _filedText[1] = "123";
                _group.SetInnerText( _group.GetRoot().name + "_InputField" + 1, GenerateStarsString( ( "123" ).Length ) );
            }
            if ( Input.GetKeyDown( KeyCode.O ) )
            {
                _filedText[0] = "bestofall@booal.com";
                _group.SetInnerText( _group.GetRoot().name + "_InputField" + 0, "bestofall@booal.com" );

                _filedText[1] = "123";
                _group.SetInnerText( _group.GetRoot().name + "_InputField" + 1, GenerateStarsString( ( "123" ).Length ) );

                _filedText[2] = "123";
                _group.SetInnerText( _group.GetRoot().name + "_InputField" + 2, GenerateStarsString( ( "123" ).Length ) );

                _filedText[3] = "Best Of All";
                _group.SetInnerText( _group.GetRoot().name + "_InputField" + 3, "Best Of All" );
            }
            if ( Input.GetKeyDown( KeyCode.C ) )
            {
                _filedText[0] = "newPass";
                _group.SetInnerText( _group.GetRoot().name + "_InputField" + 0, GenerateStarsString( ( "newPass" ).Length ) );

                string newPass = "newPass1";
                _filedText[1] = newPass;
                _group.SetInnerText( _group.GetRoot().name + "_InputField" + 1, GenerateStarsString( ( newPass ).Length ) );

                _filedText[2] = newPass;
                _group.SetInnerText( _group.GetRoot().name + "_InputField" + 2, GenerateStarsString( ( newPass ).Length ) );
            }

            if ( Input.GetKeyDown( KeyCode.Space ) )
            {
                Debug.Log( "Youuuu!!!" );
                new GameSparks.Api.Requests.ChangeUserDetailsRequest()
                    .SetDisplayName( "Zanggii" )
                    .Send( ( response ) => {
                        if ( !response.HasErrors )
                        {
                            Debug.Log( "Done change userName" );
                        }
                        else
                        {
                            Debug.Log( "failed to chagne userName" );
                        }
                    } 
                );
            }
        }

        public void Update()
        {
            TestingUpdate();


            if ( _keyboard == null )
                return;

            if ( _keyboard.active )
            {
                _filedText[_activeField] = _keyboard.text;
                _group.SetInnerText( _group.GetRoot().name + "_InputField" + _activeField, 
                                     _isSecure ? GenerateStarsString( _keyboard.text.Length ) : _keyboard.text );
            }
        }


        public void Close()
        {
            _group.ShowRoot( false );
        }
    }
}
