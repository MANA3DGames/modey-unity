using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MANA3DGames
{
    public class RotatorScript : MonoBehaviour 
    {
        Transform _transform;

        public int direction;
        int speed;

        private void Awake()
        {
            _transform = transform;
            speed = Random.Range( 150, 200 );
        }

        private void Update()
        {
            _transform.Rotate( 0, 0, direction * Time.deltaTime * speed );
        }
    }
}
