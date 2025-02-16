using UnityEditor;
using UnityEngine;

using UZSG.Data;

namespace UZSG.UnityEditor
{
    [CustomPropertyDrawer(typeof(Attributes.Attribute))]
    public class AttributeDrawer : PropertyDrawer
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

                SerializedProperty data = property.FindPropertyRelative("data");
                SerializedProperty value = property.FindPropertyRelative("value");
                SerializedProperty baseMaximum = property.FindPropertyRelative("baseMaximum");

                Rect fieldRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);

                EditorGUI.PropertyField(fieldRect, data);
                fieldRect.y += EditorGUIUtility.singleLineHeight + 2;
                EditorGUI.PropertyField(fieldRect, value);
                fieldRect.y += EditorGUIUtility.singleLineHeight + 2;
                EditorGUI.PropertyField(fieldRect, baseMaximum);
                fieldRect.y += EditorGUIUtility.singleLineHeight + 2;
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        // Set the height dynamically based on expanded state
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return EditorGUIUtility.singleLineHeight;

            int lines = 1 + 3;

            return EditorGUIUtility.singleLineHeight * lines;
        }
    }
}
