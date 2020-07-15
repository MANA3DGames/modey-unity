using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MANA3DGames.Utilities.Coroutine;

namespace MANA3DGames
{
    public class AudioLibrary
    {
        public static readonly string[] BGMs = { "happy-children",
                                                 "happy-days-are-here-nonloop-2m03s",
                                                 "happy-breakthrough-underscore-1m21s",
                                                 "happy-and-smilig",
                                                 "family-holiday-nonloop-0m59s" };

        public class UI
        {
            public const string Click = "ui_menu_button_beep_12";
            public const string Back = "ui_menu_button_beep_19";
            public const string Cancel = "ui_menu_button_cancel_02";
            public const string TweenLeftRight = "ui_menu_button_scroll_back_04";
            public const string TweenUpDown = "ui_menu_button_scroll_whoosh_01";
            public const string TweenUp = "whistle_slide_up_01";
            //public const string TweenDown = "whistle_slide_down_01";
            public static readonly string[] Scaled = { "ui_menu_popup_message_01",
                                                        "ui_menu_popup_message_03",
                                                        "ui_menu_popup_message_04",
                                                        "ui_menu_button_beep_17" };
            public const string MessagePositive = "ui_menu_popup_message_02";
            //public const string MessageNegative = "ui_menu_button_error_message_01";
        }

        public class ModeySFX
        {
            public static readonly string[] BeforeSaved = { "voice_fun_character_vocal_crazy_04",
                                                            "voice_fun_small_character_emote_happy_03",
                                                            "voice_fun_small_character_emote_happy_04",
                                                            "voice_fun_small_character_emote_happy_05",
                                                            "voice_fun_small_character_emote_surprised_02" };

            public static readonly string[] WhileSaving = { "voice_fun_character_flying_cartoon_02",
                                                            "voice_fun_character_flying_cartoon_01" };

            public const string OnDestroyed = "voice_fun_ant_creature_01";	
            
            public static readonly string[] Idle = { "voice_fun_small_character_emote_interested_01",
                                                     "voice_funny_cartoon_misc_05",
                                                     "voice_funny_cartoon_misc_01" }; 	
		    
            public const string MoveDown = "cartoon_boing_jump_01";	
					
			public const string MoveDownAnotherColor = "voice_fun_small_character_emote_surprised_01";
        }

        public class CompleteRow
        {
            public const string OnDestroyedSameColor = "chime_tinkle_wood_bell_positive_02";	
            public const string OnDestroyedDiffColors = "chime_tinkle_wood_bell_positive_01";
            public const string Reward = "collect_item_05";
        }

        public class BlockSFX
        {
            public const string LoseWarning = "alarm_siren_loop_07";

            public static readonly string[] SpawnBlock = { "ui_menu_button_beep_12",
                                                           "ui_menu_button_beep_25" };

            public static readonly string[] SwipeLeftRight = { "whoosh_swish_small_harsh_01",
                                                                "ui_menu_button_scroll_whoosh_01" };

            public const string PushDown = "ui_menu_button_scroll_whoosh_01";

            public const string FastPushDown = "whoosh_swish_high_big_01";

            public const string Landing = "ui_menu_button_beep_24";

            public static readonly string[] Rotate = { "collect_item_04", "collect_item_13" };
        }

        public class CellSFX
        {
            public static readonly string[] OnRowDestroyed = { "snowball_hit_impact_hard_01",
                                                               "snowball_hit_impact_hard_02",
                                                               "snowball_hit_impact_hard_03" };

            public static readonly string[] OnDestroyedByBomb = { "punch_heavy_huge_distorted_01",
                                                                  "punch_heavy_huge_distorted_02",
                                                                  "punch_heavy_huge_distorted_03",
                                                                  "punch_heavy_huge_distorted_04" };

            public const string LandingDown = "whistle_slide_up_03";
        }

