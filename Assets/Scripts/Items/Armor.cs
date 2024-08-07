using System;

using UnityEngine;

using UZSG.Data;

namespace UZSG.Items.Armors
{
    public class Armor: Item
    {
        protected ItemData armorData;

        public Armor(ItemData itemData) : base(itemData, 1) /// 1, since Armors are not stackable
        {
            armorData = (ItemData) itemData;
        }
    }
}
