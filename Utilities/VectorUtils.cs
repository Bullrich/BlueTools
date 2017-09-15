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
        
        public static float AngleBetween(Vector3 v1, Vector3 v2)
        {
            float dot = Vector3.Dot(v1, v2);
            float theta = (float) Mathf.Acos(dot / (v1.magnitude * v2.magnitude));
            return theta;
        }
    }
}