		public class BoostersSFX
        {
            public const string OnPressBomb = "collect_item_hurry_out_of_time_01";
            public const string OnPressBurner = "collect_item_hurry_out_of_time_01";
            public const string OnPressCustomBlock = "collect_item_02";
            public const string SpawnNewCustomBlock = "collect_item_03";
            public const string Error = "voice_fun_character_vocal_crazy_02";
        }
		
        public class PainterSFX
        {
            public const string OnPressPainter = "collect_item_01";
            public const string Error = "voice_fun_character_vocal_crazy_02";
        }	

        public class GameOver
        {
            public const string WinLevel = "ui_menu_popup_message_reward_01";

            public static readonly string[] StarsShow = { "chime_bell_positive_ring_01",
                                                            "chime_bell_positive_ring_02",
                                                            "chime_bell_positive_ring_03" };

            public static readonly string[] CoinsReward = { "ui_menu_button_beep_17",
                                                            "points_ticker_bonus_score_reward_single_01"  };

            public const string BoosterReward = "ui_menu_button_beep_18";
        }
    }

    public class AudioManager
    {
        AudioSource[] _audioSoruces;
        AudioSource _bgmSource;

        Dictionary<string, AudioClip> _clips;

        Vector3 _camPos;

        CoroutineTask _bgmTask;

        GameManager _gameManager;


        public AudioManager( Transform audioSourceRoot, GameManager manager )
        {
            _gameManager = manager;

            audioSourceRoot.gameObject.SetActive( true );

            _audioSoruces = audioSourceRoot.GetComponents<AudioSource>();
            _bgmSource = audioSourceRoot.GetComponent<AudioSource>();

            var tempClips = Resources.LoadAll<AudioClip>( "Audio" );
            _clips = new Dictionary<string, AudioClip>();
            foreach ( var clip in tempClips )
                _clips.Add( clip.name, clip );

            _camPos = Camera.main.transform.position;
        }


        IEnumerator BGMCoroutine()
        {
            List<int> indices = new List<int>( new int[] { 1, 2, 3, 4 } );

            _bgmSource.volume = 0.4f;
            _bgmSource.clip = _clips[AudioLibrary.BGMs[0]];
            _bgmSource.Play();

            while ( true )
            {
                yield return new WaitForSeconds( 1.0f );
                if ( !_bgmSource.isPlaying )
                {
                    if ( indices.Count == 0 )
                        indices = new List<int>( new int[] { 0, 1, 2, 3, 4 } );

                    int index = indices[UnityEngine.Random.Range( 0, indices.Count-1 )];
                    indices.Remove( index );

                    _bgmSource.Stop();
                    _bgmSource.clip = _clips[AudioLibrary.BGMs[index]];
                    _bgmSource.Play();
                }
            }
        }

        public void PlayBGM()
        {
            if ( _bgmTask != null )
            {
                _bgmTask.kill();
                _bgmTask = null;
            }
            _bgmTask = new CoroutineTask( BGMCoroutine() );
        }


        public void Play( string name, float val = 1.0f )
        {
            if ( !_gameManager.gameSettings.sfx ) return;
            AudioSource.PlayClipAtPoint( _clips[name], _camPos, val );
        }

        public void PlayAt( string name, Vector3 pos, float val = 1.0f )
        {
            if ( !_gameManager.gameSettings.sfx ) return;
            AudioSource.PlayClipAtPoint( _clips[name], pos, val );
        }

        public void Play2D( string name, float val = 1.0f )
        {
            if ( !_gameManager.gameSettings.sfx ) return;

            foreach ( var audio in _audioSoruces )
            {
                if ( !audio.isPlaying )
                {
                    audio.PlayOneShot( _clips[name], val );
                    break;
                }
            }
        }

        public float GetClipLength( string name )
        {
            return _clips[name].length;
        }


        public void EnableBGM( bool enable )
        {
            _bgmSource.mute = !enable;
        }
    }
}
