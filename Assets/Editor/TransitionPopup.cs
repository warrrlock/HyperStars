using System.Collections.Generic;
using System.Linq;
using FiniteStateMachine;
using UnityEngine;
using UnityEditor;

public class TransitionPopup : PopupWindowContent
{
    private readonly Vector2 _size;
    private StateMachineEditor _editor;
    private BaseState _fromState;
    private BaseState _toState;
    
    private GUIStyle _objectFieldStyle;
    private Vector2 _objectFieldSize;
    
    private readonly Vector2 _objectFieldRatio = new Vector2(1f, 0.16f);

    
    public override Vector2 GetWindowSize()
    {
        return _size;
    }

    public TransitionPopup(Vector2 size, StateMachineEditor editor)
    {
        _size = size;
        _editor = editor;
        CreateStyles();
        GetSizes();
    }

    public override void OnGUI(Rect rect)
    {
        GUILayout.Label("transition popup", EditorStyles.boldLabel);
        Draw();
    }

    public override void OnOpen()
    {
        //TODO: darken bg
        Debug.Log("Popup opened: " + this);
    }

    public override void OnClose()
    {
        //TODO: lighten bg
        Debug.Log("Popup closed: " + this);
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

        DrawObjectField("from state", ref _fromState);
        GUILayout.Space(5);
        DrawObjectField("to state", ref _toState);
        
        GUILayout.Space(10);
        DrawSaveButton();

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndVertical();
        
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawObjectField(string text, ref BaseState state)
    {
        // EditorGUILayout.BeginHorizontal();
        // //TODO: create editor to open custom asset finder
        // EditorGUILayout.SelectableLabel(text, EditorStyles.objectField,
        //     GUILayout.MaxWidth(_objectFieldSize.x), GUILayout.Height(_objectFieldSize.y));
        // GUILayout.Space(-_objectFieldSize.y);
        // if (GUILayout.Button(EditorGUIUtility.IconContent("d_GameObject Icon"),
        //         GUILayout.Width(_objectFieldSize.y), GUILayout.Height(_objectFieldSize.y)))
        // {
        //     OpenAssetFinder();
        // };
        // EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        state = (BaseState)EditorGUILayout.ObjectField(state, typeof(BaseState),false,
            GUILayout.MaxWidth(_objectFieldSize.x), GUILayout.Height(_objectFieldSize.y));
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

    private void OpenAssetFinder()
    {
        
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