using System;

namespace UZSG.Attributes
{
    [Serializable]
    public class AttributeSaveData
    {
        public string Id;
        public int Type = 0;
        public float Value = 0f;
        public float Minimum = 0f;
        public float BaseMaximum = 0f;
        public float Multiplier = 1f;
        public float FlatBonus = 0f;
        public float SiteRadius = 0f;
        public bool LimitOverflow = true;
        public bool LimitUnderflow = true;
    }
}