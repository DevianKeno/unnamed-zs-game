using UnityEditor;
using UnityEngine;

using UZSG.Data;

namespace UZSG.UnityEditor
{
    [CustomPropertyDrawer(typeof(NoiseLayer))]
    public class NoiseLayerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            // Indent & foldout
            property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
                property.isExpanded, label, true);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                // Get all struct fields
                SerializedProperty noiseType = property.FindPropertyRelative("NoiseType");
                SerializedProperty seed = property.FindPropertyRelative("Seed");
                SerializedProperty offset = property.FindPropertyRelative("Offset");
                SerializedProperty octaves = property.FindPropertyRelative("Octaves");
                SerializedProperty persistence = property.FindPropertyRelative("Persistence");
                SerializedProperty lacunarity = property.FindPropertyRelative("Lacunarity");
                SerializedProperty scale = property.FindPropertyRelative("Scale");
                SerializedProperty density = property.FindPropertyRelative("Density");

                Rect fieldRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);

                EditorGUI.PropertyField(fieldRect, noiseType);
                fieldRect.y += EditorGUIUtility.singleLineHeight + 2;
                EditorGUI.PropertyField(fieldRect, seed);
                fieldRect.y += EditorGUIUtility.singleLineHeight + 2;
                EditorGUI.PropertyField(fieldRect, offset);
                fieldRect.y += EditorGUIUtility.singleLineHeight + 2;

                // Get the current noise type value
                NoiseType selectedNoiseType = (NoiseType)noiseType.enumValueIndex;

                switch (selectedNoiseType)
                {
                    case NoiseType.Random:
                        EditorGUI.PropertyField(fieldRect, density);
                        fieldRect.y += EditorGUIUtility.singleLineHeight + 2;
                        break;

                    case NoiseType.Simplex:
                        EditorGUI.PropertyField(fieldRect, scale);
                        fieldRect.y += EditorGUIUtility.singleLineHeight + 2;
                        break;

                    case NoiseType.Perlin:
                        EditorGUI.PropertyField(fieldRect, octaves);
                        fieldRect.y += EditorGUIUtility.singleLineHeight + 2;
                        EditorGUI.PropertyField(fieldRect, persistence);
                        fieldRect.y += EditorGUIUtility.singleLineHeight + 2;
                        EditorGUI.PropertyField(fieldRect, lacunarity);
                        fieldRect.y += EditorGUIUtility.singleLineHeight + 2;
                        EditorGUI.PropertyField(fieldRect, scale);
                        fieldRect.y += EditorGUIUtility.singleLineHeight + 2;
                        break;
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        // Set the height dynamically based on expanded state
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return EditorGUIUtility.singleLineHeight; // Only 1 line when collapsed

            int lines = 4; // Base fields (NoiseType, Seed, Offset)

            NoiseType selectedNoiseType = (NoiseType)property.FindPropertyRelative("NoiseType").enumValueIndex;

            switch (selectedNoiseType)
            {
                case NoiseType.Random: lines += 1; break;   // Density
                case NoiseType.Simplex: lines += 1; break;  // Scale
                case NoiseType.Perlin: lines += 4; break;   // Octaves, Persistence, Lacunarity, Scale
            }

            return EditorGUIUtility.singleLineHeight * lines + (lines - 1) * 2; // Add spacing
        }
    }
}
