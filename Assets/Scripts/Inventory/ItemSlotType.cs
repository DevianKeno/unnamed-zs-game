using System;
using UnityEngine;
using UZSG.Items;

namespace UZSG.Inventory
{
    [Flags]
    public enum ItemSlotType
    {
        All = 1,
        Item = 2,
        Tool = 4,
        Weapon = 8,
        Equipment = 16,
        Accessory = 32
    }
}