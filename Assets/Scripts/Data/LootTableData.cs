using System;

using UnityEngine;

using UZSG.Items;

namespace UZSG.Data
{
    [Serializable]
    public struct LootItem
    {
        public ItemData ItemData;
        public int MinAmount;
        public int MaxAmount;
        [Range(0, 1)] public float Weight;
    }

    /// <summary>
    /// Loot table data.
    /// Values are set in Inspector; <b>Do not write</b>.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "New Loot Table", menuName = "UZSG/Loot Table")]
    public class LootTableData : BaseData
    {
        public int Rolls;
        public LootItem[] Items;
        public Item[] GuaranteedItems;
    }
}