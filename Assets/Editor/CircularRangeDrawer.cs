using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

//based on: https://gist.github.com/vertxxyz/5a00dbca58aee033b35be2e227e80f8d
[CustomPropertyDrawer(typeof(CircularRangeAttribute))]
public class CircularRangeDrawer : PropertyDrawer
{
    private const float _circleRadius = 50f;
    private int _hotControl = -1;
    private readonly int _hash = "CircularRange".GetHashCode();

    private Texture _circleTexture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Editor/Textures/Arrow.png");

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property); //included for prefabs
        if (property.propertyType != SerializedPropertyType.Float)
        {
            EditorGUI.LabelField(position, label.text, "Use CircularRange only with float.");
        }
        else
        {
            CircularRangeAttribute circularRange = (CircularRangeAttribute)attribute;
            DrawCircularRange(position, property, circularRange);
        }
        EditorGUI.EndProperty();
    }

    private void DrawCircularRange(Rect position, SerializedProperty property, CircularRangeAttribute circularRange)
    {
        using (new GUI.GroupScope(position))
        {
            Rect circleBounds = new Rect(Vector2.zero, 2f * _circleRadius * Vector2.one);
            Event guiEvent = Event.current;
            if (guiEvent.isMouse)
            {
                if (guiEvent.button == 0)
                {
                    if (guiEvent.rawType == EventType.MouseDown)
                    {
                        if (circleBounds.Contains(guiEvent.mousePosition))
                        {
                            _hotControl = GUIUtility.GetControlID(_hash, FocusType.Passive, circleBounds);
                            GUIUtility.hotControl = _hotControl;
                            guiEvent.Use();
                        }
                    }
                    if (guiEvent.rawType == EventType.MouseUp)
                    {
                        if (circleBounds.Contains(guiEvent.mousePosition))
                        {
                            _hotControl = -1;
                            GUIUtility.hotControl = 0;
                            guiEvent.Use();
                        }
                    }
                }
            }

            Rect rotationPivot = new Rect(circleBounds.width / 2f, circleBounds.height / 2f, 0f, 0f);
            using (new GUI.GroupScope(rotationPivot))
            {
                if (GUIUtility.hotControl == _hotControl)
                {
                    if (guiEvent.isMouse)
                    {
                        float mouseAngle = Mathf.Atan2(-guiEvent.mousePosition.y, guiEvent.mousePosition.x);
                        if (mouseAngle < 0f)
                        {
                            mouseAngle += Mathf.PI * 2f;
                        }
                        property.floatValue = Mathf.Lerp(circularRange.Min, circularRange.Max, mouseAngle / (Mathf.PI * 2f));
                        guiEvent.Use();
                    }
                }
            }

            float propertyValue = property.floatValue;
            using (EditorGUI.ChangeCheckScope cC = new EditorGUI.ChangeCheckScope())
            {
                Rect labelRect = new Rect(_circleRadius * 2 + 5f, _circleRadius - 50f, Screen.width - _circleRadius * 2f - 5f, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(labelRect, property.displayName);
                labelRect.y += EditorGUIUtility.singleLineHeight;
                propertyValue = EditorGUI.FloatField(labelRect, propertyValue);
                if (cC.changed)
                {
                    property.floatValue = Mathf.Clamp(propertyValue, circularRange.Min, circularRange.Max);
                }
            }

            using (new GUI.GroupScope(circleBounds))
            {
                if (guiEvent.type == EventType.Repaint)
                {
                    GUIUtility.RotateAroundPivot(-property.floatValue, rotationPivot.center);
                    GUI.DrawTexture(circleBounds, _circleTexture);
                }
            }
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => _circleRadius * 2f;
}
