using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
