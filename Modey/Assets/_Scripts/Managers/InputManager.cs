using UnityEngine;

namespace MANA3DGames
{
    public class InputManager
    {
        GameManager _gameManager;

        bool _lockButtons;
        public bool IsLockButtons { get { return _lockButtons; } }
        bool _lockSwipe;
        bool _canDrag;
        bool _tapToRotate;

        Collider2D _border;

        float _swipeActionDuration = 0.25f;//0.15f;
        float _lastTimeSwipeAction;




        public InputManager( GameManager manager )
        {
            _gameManager = manager;

            EasyTouch.On_SwipeStart += On_SwipeStart;
            EasyTouch.On_Swipe += On_Swipe;
            EasyTouch.On_SwipeEnd += On_SwipeEnd;
        }


        public void Update()
        {
            if ( _lockButtons ) return;

#if UNITY_EDITOR
            if ( Input.GetMouseButtonDown( 0 ) )
                ProcessInputDown( Input.mousePosition );
            if ( Input.GetMouseButtonUp( 0 ) )
                ProcessInputUp( Input.mousePosition );

#elif UNITY_ANDROID

        if ( Input.touchCount > 0 )
        {
            if ( Input.GetTouch(0).phase == TouchPhase.Began )
                ProcessInputDown( Input.GetTouch(0).position );
            else if ( Input.GetTouch(0).phase == TouchPhase.Ended )
                ProcessInputUp( Input.GetTouch(0).position );
            else if ( Input.GetTouch(0).phase == TouchPhase.Canceled )
                ProcessInputCanceled( Input.GetTouch(0).position );
        }
#endif
        }


        void ProcessInputDown( Vector2 inputPos )
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint( inputPos );
            Collider2D collider = Physics2D.OverlapPoint( pos );

            if ( collider )
            {
                if ( collider == _border && !_gameManager.IsStartupTimerState )
                    _tapToRotate = true;
                else
                    _gameManager.OnClickButtonStart( collider.GetComponent<SpriteRenderer>() );
            }
        }

        void ProcessInputUp( Vector2 inputPos )
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint( inputPos );
            Collider2D collider = Physics2D.OverlapPoint( pos );

            if ( collider == _border && !_gameManager.IsStartupTimerState )
            {
                if ( _tapToRotate )
                {
                    _gameManager.RotateBlock();
                    _tapToRotate = false;
                }
            }
            else if ( collider )
                _gameManager.OnClickButtonEnd( collider.name );
            //else
            //    _gameManager.ResetCurrentSpriteColor();

            _gameManager.ResetCurrentSpriteColor();
        }

        void ProcessInputCanceled( Vector2 inputPos )
        {
            _tapToRotate = false;
            _gameManager.ResetCurrentSpriteColor();
        }



        public void LockButtons( bool lockBtn )
        {
            _lockButtons = lockBtn;
        }

        public void LockSwipe( bool lockSwipe )
        {
            _lockSwipe = lockSwipe;
        }

        public void LockAll( bool val )
        {
            _lockButtons = val;
            _lockSwipe = val;
        }



        // At the swipe beginning 
        private void On_SwipeStart( Gesture gesture )
        {
            if ( _lockSwipe ) return;

            _canDrag = true;
        }

        // During the swipe
        private void On_Swipe( Gesture gesture )
        {
            if ( !_gameManager.IsGameplayState ) return;

            if ( _lockSwipe || !_canDrag ||
                 Time.time < _lastTimeSwipeAction + _swipeActionDuration ) return;

            _tapToRotate = false;
            _lastTimeSwipeAction = Time.time;


            switch ( gesture.swipe )
            {
                case EasyTouch.SwipeType.Right:
                    _gameManager.OnRightSwipe();
                    break;
                case EasyTouch.SwipeType.Left:
                    _gameManager.OnLeftSwipe();
                    break;
                case EasyTouch.SwipeType.Down:
                    _gameManager.SpeedUpMoveY();
                    break;
            }
        }

        // At the swipe end 
        private void On_SwipeEnd( Gesture gesture )
        {
            if ( _lockSwipe ) return;

            float speed = ( gesture.swipeLength / gesture.actionTime ) * 0.01f;

            if ( speed < 3.0f || gesture.actionTime > 1.0f ) return;

            switch ( gesture.swipe )
            {
                case EasyTouch.SwipeType.Right:
                    _gameManager.OnRightSwipe();
                    break;
                case EasyTouch.SwipeType.Left:
                    _gameManager.OnLeftSwipe();
                    break;
                case EasyTouch.SwipeType.Down:
                    OnSwipeDown( speed );
                    break;
            }
        }



        void OnSwipeDown( float speed )
        {
            if ( speed > 10 )
                _gameManager.PushDown();
            else
                _gameManager.SpeedUpMoveY();
        }

        public void OnBlockLanded()
        {
            _canDrag = false;
        }


        public void SetBorderCollider( Collider2D border )
        {
            _border = border;
        }
    }
}
