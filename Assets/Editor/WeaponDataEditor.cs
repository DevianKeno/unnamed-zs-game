using UnityEditor;
using UZSG.Items.Weapons;

namespace UZSG.UnityEditor
{
    [CustomEditor(typeof(WeaponData))]
    public class WeaponDataEditor : ItemDataEditor
    {
        SerializedProperty hotbarIcon,
            category,
            meleeType,
            bluntType,
            bladedType,
            rangedType,
            meleeAttributes,
            rangedAttributes,
            armsAnimations,
            viewmodel,
            viewmodelOffsets,
            anims,
            audioData;
        
        protected override void OnEnable()
        {
            base.OnEnable();

            hotbarIcon = serializedObject.FindProperty("HotbarIcon");
            category = serializedObject.FindProperty("Category");
            meleeType = serializedObject.FindProperty("MeleeType");
            bluntType = serializedObject.FindProperty("BluntType");
            bladedType = serializedObject.FindProperty("BladedType");
            rangedType = serializedObject.FindProperty("RangedType");
            meleeAttributes = serializedObject.FindProperty("MeleeAttributes");
            rangedAttributes = serializedObject.FindProperty("RangedAttributes");

            armsAnimations = serializedObject.FindProperty("armsAnimations");
            viewmodel = serializedObject.FindProperty("viewmodel");
            viewmodelOffsets = serializedObject.FindProperty("viewmodelOffsets");
            anims = serializedObject.FindProperty("anims");
            audioData = serializedObject.FindProperty("audioData");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            
            WeaponData weaponData = (WeaponData) target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Weapon Attributes", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(hotbarIcon);
            EditorGUILayout.PropertyField(category);
            WeaponCategory weaponCategory = (WeaponCategory) category.enumValueIndex;
            if (weaponCategory == WeaponCategory.Melee)
            {
                EditorGUILayout.PropertyField(meleeType);
                WeaponMeleeType type = (WeaponMeleeType) meleeType.enumValueIndex;
                if (type == WeaponMeleeType.Blunt)
                {
                    EditorGUILayout.PropertyField(bluntType);
                }
                else if (type == WeaponMeleeType.Bladed)
                {
                    EditorGUILayout.PropertyField(bladedType);
                }

                EditorGUILayout.PropertyField(meleeAttributes);
            }
            else if (weaponCategory == WeaponCategory.Ranged)
            {
                EditorGUILayout.PropertyField(rangedType);
                WeaponRangedType type = (WeaponRangedType) rangedType.enumValueIndex;
                if (type == WeaponRangedType.Handgun)
                {

                }
                else if (type == WeaponRangedType.Shotgun)
                {

                }
                else if (type == WeaponRangedType.SMG)
                {
                    
                }
                else if (type == WeaponRangedType.AssaultRifle)
                {
                    
                }
                else if (type == WeaponRangedType.SniperRifle)
                {
                    
                }
                else if (type == WeaponRangedType.MachineGun)
                {
                    
                }

                EditorGUILayout.PropertyField(rangedAttributes);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Viewmodel Attributes", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(armsAnimations);
            EditorGUILayout.PropertyField(viewmodel);
            EditorGUILayout.PropertyField(viewmodelOffsets);
            EditorGUILayout.PropertyField(anims);
            EditorGUILayout.PropertyField(audioData);

            serializedObject.ApplyModifiedProperties();
        }
    }
}