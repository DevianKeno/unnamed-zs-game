using System;
using UnityEngine;
using UZSG.Items;

namespace UZSG
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
        WeaponData _data;

        public Weapon(ItemData itemData, int count) : base(itemData, count)
        {
            _data = (WeaponData) itemData;
        }
    }
}
