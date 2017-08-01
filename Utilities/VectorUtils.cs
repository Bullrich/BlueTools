using UnityEngine;

namespace Blue.Utility
{
    ///<summary>Made by Martin Wein</summary>
    public static class VectorUtils
    {
        public static Vector3 Truncate(Vector3 vec, float maxMagnitude)
        {
            var magnitude = vec.magnitude;
            return vec * Mathf.Min(1f, maxMagnitude / magnitude);
        }

        //http://mathworld.wolfram.com/SpherePointPicking.html
        public static Vector3 RandomDirection()
        {
            var theta = Random.Range(0f, 2f * Mathf.PI);
            var phi = Random.Range(0f, Mathf.PI);
            var u = Mathf.Cos(phi);
            return new Vector3(Mathf.Sqrt(1 - u * u) * Mathf.Cos(theta), Mathf.Sqrt(1 - u * u) * Mathf.Sin(theta), u);
        }
    }
}