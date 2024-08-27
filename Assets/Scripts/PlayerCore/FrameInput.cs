using UnityEngine;

namespace UZSG.Players
{
    public struct FrameInput
    {
        public Vector2 Move { get; set; }
        public bool HasPressedJump { get; set; }
        public bool HasPressedInteract { get; set; }
        public bool HasPressedInventory { get; set; }
    }
}