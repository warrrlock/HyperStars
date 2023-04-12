using UnityEditor;
using UnityEngine;

///Source: https://gist.github.com/INeatFreak/e01763f844336792ebe07c1cd1b6d018
[CustomPropertyDrawer(typeof(Optional<>))]
public class OptionalPropertyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var valueProperty = property.FindPropertyRelative("value");
        return EditorGUI.GetPropertyHeight(valueProperty);
    }

    public override void OnGUI(
        Rect position,
        SerializedProperty property,
        GUIContent label
    )
    {
        var valueProperty = property.FindPropertyRelative("value");
        var enabledProperty = property.FindPropertyRelative("enabled");

        EditorGUI.BeginProperty(position, label, property);
        position.width -= 24;
        EditorGUI.BeginDisabledGroup(!enabledProperty.boolValue);
        EditorGUI.PropertyField(position, valueProperty, label, true);
        EditorGUI.EndDisabledGroup();

        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;
        position.x += position.width + 24;
        position.width = position.height = EditorGUI.GetPropertyHeight(enabledProperty);
        position.x -= position.width;
        EditorGUI.PropertyField(position, enabledProperty, GUIContent.none);
        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
}