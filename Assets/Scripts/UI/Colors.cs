using UnityEngine;

namespace URMG.UI.Colors
{
    public class Colors
    {
        public static Color ToTransparent(Color color)
        {
            return new(color.r, color.g, color.b, 0f);
        }
        
        public static Color ToOpaque(Color color)
        {
            return new(color.r, color.g, color.b, 1f);
        }
    }
}