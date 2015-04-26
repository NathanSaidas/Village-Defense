using UnityEngine;
using UnityEditor;

namespace Gem
{
    [CustomPropertyDrawer(typeof(TextAreaAttribute))]
    public class TextAreaDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect drawRect = position;
            drawRect.height = EditorGUIUtility.singleLineHeight * 3;
            EditorGUI.LabelField(drawRect, label);
            drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            property.stringValue = EditorGUI.TextArea(drawRect, property.stringValue);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {

            return 4 * EditorGUIUtility.singleLineHeight + 5 * EditorGUIUtility.standardVerticalSpacing;
        }
    }
}


