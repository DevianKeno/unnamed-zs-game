using System;
using UnityEngine;
using UZSG.FPP;
using UZSG.Items;

namespace UZSG
{
    public class Tool
    {
        public GameObject FPPModel => throw new NotImplementedException();
    }

    public class Weapon : Item
    {
        WeaponData _data;

        public Weapon(ItemData itemData, int count) : base(itemData, count)
        {
            _data = (WeaponData) itemData;
        }

        public GameObject FPPModel => _data.FPPModel;
    }
}
