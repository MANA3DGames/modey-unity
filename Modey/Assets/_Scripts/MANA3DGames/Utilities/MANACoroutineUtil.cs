using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MANA3DGames.Utilities.Coroutine
{
    public class CoroutineCoordinator : MonoBehaviour
    {
        private static CoroutineCoordinator _instance = null;


        #region public properties

        // ***************************************************************
        // Instance of CoroutineCoordinator.
        // Singleton pattern to have only one instance for the whole game.
        // ***************************************************************
        public static CoroutineCoordinator Instance
        {
            get
            {
                // Check if we don't have an instance of the class.
                if (!_instance)
                {
                    // Check if an CoroutineCoordinator is already available in the scene.
                    _instance = FindObjectOfType(typeof(CoroutineCoordinator)) as CoroutineCoordinator;

                    // Create a new one if there is none.
                    if (!_instance)
                    {
                        // Create an enmpty gameobject.
                        GameObject obj = new GameObject("_CoroutineCoordinator");

                        // Add CoroutineCoordinator script to the empty gameobject.
                        _instance = obj.AddComponent<CoroutineCoordinator>();
                    }
                }

                // Return the instance.
                return _instance;
            }
        }


        #endregion


        #region MonoBehaviour

        void OnApplicationQuit()
        {
            // Release reference on exist.
            _instance = null;
        }


        #endregion

    }
	public class CoroutineTask
	{
		#region public Actions
		
		public event System.Action<bool> onCoroutineTaskComplete;
		
		
		#endregion
		
		
		#region public properties

        public bool IsRunning { get { return !_isPaused; } }
		public bool IsPaused { get { return _isPaused; } }
		
		
		#endregion
		
		
		#region Private variables
		
		private bool _isAlive;								    // Indicates that the coroutine is runnning.
		private bool _isPaused;									// Indicates that the coroutine is paused.
		private bool _coroutineTaskWasKilled;					// Indicates the the coroutine was stopped (terminated).
		
		private IEnumerator _coroutine;							// Running coroutine.
		private Stack<CoroutineTask> _subCoroutineTaskStack;	// Stack of all subcoroutine tasks.
		
		
		#endregion
		
		
		
		#region Private Funtions
		
		// ***************************************************************
		// executeTask function.
		// Executes current coroutine.
		// ***************************************************************
		private IEnumerator executeTask()
		{
			// Null out the first run through in the case we start paused.
			yield return null;
			
			// Execute the task as long as its status is running. 
			while ( _isAlive )
			{
				// Check if the task is paused.
				if ( _isPaused )
				{
					yield return null;
				}
				// The task is still running.
				else
				{
					// Run the next iteration and stop if we are done.
					if ( _coroutine.MoveNext() )
					{
						yield return _coroutine.Current;
					}
					else
					{
						// Run subCoroutine tasks if we have any.
						if ( _subCoroutineTaskStack != null )
							yield return CoroutineCoordinator.Instance.StartCoroutine( executeSubTask() );
						
						_isAlive = false;
					}
				}
			}
			
			// Fire off a complete task event (action).
			if ( onCoroutineTaskComplete != null )
				onCoroutineTaskComplete( _coroutineTaskWasKilled );
		}
		
		
		// ***************************************************************
		// executeSubTask function.
		// Executes current sub-coroutine tasks.
		// ***************************************************************
		private IEnumerator executeSubTask()
		{
			// Check if there is any sub-coroutines in the stack.
			if ( _subCoroutineTaskStack != null && _subCoroutineTaskStack.Count > 0 )
			{
				do
				{
					// Get first sub-task.
					CoroutineTask subTask = _subCoroutineTaskStack.Pop();
					
					// Execute the sub task.
					yield return CoroutineCoordinator.Instance.StartCoroutine( subTask.startAsCoroutine() );
				}
				// Keep executing the sub tasks as long as there is any.
				while( _subCoroutineTaskStack.Count > 0 );
			}
		}
		
		
		
		
		#endregion
		
		
		#region Public Constructors
		
		public CoroutineTask( IEnumerator coroutine ) : this( coroutine, true )
		{
			// ...
		}
		
		
		public CoroutineTask( IEnumerator coroutine, bool shouldStart )
		{
			// Set the current coroutine.
			_coroutine = coroutine;
			
			// Check if it should start immediately.
			if ( shouldStart )
				start();
		}
		
		
		#endregion
		
		
		#region public Functions
		
		// ***************************************************************
		// createAndAddSubCoroutineTask function.
		// ***************************************************************
		public CoroutineTask createAndAddSubCoroutineTask( IEnumerator coroutine )
		{
			var j = new CoroutineTask( coroutine, false );
			addSubCoroutineTask( j );
			return j;
		}
		
		
		// ***************************************************************
		// addSubCoroutineTask function.
		// ***************************************************************
		public void addSubCoroutineTask( CoroutineTask subCoroutineTask )
		{
			if ( _subCoroutineTaskStack == null )
				_subCoroutineTaskStack = new Stack<CoroutineTask>();
			_subCoroutineTaskStack.Push( subCoroutineTask );
		}
		
		
		// ***************************************************************
		// removeSubCoroutineTask function.
		// ***************************************************************
		public void removeSubCoroutineTask( CoroutineTask subCoroutineTask )
		{
			if ( _subCoroutineTaskStack.Contains( subCoroutineTask ) )
			{
				var subCoroutineTaskStack = new Stack<CoroutineTask>( _subCoroutineTaskStack.Count - 1 );
				var allCurrentSubTasks = _subCoroutineTaskStack.ToArray();
				System.Array.Reverse( allCurrentSubTasks );
				
				for ( var i = 0; i < allCurrentSubTasks.Length; i++ )
				{
					var j = allCurrentSubTasks[i];
					if ( j != subCoroutineTask )
						subCoroutineTaskStack.Push( j );
				}
				
				// assign the new stack
				_subCoroutineTaskStack = subCoroutineTaskStack;
			}
		}
		
		
		// ***************************************************************
		// start function.
		// ***************************************************************
		public void start()
		{
			_isAlive = true;
			CoroutineCoordinator.Instance.StartCoroutine( executeTask() );
		}
		
		
		// ***************************************************************
		// startAsCoroutine function.
		// ***************************************************************
		public IEnumerator startAsCoroutine()
		{
			_isAlive = true;
			yield return CoroutineCoordinator.Instance.StartCoroutine( executeTask() );
		}
		
		
		// ***************************************************************
		// pause function.
		// ***************************************************************
		public void pause()
		{
			_isPaused = true;
		}
		
		
		// ***************************************************************
		// unPause function.
		// ***************************************************************
		public void unPause()
		{
			_isPaused = false;
		}
		
		
		// ***************************************************************
		// kill function.
		// ***************************************************************
		public void kill()
		{
			_coroutineTaskWasKilled = true;
			_isAlive = false;
			_isPaused = false;
		}
		
		
		// ***************************************************************
		// kill function.
		// ***************************************************************
		public void kill( float delayInSeconds ) 
		{
			var delay = (int)( delayInSeconds * 1000 );
			
			new System.Threading.Timer( obj => 
			                           {
				lock( this )
				{
					kill();
				}
			}, null, delay, System.Threading.Timeout.Infinite );
			
		}
		
		
		#endregion
		
		
		#region Public Static Functions.
		
		// ***************************************************************
		// create function.
		// Creates a task with a coroutine.
		// ***************************************************************
		public static CoroutineTask create( IEnumerator coroutine )
		{
			// Call a proper constructor.
			return new CoroutineTask( coroutine );
		}
		
		// ***************************************************************
		// create function.
		// Creates a task with a coroutine/ Boolean Instrcution to start.
		// ***************************************************************
		public static CoroutineTask create( IEnumerator coroutine, bool shouldStart )
		{
			// Call a proper constructor.
			return new CoroutineTask( coroutine, shouldStart );
		}
		
		
		#endregion
		
	}
}
