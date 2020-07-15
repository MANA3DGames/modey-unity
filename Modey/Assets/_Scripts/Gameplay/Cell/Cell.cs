using UnityEngine;

namespace MANA3DGames
{
    public class Cell
    {
        #region [Variables]

        /// <summary>
        /// The gameObject of this Cell.
        /// </summary>
        public GameObject gameObject;

        /// <summary>
        /// The transform component of this Cell.
        /// </summary>
        public Transform transform;

        /// <summary>
        /// The current for this Cell.
        /// </summary>
        private Room _room;
        /// <summary>
        /// Returns the current room for this Cell.
        /// </summary>
        public  Room room { get { return _room; } }
        
        /// <summary>
        /// Move X tween for this Cell. 
        /// </summary>
        private LTDescr _moveXTween;
        /// <summary>
        /// Move Y Tween for this Cell.
        /// </summary>
        private LTDescr _moveYTween;

        private int colorId;
        public int ColorID { get { return colorId; } }

        protected bool isFace;
        public bool IsFace { get { return isFace; } }

        SpriteRenderer sRenderer;
        public SpriteRenderer SRenderer { get { return sRenderer; } }

        #endregion


        #region [Constructors]

        /// <summary>
        /// public Constructor
        /// </summary>
        /// <param name="go">The given gameObject</param>
        public Cell( GameObject go, int colorId )
        {
            // Init the main fields for this cell.
            gameObject = go;
            transform = go.transform;
            //id = System.Convert.ToInt32( gameObject.name.Substring( 0, 1 ) );
            this.colorId = colorId;
            sRenderer = gameObject.GetComponent<SpriteRenderer>();
        }

        #endregion


        #region [Move Functions]
        
        /// <summary>
        /// Moves the cell left or right.
        /// </summary>
        /// <param name="x">The given target x position</param>
        /// <param name="time">The given time</param>
        /// <param name="onComplete">The given on complete action</param>
        public void MoveX( float x, float time, System.Action onComplete )
        {
            // Create a new tween and assign to the _moveXTween so we can manipulate it later on.
            _moveXTween = LeanTween.moveX( gameObject, x, time );
            _moveXTween.setOnComplete( onComplete );
        }

        /// <summary>
        /// Moves the cell down.
        /// </summary>
        /// <param name="y">The given target y position</param>
        /// <param name="time">The given time</param>
        /// <param name="onComplete">The given on complete action</param>
        public void MoveY( float y, float time, System.Action onComplete )
        {
            // Create a new tween and assign it to _moveYTween so we can manipulate it later on. 
            _moveYTween = LeanTween.moveY( gameObject, y, time );
            _moveYTween.setOnComplete( onComplete );
        }

        /// <summary>
        /// Increase the falling speed for this cell, when the player clicks on Down Button.
        /// </summary>
        public void SpeedUpMoveY()
        {
            if ( _moveYTween == null ) return;

            // Increase the speed of the current _moveYTween.
            _moveYTween.setSpeed( _moveYTween.speed * 2 );
            // Decrease the time of the current _moveYTween.
            _moveYTween.setTime( _moveYTween.time * 0.5f );
            // Set from value to the current Y position of the cell so we don't get any weird result.
            _moveYTween.setFrom( transform.position.y );
        }

        public void Freeze()
        {
            LeanTween.cancel( gameObject );
        }

        public void PauseTween()
        {
            if ( _moveXTween != null )
                _moveXTween.pause();

            if ( _moveYTween != null )
                _moveYTween.pause();
        }

        public void ResumeTween()
        {
            if ( _moveXTween != null )
                _moveXTween.resume();

            if ( _moveYTween != null )
                _moveYTween.resume();
        }

        public void ResumeMoveDownAfterRotation( float time, System.Action onComplete )
        {
            float max = Mathf.Max( transform.position.y, room.Position.y );
            float min = Mathf.Min( transform.position.y, room.Position.y );

            _moveYTween = LeanTween.moveY( gameObject, room.Position.y, time * ( max - min ) );
            _moveYTween.setOnComplete( onComplete );
        }

        #endregion


        #region [Set Funtions]

        /// <summary>
        /// Sets the current Cell in the given Room.
        /// </summary>
        /// <param name="room">The given room</param>
        /// <param name="forcePosition">Whether to set the position of the Cell as the Room position</param>
        public void SetInRoom( Room room, bool forcePosition = false )
        {
            // Assign the given Room to our _room.
            _room = room;

            // Check if we need to set the position of this Cell to the same position as the given room.
            if ( forcePosition )
                transform.position = room.Position;
        }

        public void SetAlpha( float alpha )
        {
            Color color = gameObject.GetComponent<SpriteRenderer>().color;
            gameObject.GetComponent<SpriteRenderer>().color = new Color( color.r, color.g, color.b, alpha );
        }

        public void SetPosition( Vector3 pos )
        {
            transform.position = pos;
        }

        public virtual void SetColor( Sprite sprite, int colorId )
        {
            this.colorId = colorId;
            sRenderer.sprite = sprite;
        }

        #endregion


        #region [Landing Effect Funtions]

        /// <summary>
        /// Applies a landing effect to this Cell.
        /// </summary>
        /// <param name="halfMoveVal">To check if this cell is on a top row or not</param>
        public void ApplyLandingEffect( bool halfMoveVal = true, System.Action onComplete = null )
        {
            float time = 0.2f;
            float val = 0.2f;

            // If this cell is on a top row (there is another cell on beneath of it within the same block)
            // move down this cell to the given val otherwise to the half of it. 
            float moveVal = halfMoveVal ? val * 0.5f : val;
            
            var tween = LeanTween.scaleY( gameObject, transform.localScale.x - val, time );
            tween.setOnComplete( ()=> { LandingEffect2( time, val, moveVal, onComplete ); } );

            LeanTween.scaleX( gameObject, transform.localScale.x + val, time );
            LeanTween.moveY( gameObject, transform.position.y - moveVal, time );
        }

        /// <summary>
        /// The 2nd step of the Landing effect which returns the Cell to its original landing scale and position.
        /// </summary>
        /// <param name="time">The given time</param>
        /// <param name="val">The given scale value</param>
        /// <param name="moveVal">The given move value</param>
        private void LandingEffect2( float time, float val, float moveVal, System.Action onComplete = null )
        {
            var tween = LeanTween.scaleY( gameObject, 1.0f, time );
            tween.setEaseOutBounce();
            tween.setOnComplete( onComplete );

            LeanTween.scaleX( gameObject, 1.0f, time );
            LeanTween.moveY( gameObject, transform.position.y + moveVal, time );
        }

        #endregion


        public virtual void FreeMemory()
        {
            _moveXTween = null;
            _moveYTween = null;

            GameObject.Destroy( gameObject );
        }
    }
}