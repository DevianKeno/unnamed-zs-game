using System;
using UnityEditor.Animations;
using UnityEngine;

namespace UZSG.Items
{
    [Serializable]
    public struct FPPWeaponAnimations
    {
        public string Equip;
        public string Idle;
        public string Run;
        public string[] Primary;
        public string Secondary;
        public string Hold;

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
    public class WeaponData : ItemData
    {
        [Header("Weapon Attributes")]
        public WeaponAttributes Attributes;
        [SerializeField] GameObject _FPPModel;
        public GameObject FPPModel => _FPPModel;

        [Header("Animations")]
        [SerializeField] AnimatorController _controller;
        public AnimatorController Controller => _controller;
        public FPPWeaponAnimations AnimNames;
    }
}
