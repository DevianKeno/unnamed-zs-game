using System;

using UnityEngine;

using UZSG.Data;

namespace UZSG.Data.Perks
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Perk Data", menuName = "UZSG/Perk Data")]
    public class PerkData : BaseData
    {
        [Header("Perk Attributes")]
        public string Name;
        [TextArea] public string Description;
        public Sprite Sprite;
    }
}