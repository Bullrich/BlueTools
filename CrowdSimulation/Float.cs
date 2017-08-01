using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// by @Bullrich

namespace Blue.Crowd
{
    public class Float : MonoBehaviour
    {
        float originalY;
        public float
            amplitude = 5,
            speed = 20;

        void Start()
        {
            originalY = transform.position.y;
        }

        void Update()
        {
            Vector3 transformPos = transform.position;
            transformPos.y = originalY + amplitude * Mathf.Sin(speed * Time.time);
            transform.position = transformPos;
        }

    }
}
