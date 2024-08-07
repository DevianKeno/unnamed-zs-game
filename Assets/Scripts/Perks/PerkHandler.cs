using UnityEngine;
using UZSG.Data.Perks;
using UZSG.Entities;

namespace UZSG.Perks
{
    public abstract class PerkHandler : MonoBehaviour
    {
        [SerializeField] protected PerkData perkData;
        public PerkData PerkData => perkData;
        [SerializeField] protected Entity actor;
        public Entity Actor => actor;
    }

    public class CombatMomentum : PerkHandler
    {
        
    }
}