using System;

using UnityEngine;
using UnityEditor.Animations;
using UnityEngine.AddressableAssets;

using UZSG.FPP;
using UZSG.Items.Weapons;

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
        [Header("Weapon Data")]
        public Sprite HotbarIcon;
        public WeaponCategory Category;
        public WeaponMeleeType MeleeType;
        public WeaponBluntType BluntType;
        public WeaponBladedType BladedType;
        public WeaponRangedType RangedType;
        public WeaponMeleeAttributes MeleeAttributes;
        public WeaponRangedAttributes RangedAttributes;

        /// Viewmodel Settings
        [SerializeField] AnimatorController armsAnimations;
        public AnimatorController ArmsAnimations => armsAnimations;
        [SerializeField] AssetReference viewmodel;
        public AssetReference Viewmodel => viewmodel;
        [SerializeField] ViewmodelOffsets viewmodelOffsets;
        public ViewmodelOffsets Offsets => viewmodelOffsets;
        [SerializeField] EquipmentAnimationData anims;
        public EquipmentAnimationData Animations => anims;
        [SerializeField] EquipmentAudioData audioData;
        public EquipmentAudioData AudioData => audioData;
        public bool HasViewmodel => viewmodel.IsSet();
    }
}
