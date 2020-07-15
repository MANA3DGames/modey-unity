using System.Collections;
using UnityEngine;
using MANA3DGames.Utilities.Coroutine;

namespace MANA3DGames
{
    public class Face : Cell
    {
        Transform eye0;
        Transform eye1;

        GameObject eyelash0;
        GameObject eyelash1;
        GameObject mouth;

        float speed = 4.0f;

        CoroutineTask _blinkTask;
        CoroutineTask _mouthTask;

        Color[] colors = { new Color( 0.2f, 0.75f, 1.0f ),
                           new Color( 1.0f, 0.3f, 0.3f ),
                           new Color( 0.2f, 1.0f, 0.75f ),
                           new Color( 0.2f, 0.5f, 0.7f ),
                           new Color( 1.0f, 0.75f, 0.0f ) };

        LevelManager _levelManager;


        public Face( GameObject go, int ColorId, LevelManager levelManager ) : base( go, ColorId )
        {
            _levelManager = levelManager;

            eye0 = transform.Find( "Eye0" ).GetChild( 0 );
            eye1 = transform.Find( "Eye1" ).GetChild( 0 );

            eyelash0 = transform.Find( "Eye0" ).GetChild( 1 ).gameObject;
            eyelash1 = transform.Find( "Eye1" ).GetChild( 1 ).gameObject;

            mouth = transform.Find( "Mouth" ).gameObject;

            Inhalation();

            _blinkTask = new CoroutineTask( Blink(), true );
            _mouthTask = new CoroutineTask( OMouth(), true );

            isFace = true;
        }

        public void LookAtBlock( Block block )
        {
            if ( block == null || 
                 block.GetCenterCell() == null || 
                 block.GetCenterCell().transform == null )
                return;

            Vector3 lookAtPos = ( block.GetCenterCell().transform.position - transform.position ) * 0.0125f;

            eye0.localPosition = Vector3.Lerp( eye0.localPosition, lookAtPos, Time.deltaTime * speed );
            eye1.localPosition = Vector3.Lerp( eye1.localPosition, lookAtPos, Time.deltaTime * speed );

            float eye0_x = Mathf.Clamp( eye0.localPosition.x, -0.05f, 0.05f );
            float eye0_y = Mathf.Clamp( eye0.localPosition.y, -0.05f, 0.05f );

            float eye1_x = Mathf.Clamp( eye1.localPosition.x, -0.05f, 0.05f );
            float eye1_y = Mathf.Clamp( eye1.localPosition.y, -0.05f, 0.05f );

            eye0.localPosition = new Vector3( eye0_x, eye0_y, eye0.localPosition.z );
            eye1.localPosition = new Vector3( eye1_x, eye1_y, eye1.localPosition.z );
        }



        void Inhalation()
        {
            var tween = LeanTween.scale( gameObject, new Vector3( 1.05f, 1.05f, 1.0f ), 2.0f );
            tween.setOnComplete( () => { Exhalation(); } );
        }

        void Exhalation()
        {
            var tween = LeanTween.scale( gameObject, Vector3.one, 2.0f );
            tween.setOnComplete( () => { Inhalation(); } );
        }


        IEnumerator Blink()
        {
            float time = 0.2f;
            float currentY = eyelash0.transform.localPosition.y;

            var tween0 = LeanTween.scale( eyelash0, new Vector3( 1.0f, 1.0f, 0.0f ), time );
            tween0.setOnComplete( () => 
            {
                LeanTween.scale( eyelash0, new Vector3( 1.0f, 0.0f, 0.0f ), time );
                LeanTween.moveLocalY( eyelash0, currentY, time );
            } );

            LeanTween.moveLocalY( eyelash0, 0.0f, time );


            var tween1 = LeanTween.scale( eyelash1, new Vector3( 1.0f, 1.0f, 0.0f ), time );
            tween1.setOnComplete( () => 
            {
                LeanTween.scale( eyelash1, new Vector3( 1.0f, 0.0f, 0.0f ), time );
                LeanTween.moveLocalY( eyelash1, currentY, time );
            } );

            LeanTween.moveLocalY( eyelash1, 0.0f, time );


            yield return new WaitForSeconds( Random.Range( 5, 7 ) );

            _blinkTask = new CoroutineTask( Blink(), true );
        }

        IEnumerator OMouth()
        {
            yield return new WaitForSeconds( Random.Range( 5, 15 ) );

            float time = 0.5f;

            float originalY = 0.1f;//mouth.transform.localScale.y;

            var tween0 = LeanTween.scaleX( mouth, 0.5f, time );
            tween0.setOnComplete( () => 
            {
                var tween21 = LeanTween.scaleX( mouth, 1.0f, time );
                tween21.setDelay( time * 4.0f );

                var tween22 =  LeanTween.scaleY( mouth, originalY, time );
                tween22.setDelay( time * 4.0f );
            } );

            LeanTween.scaleY( mouth, 0.6f, time );

            _levelManager.PlayAudio( AudioLibrary.ModeySFX.Idle[UnityEngine.Random.Range( 0, AudioLibrary.ModeySFX.Idle.Length )], 0.5f );

            _mouthTask = new CoroutineTask( OMouth(), true );
        }



        public void OnPauseGame()
        {
            if ( _blinkTask != null )
                _blinkTask.pause();

            if ( _mouthTask != null )
                _mouthTask.pause();
        }

        public void OnResumeGame()
        {
            if ( _blinkTask != null )
                _blinkTask.unPause();

            if ( _mouthTask != null )
                _mouthTask.unPause();
        }


        public override void SetColor( Sprite sprite, int colorId )
        {
            base.SetColor( sprite, colorId );
            eyelash0.GetComponent<SpriteRenderer>().color = colors[colorId];
            eyelash1.GetComponent<SpriteRenderer>().color = colors[colorId];
        }

        public override void FreeMemory()
        {
            base.FreeMemory();

            if ( _blinkTask != null )
            {
                _blinkTask.kill();
                _blinkTask = null;
            }

            if ( _mouthTask != null )
            {
                _mouthTask.kill();
                _mouthTask = null;
            }
        }

        public void SmileToCam()
        {
            transform.Find( "Free" ).gameObject.SetActive( true );
            eye0.gameObject.SetActive( false );
            eye1.gameObject.SetActive( false );
            mouth.SetActive( false );
        }

        public void IncreaseOrder( int val )
        {
            eye0.GetComponent<SpriteRenderer>().sortingOrder += val;
            eye1.GetComponent<SpriteRenderer>().sortingOrder += val;
            eyelash0.GetComponent<SpriteRenderer>().sortingOrder += val;
            eyelash1.GetComponent<SpriteRenderer>().sortingOrder += val;
            mouth.GetComponent<SpriteRenderer>().sortingOrder += val;
            gameObject.GetComponent<SpriteRenderer>().sortingOrder += val;
            transform.Find( "Eye0" ).GetComponent<SpriteRenderer>().sortingOrder += val;
            transform.Find( "Eye1" ).GetComponent<SpriteRenderer>().sortingOrder += val;
        }
    }
}
