using System;

using UnityEngine;
using UnityEngine.AddressableAssets;

using UZSG.Items.Armors;

namespace UZSG.Data
{
    public enum ArmorPiece { Headgear, Chestpiece, Legwear, Footwear };
    public enum ArmorCategory { Light, Medium, Heavy, Specialized };

    /// <summary>
    /// Represents the various data an Armor has.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "New Armor Data", menuName = "UZSG/Items/Armor Data")]
    public class ArmorData: ItemData
    {
        [Header("Armor Data")]
        public ArmorPiece Piece;
        public ArmorCategory Category;
        public ArmorAttributes Attributes;

        // To do: Add model handler for 3D player itself
    }
}
