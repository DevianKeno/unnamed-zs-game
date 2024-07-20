using System;

using UnityEngine;

using UZSG.Items.Weapons;

namespace UZSG.FPP
{
    [Serializable]
    public struct Viewmodel
    {
        [field: SerializeField] public GameObject Arms { get; set; }
        [field: SerializeField] public GameObject Weapon { get; set; }
        [field: SerializeField] public WeaponData WeaponData { get; set; }
    }
}