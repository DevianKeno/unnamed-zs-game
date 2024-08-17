using System;

using UnityEngine;

using UZSG.Entities;
using UZSG.Items;
using UZSG.Items.Weapons;
using UZSG.Systems;

namespace UZSG.UI.HUD
{
    public class WeaponDetailsUI : MonoBehaviour
    {
        [SerializeField] GameObject gunVariant;
        [SerializeField] GameObject meleeVariant;

        public void SetGunVariant()
        {
            meleeVariant.gameObject.SetActive(false);

            gunVariant.gameObject.SetActive(true);
        }

        public void SetMeleeVariant()
        {
            gunVariant.gameObject.SetActive(false);

            meleeVariant.gameObject.SetActive(true);
        }
    }
}