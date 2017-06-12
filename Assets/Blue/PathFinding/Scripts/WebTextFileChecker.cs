using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//Got it from http://answers.unity3d.com/questions/27148/how-to-read-text-file-txt-from-url.html

namespace Blue.Pathfinding
{
    /// <summary>WebTextFileChecker.cs
    /// <para>This checks the text in a text file on the web - believe it!</para></summary>
    public class WebTextFileChecker : MonoBehaviour
    {
        public void Start()
        {
            StartCoroutine(Check());
        }

        private IEnumerator Check()
        {
            WWW w = new WWW("jbullrich.com.ar/right.txt");
            yield return w;
            if (w.error != null)
            {
                Debug.Log("Error .. " + w.error);
            }
            else
            {
                Debug.Log("Found ... ==>" + w.text + "<==");
            }
        }
    }
}