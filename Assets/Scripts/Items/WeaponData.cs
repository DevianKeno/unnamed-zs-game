using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor.Animations;

using UZSG.FPP;
using UnityEngine.AddressableAssets;
using UZSG.Players;

namespace UZSG.Items.Weapons
{
    /// <summary>
    /// List of possible animations an FPP model have.
    /// </summary>
    [Serializable]
    public struct EquipmentAnimationData : IEnumerable
    {
        public string Idle;
        public string Move;
        public string Primary;
        public string PrimaryHold;
        public string PrimaryRelease;
        public string[] PrimaryVariant;
        public string Secondary;
        public string SecondaryHold;
        public string SecondaryRelease;
        public string Equip;
        public string Dequip;

        public readonly string GetAnimHashFromState(ActionStates state)
        {
            return state switch
            {
                ActionStates.Idle => Idle,
                ActionStates.Primary => Primary,
                ActionStates.PrimaryHold => PrimaryHold,
                ActionStates.PrimaryRelease => PrimaryRelease,
                ActionStates.Secondary => Secondary,
                ActionStates.SecondaryHold => SecondaryHold,
                ActionStates.SecondaryRelease => SecondaryRelease,
                ActionStates.Equip => Equip,
                ActionStates.Dequip => Dequip,
                
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public readonly IEnumerator GetEnumerator()
        {
            List<string> list = new()
            {
                Equip,
                Idle,
                Move,
                Primary,
                Secondary,
                SecondaryHold
            };
            list.AddRange(PrimaryVariant);
            return list.GetEnumerator();
        }

        /// <summary>
        /// Get a random primary animation.
        /// </summary>
        public readonly string GetRandomPrimaryVariant()
        {
            if (PrimaryVariant.Length == 0) return null;
            /// There's no actual randomness happening, fix
            return PrimaryVariant[1];
        }
    }

    [Serializable]
    public struct AudioAssetId
    {
        public string Id;
        public AssetReference AudioAsset;
    }

    [Serializable]
    public struct EquipmentAudioData
    {
        public List<AudioAssetId> AudioAssetIds;
    }

    public enum WeaponCategory { Melee, Ranged }
    public enum WeaponMeleeType { None, Blunt, Bladed }
    public enum WeaponBluntType { None, Bat, Hammer }
    public enum WeaponBladedType { None, Sword, Knife, Katana, Axe }
    public enum WeaponRangedType { None, Handgun, Shotgun, SMG, AssaultRifle, SniperRifle, MachineGun }

    /// <summary>
    /// Represents the various data a Weapon has.
    /// </summary>
    [CreateAssetMenu(fileName = "New Weapon Data", menuName = "UZSG/Weapon Data")]
    [Serializable]
    public class WeaponData : ItemData, IFPPVisible
    {
        public Sprite HotbarIcon;
        public float Weight;
        public WeaponCategory Category;
        public WeaponMeleeType MeleeType;
        public WeaponBluntType BluntType;
        public WeaponBladedType BladedType;
        public WeaponRangedType RangedType;
        public WeaponMeleeAttributes MeleeAttributes;
        public WeaponRangedAttributes RangedAttributes;

        [Header("FPP")]
        [SerializeField] AnimatorController armsAnimations;
        public AnimatorController ArmsAnimations => armsAnimations;
        [SerializeField] AssetReference viewmodel;
        public AssetReference Viewmodel => viewmodel;
        [SerializeField] bool isActions;
        public bool IsActions => IsActions;
        [SerializeField] ViewmodelOffsets viewmodelOffsets;
        public ViewmodelOffsets ViewmodelOffsets => viewmodelOffsets;
        [SerializeField] EquipmentAnimationData anims;
        public EquipmentAnimationData Anims => anims;
        [SerializeField] EquipmentAudioData audioData;
        public EquipmentAudioData AudioData => audioData;
        
        public static bool IsWeaponData(ItemData data, out WeaponData weaponData)
        {
            weaponData = data as WeaponData;
            return weaponData != null;
        }

        public bool HasViewmodel
        {
            get
            {
                return viewmodel.IsSet();
            }
        }
    }
}
