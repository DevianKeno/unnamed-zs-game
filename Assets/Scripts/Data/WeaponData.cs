using System;

using UnityEngine;
using UnityEditor.Animations;
using UnityEngine.AddressableAssets;

using UZSG.FPP;
using UZSG.Items.Weapons;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace UZSG.Data
{
    public enum WeaponCategory { Melee, Ranged }
    public enum WeaponMeleeType { None, Blunt, Bladed }
    public enum WeaponBluntType { None, Bat, Hammer }
    public enum WeaponBladedType { None, Sword, Knife, Katana, Axe }
    public enum WeaponRangedType { None, Handgun, Shotgun, SMG, AssaultRifle, SniperRifle, MachineGun }

    /// <summary>
    /// Represents the various data a Weapon has.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "New Weapon Data", menuName = "UZSG/Items/Weapon Data")]
    public class WeaponData : ItemData, IViewmodel
    {
        #region Weapon Data
        public Sprite HotbarIcon;
        public WeaponCategory Category;
        public List<Attributes.Attribute> Attributes;
        public WeaponMeleeType MeleeType;
        public List<MeleeAttackParametersData> MeleeAttacks;
        public WeaponBluntType BluntType;
        public WeaponBladedType BladedType;
        public WeaponRangedType RangedType;
        /// These attributes refer to the Weapon's specifications and whatnot,
        /// and are different from the Attributes above
        public WeaponMeleeAttributes MeleeAttributes;
        public WeaponRangedAttributes RangedAttributes;

        #endregion

        
        #region Viewmodel Data

        [SerializeField] AnimatorController armsAnimations;
        public AnimatorController ArmsAnimations => armsAnimations;
        
        [FormerlySerializedAs("viewmodel"), SerializeField] AssetReference viewmodelAsset;
        public AssetReference Viewmodel => viewmodelAsset;
        [SerializeField] ViewmodelSettings viewmodelSettings;
        public ViewmodelSettings Settings => viewmodelSettings;
        [FormerlySerializedAs("anims"), SerializeField] EquipmentAnimationData animationData;
        public EquipmentAnimationData Animations => animationData;
        public bool HasViewmodel => viewmodelAsset != null && viewmodelAsset.IsSet();

        #endregion
    }
}
