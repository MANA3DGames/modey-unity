using System.Collections;
using UnityEngine;

namespace MANA3DGames
{
    public class Startup : MonoBehaviour
    {
        IEnumerator Start()
        {
            yield return new WaitForSeconds( 0.1f );
            UnityEngine.SceneManagement.SceneManager.LoadScene( "Game" );
        }
    }
}
