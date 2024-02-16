using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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

    public enum WeaponCategory { Melee, Ranged }
    public enum WeaponMeleeType { Blunt, Bladed }
    public enum WeaponBluntType { None, Bat, Hammer }
    public enum WeaponBladedType { None, Sword, Knife, Katana, Axe }
    public enum WeaponRangedType { None, Handgun, Shotgun, SMG, AssaultRifle, SniperRifle, MachineGun }

    /// <summary>
    /// Represents the various data a Weapon has.
    /// </summary>
    [CreateAssetMenu(fileName = "Weapon", menuName = "URMG/Weapon")]
    public class WeaponData : ItemData, IFPPVisible
    {
        public float Weight;
        public WeaponCategory Category;
        public WeaponMeleeType MeleeType;
        public WeaponBluntType BluntType;
        public WeaponBladedType BladedType;
        public WeaponRangedType RangedType;
        public WeaponMeleeAttributes Attributes;

        [Header("FPP")]
        [SerializeField] GameObject _armsModel;
        public GameObject ArmsModel => _armsModel;
        [SerializeField] GameObject _model;
        public GameObject Model => _model;

        // [SerializeField] AnimatorController _armsController;
        // public AnimatorController ArmsController => _armsController;
        // [SerializeField] AnimatorController _modelController;
        // public AnimatorController ModelController => _modelController;

        [SerializeField] FPPAnimations _anims;
        public FPPAnimations Anims => _anims;
        
        public static bool TryGetWeaponData(ItemData item, out WeaponData weaponData)
        {
            weaponData = item as WeaponData;
            return weaponData != null;
        }
    }    

    // [CustomEditor(typeof(WeaponData))]
    // public class WeaponDataEditor : ItemDataEditor
    // {
    //     SerializedProperty weight,
    //         category,
    //         meleeType,
    //         bluntType,
    //         bladedType,
    //         rangedType,
    //         attributes,
    //         FPPmodel,
    //         controller,
    //         anims;
        
    //     void OnEnable()
    //     {
    //         weight = serializedObject.FindProperty("Weight");
    //         category = serializedObject.FindProperty("Category");
    //         meleeType = serializedObject.FindProperty("MeleeType");
    //         bluntType = serializedObject.FindProperty("BluntType");
    //         bladedType = serializedObject.FindProperty("BladedType");
    //         rangedType = serializedObject.FindProperty("RangedType");
    //         attributes = serializedObject.FindProperty("Attributes");
    //         FPPmodel = serializedObject.FindProperty("_FPPModel");
    //         controller = serializedObject.FindProperty("_controller");
    //         anims = serializedObject.FindProperty("_anims");
    //     }

    //     public override void OnInspectorGUI()
    //     {
    //         base.OnInspectorGUI();
    //         serializedObject.Update();
    //         WeaponData attributeData = (WeaponData)target;

    //         EditorGUILayout.Space();
    //         EditorGUILayout.LabelField("Weapon Attributes", EditorStyles.boldLabel);
    //         EditorGUILayout.PropertyField(weight);
    //         EditorGUILayout.PropertyField(category);
            
    //         EditorGUI.indentLevel++;
    //         if (attributeData.Category == WeaponCategory.Melee)
    //         {
    //             EditorGUILayout.PropertyField(meleeType);

    //             EditorGUI.indentLevel++;
    //             if (attributeData.MeleeType == WeaponMeleeType.Blunt)
    //             {
    //                 EditorGUILayout.PropertyField(bluntType);
    //             } else if (attributeData.MeleeType == WeaponMeleeType.Bladed)
    //             {
    //                 EditorGUILayout.PropertyField(bladedType);
    //             }

    //         } else if (attributeData.Category == WeaponCategory.Ranged)
    //         {
    //             EditorGUILayout.PropertyField(rangedType);

    //             EditorGUI.indentLevel++;
    //             if (attributeData.RangedType == WeaponRangedType.Handgun)
    //             {

    //             } else if (attributeData.RangedType == WeaponRangedType.Shotgun)
    //             {

    //             } else if (attributeData.RangedType == WeaponRangedType.SMG)
    //             {
                    
    //             } else if (attributeData.RangedType == WeaponRangedType.AssaultRifle)
    //             {
                    
    //             } else if (attributeData.RangedType == WeaponRangedType.SniperRifle)
    //             {
                    
    //             } else if (attributeData.RangedType == WeaponRangedType.MachineGun)
    //             {
                    
    //             }
    //         }            
    //         EditorGUI.indentLevel -= 2;
            
    //         EditorGUILayout.PropertyField(attributes);

    //         EditorGUILayout.Space();
    //         EditorGUILayout.LabelField("Animations", EditorStyles.boldLabel);
    //         EditorGUILayout.PropertyField(FPPmodel);
    //         EditorGUILayout.PropertyField(controller);
    //         EditorGUILayout.PropertyField(anims);


    //         serializedObject.ApplyModifiedProperties();
    //     }
    // }
}
