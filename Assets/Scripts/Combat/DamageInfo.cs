using System;

using UZSG.Entities;

namespace UZSG
{
    [Serializable]
    public struct DamageInfo
    {
        public IDamageSource Source { get; set;}
        public float Amount { get; set; }

        public DamageInfo(IDamageSource source, float amount)
        {
            this.Source = source;
            this.Amount = amount;
        }
    }
}
