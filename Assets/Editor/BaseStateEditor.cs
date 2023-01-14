using System;
using System.Linq;
using FiniteStateMachine;
using Managers;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BaseState), true), CanEditMultipleObjects]
public class BaseStateEditor : Editor
{
    private bool showFilters;
    private CharacterManager characterManager;
    private Character character;
    private Texture2D addSymbol;
    private Texture2D minusSymbol;
    private string originPath = "Assets/Scriptable Objects/[TEST] editor";
    
    public override void OnInspectorGUI()
    {
        BaseState baseState = (BaseState)target;
        base.OnInspectorGUI();
        DrawFilters(baseState);
        DrawFilterPicker(baseState);
    }

    private void OnEnable()
    {
        addSymbol = EditorGUIUtility.Load("icons/d_Toolbar Plus.png") as Texture2D;
        minusSymbol = EditorGUIUtility.Load("icons/d_Toolbar Minus.png") as Texture2D;
    }

    private void DrawFilters(BaseState baseState)
    {
        EditorGUILayout.Space(20);
        showFilters = EditorGUILayout.BeginFoldoutHeaderGroup(baseState.ShowFilters, "Filters");
        if (baseState.ShowFilters != showFilters) baseState.ToggleShowFilter();
        if (baseState.ShowFilters)
        {
            if (baseState.Filters.Count <= 0) EditorGUILayout.LabelField("no filters.");
            else
            {
                for (int i = 0; i < baseState.Filters.Count; i++) 
                {
                    FSMFilter filter = baseState.Filters[i];
                    DrawFilter(filter, minusSymbol, () => RemoveFilterFromState(filter, baseState));
                }
            }
            
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space(20);
    }

    private void DrawFilterPicker(BaseState baseState)
    {
        if (baseState.ShowFilters)
        {
            EditorGUILayout.LabelField("available filters");
            if (!characterManager) LoadCharacterManager();
            if (!character)
            {
                KeyCharacterPair characterPair = characterManager.Characters.Find(o => o.characterSelection == baseState.character);
                if (characterPair != null) character = characterPair.character;
                else
                {
                    Debug.LogError($"no character '{baseState.character.ToString()}' exists in character manager.");
                    return;
                }
            }

            for (int i = 0; i < character.Filters.Count; i++)
            {
                FSMFilter filter = character.Filters[i];
                if (!baseState.Filters.Contains(filter, new FilterEqualityComparer())) DrawFilter(filter, addSymbol, () => AddFilterToState(filter, baseState));
            }
        }
    }

    private void DrawFilter(FSMFilter f, Texture2D symbol, Action a)
    {
        EditorGUILayout.BeginHorizontal("box");
        EditorGUILayout.LabelField(f.filterName);
        EditorGUILayout.ColorField(new GUIContent(""), f.color,
            false, false, false, GUILayout.Width(50));
        if (GUILayout.Button(new GUIContent(symbol), GUILayout.Width(30))) a();
        EditorGUILayout.EndHorizontal();
        
    }

    private void AddFilterToState(FSMFilter f, BaseState bs)
    {
        bs.AddFilter(f);
        MoveToCorrectFolder(f, bs);
    }
    
    private void RemoveFilterFromState(FSMFilter f, BaseState bs)
    {
        bs.RemoveFilter(f);
        MoveToCorrectFolder(f, bs);
    }

    private void MoveToCorrectFolder(FSMFilter f, BaseState bs)
    {
        string ending = $"{bs.name}.asset";
        if (bs.Filters.Count == 0)
            AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(bs), $"{originPath}/{ending}");
        else if (bs.Filters.Count == 1)
            AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(bs), $"{originPath}/{bs.Filters[0].filterName}/{ending}");
        else
            AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(bs), $"{originPath}/multi-filtered/{ending}");
        AssetDatabase.Refresh();
        
        Debug.Log($"adding/removing {f.filterName}, moving {bs.name} with {bs.Filters.Count} filters.");
    }
    
    private void LoadCharacterManager()
    {
        characterManager = (CharacterManager)AssetDatabase.LoadAssetAtPath($"{originPath}/[TEST]character manager.asset", typeof(CharacterManager));
        if (!characterManager) Debug.LogError("no character manager exits at " +
                                              $"{originPath}/[TEST]character manager. " +
                                              "Please ensure the character manager has not been moved or deleted");
    }

}
