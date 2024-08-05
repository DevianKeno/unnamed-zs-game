using System;

namespace UZSG.Attributes
{
    [Serializable]
    public class VitalAttributeSaveData : AttributeSaveData
    {
        public bool AllowChange = true;
        public VitalAttributeChangeType ChangeType = VitalAttributeChangeType.Static;
        public VitalAttributeTimeCycle TimeCycle = VitalAttributeTimeCycle.Second;
        public float BaseChange = 0f;
        public float ChangeMultiplier = 1f;
        public float ChangeFlatBonus = 0f;
        public bool EnableDelayedChange = false;
        public float DelayedChangeDuration = 0f;
    }
}