using System;
using System.Collections.Generic;
using System.Linq;
using FiniteStateMachine;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

public class TransitionPopup : EditorWindow
{
    private Vector2 _size;
    private StateMachineEditor _editor;
    private BaseState _fromState;
    private BaseState _toState;
    
    private GUIStyle _objectFieldStyle;
    private Vector2 _objectFieldSize;
    private string _originPath;
    private AssetFinderWindow _assetFinder;
    
    private readonly Vector2 _objectFieldRatio = new Vector2(1f, 0.16f);

    
    public static TransitionPopup CreateTransitionPopup(Vector2 editorSize, StateMachineEditor editor)
    {
        Debug.Log("creating asset window");
        TransitionPopup window = CreateInstance(typeof(TransitionPopup)) as TransitionPopup;
        window.titleContent = new GUIContent("Transition Creator");
        
        window._originPath = editor.CharacterPath;
        window.maxSize = editorSize;
        window.minSize = editorSize;
        window._size = editorSize;
        window.CreateStyles();
        window.GetSizes();
        window. _editor = editor;
        
        window.ShowUtility();
        return window;
    }

    public void OnGUI()
    {
        GUILayout.Label("transition popup", EditorStyles.boldLabel);
        Draw();
    }

    private void CreateStyles()
    {
        _objectFieldStyle = EditorStyles.objectField;
        _objectFieldStyle.alignment = TextAnchor.MiddleLeft;
    }

    private void GetSizes()
    {
        float xRatio = 0.8f;
        float xSize = _size.x * xRatio;
        _objectFieldSize = xSize * _objectFieldRatio;
    }

    /// <summary>
    /// to state must be filled. from state optional.
    /// </summary>
    private void Draw()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        EditorGUILayout.BeginVertical();
        GUILayout.FlexibleSpace();

        DrawObjectField(_fromState? _fromState.name: "from state", ref _fromState);
        GUILayout.Space(5);
        DrawObjectField(_toState? _toState.name: "to state", ref _toState);
        
        GUILayout.Space(10);
        DrawSaveButton();

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndVertical();
        
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawObjectField(string text, ref BaseState state)
    {
        EditorGUILayout.BeginHorizontal();
        //TODO: create editor to open custom asset finder
        EditorGUILayout.SelectableLabel(text, EditorStyles.objectField,
            GUILayout.MaxWidth(_objectFieldSize.x), GUILayout.Height(_objectFieldSize.y));
        // GUILayout.Space(-_objectFieldSize.y);
        if (GUILayout.Button(EditorGUIUtility.IconContent("d_GameObject Icon"),
                GUILayout.Width(_objectFieldSize.y), GUILayout.Height(_objectFieldSize.y)))
        {
            OpenAssetFinder(state == _fromState ? SetFrom : SetTo );
        };
        EditorGUILayout.EndHorizontal();
    }

    private void DrawSaveButton()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("save",
                GUILayout.MaxWidth(80), GUILayout.Height(20)))
        {
            if (!CreateTransition(out string message)) Debug.LogError(message);
            else Debug.Log(message);
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private void OpenAssetFinder(Action<BaseState> action)
    {
        if (_assetFinder) _assetFinder.Close();
        _assetFinder = AssetFinderWindow.CreateFinderWindow(Vector2.zero, new Vector2(500, 300),
            _editor.CharacterPath, typeof(BaseState), (Object o) => action(o as BaseState));
    }

    private void SetFrom(BaseState state)
    {
        Debug.Log("setting state");
        _fromState = state;
        Repaint();
    }
    
    private void SetTo(BaseState state)
    {
        _toState = state;
        Repaint();
    }

    private bool CreateTransition(out string message)
    {
        if (!_toState || !_fromState)
        {
            message = "Missing either 'from state' or 'to state'.";
            return false;
        }
        //TODO: create transition node
        if (!_editor.StateToNode.TryGetValue(_fromState, out StateNode fromNode))
        {
            message = $"from state, {_fromState.name}, does not exist in editor.";
            return false;
        }
        if (!_editor.StateToNode.TryGetValue(_toState, out StateNode toNode))
        {
            message = $"to state, {_toState.name}, does not exist in editor.";
            return false;
        }
        _editor.CheckCreateTransition(fromNode.OutPoint, toNode.InPoint);
        message = $"Successfully created transition from {_fromState.name} to {_toState.name}";
        _fromState = null;
        _toState = null;
        return true;
    }
}