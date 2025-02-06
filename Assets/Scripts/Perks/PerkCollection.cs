using System.Collections.Generic;
using UnityEngine;


using UZSG.Data.Perks;
using UZSG.Entities;

namespace UZSG.Perks
{
    public class PerkCollection
    {
        List<PerkHandler> _perks = new();
        Dictionary<string, PerkHandler> _perksDict = new();

        public bool IsActive(string id)
        {
            return false;
        }
        
        public void AddPerk(PerkHandler perk)
        {
            if (_perksDict.ContainsKey(perk.PerkData.Id))
            {
                Game.Console.LogInfo($"Perk {perk.PerkData.Id} already exists within the Perk Collection.");
                return;
            }

            _perksDict[perk.PerkData.Id] = perk;
        }
                
        public void RemovePerk(PerkHandler perk)
        {
            
        }
        
        public PerkHandler GetPerk(string id)
        {
            if (_perksDict.TryGetValue(id, out var perk))
            {
                return perk;
            }

            return null;
        }
        
        public bool TryGetPerk(string id, out PerkHandler perk)
        {
            if (_perksDict.TryGetValue(id, out perk))
            {
                return true;
            }

            return false;
        }
    }
}