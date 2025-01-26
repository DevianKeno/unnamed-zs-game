using System;

namespace UZSG.Saves
{
    [Serializable]
    public class AttributeSaveData : SaveData
    {
        public string Id = string.Empty;
        public float Value = 0f;
        public float Minimum = 0f;
        public float BaseMaximum = 0f;
        public float Multiplier = 1f;
        public float FlatBonus = 0f;
        public bool LimitOverflow = true;
        public bool LimitUnderflow = true;
    }
}