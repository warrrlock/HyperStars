using System;
using FiniteStateMachine;
using Managers;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomPropertyDrawer(typeof(FsmListAttribute))]
public class FsmListDrawer: PropertyDrawer
{
    private AssetFinderWindow _assetFinder;
    private CharacterManager _characterManager;
    private Character _character;
    private string _characterManagerPath = "Assets/Scriptable Objects/State Machine/Character Manager.asset";
    private FsmListAttribute _attribute;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, label);
        DrawObjectField(position, property, label);
        EditorGUI.EndProperty();
        ProcessEvents(Event.current, position, property);
        _attribute = (FsmListAttribute)attribute;
    }

    private void ProcessEvents(Event e, Rect position, SerializedProperty property)
    {
        switch (e.type)
        {
            case EventType.MouseUp:
                if (e.button == 0 && position.Contains(e.mousePosition))
                    if(property.objectReferenceValue) Selection.objects = new[] { property.objectReferenceValue };
                break;
        }
    }
    
    private void DrawObjectField(Rect position, SerializedProperty property, GUIContent label)
    {
        if (!_character) LoadCharacter(((BaseState)property.serializedObject.targetObject).characterSelection);

        float height = position.height - 2;
        Rect labelRect = new Rect(position);
        labelRect.height = height;
        
        Vector2 buttonSize = new Vector2(height, height);
        EditorGUI.LabelField(labelRect, 
            new GUIContent(property.objectReferenceValue? property.objectReferenceValue.name : "None"), 
            EditorStyles.objectField);
        if (GUI.Button(new Rect(position.max - buttonSize, buttonSize), 
                EditorGUIUtility.IconContent("d_GameObject Icon"), EditorStyles.iconButton))
        {
            OpenAssetFinder(SetAsset(property), _character.CharacterPath);
        }
    }

    private Action<Object> SetAsset(SerializedProperty property)
    {
        return (action) =>
        {
            property.objectReferenceValue = action;
            property.serializedObject.ApplyModifiedProperties();
        };
    }
    
    private void OpenAssetFinder(Action<Object> action, string path)
    {
        if (path == "") return;
        if (_assetFinder) _assetFinder.Close();
        _assetFinder = AssetFinderWindow.CreateFinderWindow(Vector2.zero, new Vector2(500, 300),
            path, _attribute.type, action);
    }

    private void LoadCharacter(CharacterManager.CharacterName selection)
    {
        if (!_characterManager) LoadCharacterManager();
        _characterManager.Characters.TryGetValue(selection, out _character);
    }
    
    private void LoadCharacterManager()
    {
        _characterManager = (CharacterManager)AssetDatabase.LoadAssetAtPath(_characterManagerPath, typeof(CharacterManager));
        if (!_characterManager) Debug.LogError("no character manager exits at " +
                                              $"{_characterManagerPath}. " +
                                              "Please ensure the character manager has not been moved or deleted");
    }
}