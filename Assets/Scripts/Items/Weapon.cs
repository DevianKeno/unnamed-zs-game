using System;

using UnityEngine;

using UZSG.Data;

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
        protected ItemData weaponData;
        
        public Weapon(ItemData itemData) : base(itemData, 1) /// 1, since Weapons are not stackable
        {
            weaponData = (ItemData) itemData;
        }
    }
}
