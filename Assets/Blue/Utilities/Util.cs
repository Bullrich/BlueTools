using UnityEngine;

// By @Bullrich
// Some part obtained from http://mfyg.dk/c-sharp-extension-methods/

namespace Blue.Utility
{
    /// <summary>Class with several utilities for Unity</summary>
    public static class Util
    {
        /// <summary>Use as 'Vector3 dir = transform.position.DirectionTo(Vector3)'</summary>
        /// <param name="a">My position</param>
        /// <param name="b">Target position</param>
        public static Vector3 DirectionTo(this Vector3 a, Vector3 b)
        {
            return b - a;
        }

        /// <summary>Use as 'Color.white.WithAlpha(0.5f)'</summary>
        public static Color WithAlpha(this Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        public static Vector3 WithMagnitude(this Vector3 v, float magnitude)
        {
            return v.normalized * magnitude;
        }

        public static Vector3 WithY(this Vector3 v, float newY)
        {
            return new Vector3(v.x, newY, v.z);
        }

        public static Rect WithInsidePadding(this Rect rect, float padding)
        {
            rect.x += padding;
            rect.xMax -= padding * 2;
            rect.y += padding;
            rect.yMax -= padding * 2;

            return rect;
        }

        /* This one is a bit silly (a weird way of casting), but it shows that 
         * you can return other types than the type from which the method was called */
        public static int ToInt(this float n)
        {
            return (int) n;
        }
    }
}