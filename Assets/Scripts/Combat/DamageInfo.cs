using System;

namespace UZSG
{
    [Serializable]
    public class DamageInfo
    {
        public IDamageSource Source { get; set; }
        public float Amount { get; set; }

        public DamageInfo(IDamageSource source, float amount)
        {
            this.Source = source;
            this.Amount = Math.Clamp(amount, 0f, amount); /// dealing negative damage is prohibited
        }
    }
}
