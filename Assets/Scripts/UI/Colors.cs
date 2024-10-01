using UnityEngine;

namespace UZSG.UI.Colors
{
    public class Colors
    {
        public static Color Opaque => new(1f, 1f, 1f, 1f);
        public static Color Transparent => new(1f, 1f, 1f, 0f);

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