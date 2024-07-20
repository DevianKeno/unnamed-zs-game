using System.Collections.Generic;

namespace UZSG.StatusEffects
{
    public class StatusEffectCollection
    {
        public List<StatusEffect> statusEffects = new();
        
        /// <summary>
        /// Check if afflicted with status effect.
        /// </summary>
        /// <returns></returns>
        public bool IsAfflictedWith()
        {
            return false;
        }
    }
}