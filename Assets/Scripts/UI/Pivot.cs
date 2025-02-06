using System;

using UnityEngine;
using UnityEngine.UI;



namespace UZSG.UI
{
    /// <summary>
    /// Helper class for RectTransform pivot values.
    /// </summary>
    public class Pivot
    {
        public static Vector2 TopLeft => new (0, 1);
        public static Vector2 TopMiddle => new (0.5f, 1);
        public static Vector2 TopRight => new (1, 1);
        public static Vector2 MiddleLeft => new (0, 0.5f);
        public static Vector2 Center => new (0.5f, 0.5f);
        public static Vector2 MiddleRight => new (1, 0.5f);
        public static Vector2 BottomLeft => new (0, 0);
        public static Vector2 BottomMiddle => new (0.5f, 0);
        public static Vector2 BottomRight => new (1, 0);
    }
}
