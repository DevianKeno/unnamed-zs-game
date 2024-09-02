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
        public bool IsGunVariantDisplayed { get; private set; }
        public bool IsMeleeVariantDisplayed { get; private set; }
        
        [SerializeField] GameObject gunVariant;
        [SerializeField] GameObject meleeVariant;

        public void SetGunVariant()
        {
            meleeVariant.gameObject.SetActive(false);
            IsMeleeVariantDisplayed = false;

            gunVariant.gameObject.SetActive(true);
            IsGunVariantDisplayed = true;
        }

        public void SetMeleeVariant()
        {
            gunVariant.gameObject.SetActive(false);
            IsGunVariantDisplayed = false;

            meleeVariant.gameObject.SetActive(true);
            IsMeleeVariantDisplayed = true;
        }
    }
}