using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using WesleyDavies;
using UnityEngine.Rendering;
using System.Drawing.Drawing2D;
using System.Reflection;

[CustomPropertyDrawer(typeof(PolarArrowAttribute))]
public class PolarArrowDrawer : PropertyDrawer
{
    private const float _gridWidth = 150f;
    private int _hotControl = -1;
    private readonly int _hash = "PolarArrow".GetHashCode();

    private Texture _arrowTexture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Editor/Textures/Arrow.png");

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property); //included for prefabs
        if (property.propertyType != SerializedPropertyType.Vector2)
        {
            EditorGUI.LabelField(position, label.text, "Use PolarArrow only with Vector2.");
        }
        else
        {
            PolarArrowAttribute polarArrow = (PolarArrowAttribute)attribute;
            DrawPolarArrow(position, property, polarArrow);
        }
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => _gridWidth;

    private void DrawPolarArrow(Rect position, SerializedProperty property, PolarArrowAttribute polarArrow)
    {
        using (new GUI.GroupScope(position))
        {
            Rect gridBounds = new Rect(Vector2.zero, _gridWidth * Vector2.one);
            Event guiEvent = Event.current;
            if (guiEvent.isMouse)
            {
                if (guiEvent.button == 0)
                {
                    if (guiEvent.rawType == EventType.MouseDown)
                    {
                        if (gridBounds.Contains(guiEvent.mousePosition))
                        {
                            _hotControl = GUIUtility.GetControlID(_hash, FocusType.Passive, gridBounds);
                            GUIUtility.hotControl = _hotControl;
                            guiEvent.Use();
                        }
                    }
                    if (guiEvent.rawType == EventType.MouseUp)
                    {
                        if (gridBounds.Contains(guiEvent.mousePosition))
                        {
                            _hotControl = -1;
                            GUIUtility.hotControl = 0;
                            guiEvent.Use();
                        }
                    }
                }
            }

            Rect rotationPivot = new Rect(_gridWidth / 2f, _gridWidth / 2f, 0f, 0f);
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
                        float angle = Mathf.Lerp(polarArrow.MinAngle, polarArrow.MaxAngle, mouseAngle / (Mathf.PI * 2f));
                        float maxDistance = Vector2.Distance(Vector2.zero, new Vector2(rotationPivot.center.x, 0f));
                        float mouseDistance = Vector2.Distance(Vector2.zero, guiEvent.mousePosition);
                        float distance = Mathf.Lerp(polarArrow.MinDistance, polarArrow.MaxDistance, mouseDistance / maxDistance);
                        property.vector2Value = new Vector2(angle, distance);
                        guiEvent.Use();
                    }
                }
            }

            Vector2 propertyValue = property.vector2Value;
            using (EditorGUI.ChangeCheckScope changeCheck = new EditorGUI.ChangeCheckScope())
            {
                Rect labelRect = new Rect(_gridWidth + 5f, _gridWidth / 2f - 50f, Screen.width - _gridWidth * 2f - 5f, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(labelRect, property.displayName, EditorStyles.boldLabel);
                labelRect.y += 20;
                EditorGUI.LabelField(labelRect, "Angle");
                labelRect.y += EditorGUIUtility.singleLineHeight;
                propertyValue.x = EditorGUI.FloatField(labelRect, propertyValue.x);
                labelRect.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(labelRect, "Magnitude");
                labelRect.y += EditorGUIUtility.singleLineHeight;
                propertyValue.y = EditorGUI.FloatField(labelRect, propertyValue.y);
                if (changeCheck.changed)
                {
                    property.vector2Value = new Vector2(Mathf.Clamp(propertyValue.x, polarArrow.MinAngle, polarArrow.MaxAngle), Mathf.Clamp(propertyValue.y, polarArrow.MinDistance, polarArrow.MaxDistance));
                }
            }

            using (new GUI.GroupScope(gridBounds, EditorStyles.helpBox))
            {
                if (guiEvent.type == EventType.Repaint)
                {
                    //Draw Grid
                    GL.Begin(Application.platform == RuntimePlatform.WindowsEditor ? GL.QUADS : GL.LINES);
                    ApplyWireMaterial.Invoke(null, new object[] { CompareFunction.Always });
                    const float quarter = 0.25f * _gridWidth;
                    const float half = quarter * 2;
                    const float threeQuarters = half + quarter;
                    float lowGrayValue = 0.375f;
                    float highGrayValue = 0.5f;
                    Color lowGray = new Color(lowGrayValue, lowGrayValue, lowGrayValue);
                    Color highGray = new Color(highGrayValue, highGrayValue, highGrayValue);
                    DrawLineFast(new Vector2(quarter, 0), new Vector2(quarter, _gridWidth), lowGray);
                    DrawLineFast(new Vector2(quarter, 0), new Vector2(quarter, _gridWidth), lowGray);
                    DrawLineFast(new Vector2(threeQuarters, 0), new Vector2(threeQuarters, _gridWidth), lowGray);
                    DrawLineFast(new Vector2(0, quarter), new Vector2(_gridWidth, quarter), lowGray);
                    DrawLineFast(new Vector2(0, threeQuarters), new Vector2(_gridWidth, threeQuarters), lowGray);
                    DrawLineFast(new Vector2(half, 0), new Vector2(half, _gridWidth), highGray);
                    DrawLineFast(new Vector2(0, half), new Vector2(_gridWidth, half), highGray);
                    GL.End();

                    //Draw Arrow
                    Matrix4x4 defaultMatrix = GUI.matrix;
                    GUIUtility.RotateAroundPivot(-property.vector2Value.x, rotationPivot.center);
                    float arrowLength = Mathf.Lerp(0f, gridBounds.width / 2f, propertyValue.y / polarArrow.MaxDistance);
                    const float minArrowHeight = _gridWidth / 20f;
                    const float maxArrowHeight = _gridWidth / 5f;
                    float arrowHeight = Mathf.Lerp(minArrowHeight, maxArrowHeight, propertyValue.y / polarArrow.MaxDistance);
                    Rect arrowRect = new Rect(rotationPivot.center.x, rotationPivot.center.y - arrowHeight / 2f, arrowLength, arrowHeight);
                    GUI.DrawTexture(arrowRect, _arrowTexture);
                    GUI.matrix = defaultMatrix;
                }
            }
        }
    }

    private static void DrawLineFast(Vector2 from, Vector2 to, Color color)
    {
        GL.Color(color);
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            Vector2 tangent = (to - from).normalized;
            Vector2 mult = new Vector2(tangent.y > tangent.x ? -1 : 1, tangent.y > tangent.x ? 1 : -1);
            tangent = new Vector2(mult.x * tangent.y, mult.y * tangent.x) * 0.5f;
            GL.Vertex(new Vector3(from.x + tangent.x, from.y + tangent.y, 0f));
            GL.Vertex(new Vector3(from.x - tangent.x, from.y - tangent.y, 0f));
            GL.Vertex(new Vector3(to.x + tangent.x, to.y + tangent.y, 0f));
            GL.Vertex(new Vector3(to.x - tangent.x, to.y - tangent.y, 0f));
        }
        else
        {
            GL.Vertex(new Vector3(from.x, from.y, 0f));
            GL.Vertex(new Vector3(to.x, to.y, 0f));
        }
    }

    private MethodInfo applyWireMaterial;

    private MethodInfo ApplyWireMaterial =>
        applyWireMaterial ?? (applyWireMaterial =
            typeof(HandleUtility).GetMethod("ApplyWireMaterial", BindingFlags.NonPublic | BindingFlags.Static, null, new[] { typeof(CompareFunction) }, null));
}
