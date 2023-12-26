using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UZSG.FPP;

namespace UZSG.Items
{
    /// <summary>
    /// List of possible animations an FPP model have.
    /// </summary>
    [Serializable]
    public struct FPPAnimations : IEnumerable
    {
        public string Equip;
        public string Idle;
        public string Run;
        public string[] Primary;
        public string Secondary;
        public string Hold;

        public readonly string this[int i]
        {
            get
            {
                return "null";
            }
        }

        public readonly IEnumerator GetEnumerator()
        {
            List<string> s = new()
            {
                Equip,
                Idle,
                Run,
                Secondary,
                Hold
            };
            s.AddRange(Primary);
            return s.GetEnumerator();
        }

        /// <summary>
        /// Get a random animation.
        /// </summary>
        public readonly string GetRandomPrimary()
        {
            if (Primary.Length == 0) return null;
            // There's no actual randomness happening, fix
            return Primary[1];
        }
    }

    /// <summary>
    /// Represents the various data a Weapon has.
    /// </summary>
    [CreateAssetMenu(fileName = "Weapon", menuName = "URMG/Weapon")]
    public class WeaponData : ItemData, IFPPVisible
    {
        [Header("Weapon Attributes")]
        public WeaponAttributes Attributes;
        [SerializeField] GameObject _FPPModel;
        public GameObject FPPModel => _FPPModel;

        [Header("Animations")]
        [SerializeField] AnimatorController _controller;
        public AnimatorController Controller => _controller;
        public FPPAnimations Anim;
        public FPPAnimations Anims => Anim;
        private ItemData itemData;
        private int count;
        
        public static bool TryGetWeaponData(ItemData item, out WeaponData weaponData)
        {
            weaponData = item as WeaponData;
            return weaponData != null;
        }
    }
}
