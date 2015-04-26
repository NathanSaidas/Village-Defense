using UnityEngine;
using UnityEditor;

namespace Gem
{
    [CustomPropertyDrawer(typeof(DebugLabelAttribute))]
    public class DebugLabelDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property);
            GUI.enabled = true;
        }
    }

}

