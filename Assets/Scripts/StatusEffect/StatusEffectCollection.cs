using System.Collections.Generic;
using System.Linq;
using UZSG.Entities;

namespace UZSG.StatusEffects
{
    public class StatusEffectCollection
    {
        public Entity Entity;
        
        Dictionary<string, StatusEffect> statusEffectsDict = new();

        public bool Any
        {
            get
            {
                return statusEffectsDict.Any();
            }
        }
        
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