using System.Collections.Generic;
using System.Linq;

using UZSG.Entities;
using UZSG.StatusEffects;

namespace UZSG
{
    public class StatusEffectCollection
    {
        public IStatusEffectAfflictable Afflicted;
        
        Dictionary<string, StatusEffect> statusEffectsDict = new();

        /// <summary>
        /// Whether if this collection contains any status effects.
        /// </summary>
        public bool Any => statusEffectsDict.Any();
        
        /// <summary>
        /// Check if afflicted with status effect.
        /// </summary>
        /// <returns></returns>
        public bool IsAfflictedWith(string id)
        {
            return statusEffectsDict.ContainsKey(id);
        }

        public void Afflict(string id, int level, int seconds)
        {

        }

        public void Clear(string id)
        {
            if (statusEffectsDict.ContainsKey(id))
            {
                /// handle removal
                statusEffectsDict.Remove(id);
            }
        }
    }
}