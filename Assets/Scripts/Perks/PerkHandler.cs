using UnityEngine;

using UZSG.Data.Perks;
using UZSG.Entities;

namespace UZSG.Perks
{
    public abstract class PerkHandler : MonoBehaviour
    {
        [SerializeField] protected PerkData perkData;
        public PerkData PerkData => perkData;
        [SerializeField] protected Entity entity;
        public Entity Entity => entity;

        bool _isActive;
        public bool IsActive => _isActive;
    }
}