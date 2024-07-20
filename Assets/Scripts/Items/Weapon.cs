using System;

using UnityEngine;

namespace UZSG.Items.Weapons
{
    public class Tool
    {
        public GameObject FPPModel => throw new NotImplementedException();
    }

    /// <summary>
    /// Represents a Weapon as an Item.
    /// </summary>
    public class Weapon : Item
    {
        protected WeaponData weaponData;
        
        public Weapon(ItemData itemData) : base(itemData, 1) /// 1, since Weapons are not stackable
        {
            weaponData = (WeaponData) itemData;
        }
    }
}
