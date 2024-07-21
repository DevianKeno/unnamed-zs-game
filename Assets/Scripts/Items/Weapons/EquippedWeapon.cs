using UnityEngine;
using UZSG.Entities;
using UZSG.Players;

namespace UZSG.Items.Weapons
{
    public abstract class EquippedWeapon : MonoBehaviour
    {
        Entity owner;
        public Entity Owner
        {
            get { return owner; }
            set { owner = value; }
        }
        [SerializeField] protected WeaponData weaponData;
        public WeaponData WeaponData => weaponData;
        public abstract void Initialize();
        public abstract void SetWeaponStateFromPlayerAction(ActionStates state);
    }
}
