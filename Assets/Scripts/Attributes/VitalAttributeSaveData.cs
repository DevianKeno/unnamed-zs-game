using System;

namespace UZSG.Attributes
{
    [Serializable]
    public class VitalAttributeSaveData : AttributeSaveData
    {
        public bool AllowChange = true;
        public int ChangeType = (int) VitalAttributeChangeType.Static;
        public int TimeCycle = (int) VitalAttributeTimeCycle.Second;
        public float BaseChange = 0f;
        public float ChangeMultiplier = 1f;
        public float ChangeFlatBonus = 0f;
        public bool EnableDelayedChange = false;
        public float DelayedChangeDuration = 0f;
    }
}