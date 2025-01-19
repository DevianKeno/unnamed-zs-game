using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;

using UZSG.FPP;
using UZSG.Items.Weapons;

namespace UZSG.Data
{
    public enum ToolType {
        Any, Axe, Pickaxe, Shovel,
    }

    public enum ToolSwingDirection {
        Upward, Downward, Leftward, Rightward,
    }

    /// <summary>
    /// Represents the various data a Tool has.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "New Tool Data", menuName = "UZSG/Items/Tool Data")]
    public class ToolData : ItemData, IViewmodel
    {
        [Header("Tool Data")]
        // public Sprite HotbarIcon;
        public ToolType ToolType;
        public List<MeleeAttackParametersData> Attacks;
        public ToolSwingDirection SwingDirection;
        public List<UZSG.Attributes.Attribute> Attributes;

        [Header("Tool as a Weapon")]
        public WeaponCategory Category;
        public WeaponMeleeAttributes MeleeAttributes;


        [Header("Viewmodel Data")]
        /// Viewmodel Settings
        [SerializeField] RuntimeAnimatorController armsAnimations;
        public RuntimeAnimatorController ArmsAnimations => armsAnimations;
        [SerializeField] AssetReference viewmodel;
        public AssetReference Viewmodel => viewmodel;
        [SerializeField] ViewmodelSettings viewmodelOffsets;
        public ViewmodelSettings Settings => viewmodelOffsets;
        [SerializeField] EquipmentAnimationData anims;
        public EquipmentAnimationData Animations => anims;
        [SerializeField] EquipmentAudioData audioData;
        public EquipmentAudioData AudioData => audioData;
        public bool HasViewmodel => viewmodel != null && viewmodel.IsSet();
    }
}
