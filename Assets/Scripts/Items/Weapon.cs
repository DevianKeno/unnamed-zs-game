using System;
using UnityEngine;
using UZSG.FPP;
using UZSG.Items;

namespace UZSG
{
    public class Tool : IFPPVisible
    {
        public GameObject FPPModel => throw new NotImplementedException();
    }

    public class Weapon : IFPPVisible
    {
        WeaponData _data;
        public GameObject FPPModel => _data.FPPModel;

        public Weapon(WeaponData data)
        {
            _data = data;
        }
    }
}
