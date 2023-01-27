using System;
using System.Collections.Generic;
using FiniteStateMachine;
using UnityEditor;
using UnityEngine;

// based off of https://oguzkonya.com/creating-node-based-editor-unity/
public class TransitionNode
{
    private Rect _rect;
    private const float Width = 50f;
    private const float Height = 30f;
    private string _title;
    
    private bool _isCLicked;
    private bool _isDragged;
    private bool _isSelected;
    private Rect _buttonRect;

    private const float BezierTangent = 20f;
    private const float BezierWidth = 2f;

    private GUIStyle _defaultStyle;
    private GUIStyle _selectedStyle;
    
    private ConnectionPoint _inPoint; //to state
    private ConnectionPoint _outPoint; //from state
    private BaseState _toState;
    private BaseState _fromState;
    public ConnectionPoint InPoint => _inPoint;
    public ConnectionPoint OutPoint => _outPoint;
    private Action<TransitionNode> OnClickRemoveTransition;

    private StateMachineEditor _editor;
    private Transition _transition;
    public Transition Transition => _transition;
    
    public TransitionNode(GUIStyle defaultStyle, GUIStyle selectedStyle,
        ConnectionPoint to, ConnectionPoint from, Action<TransitionNode> onClickRemoveTransition,
        StateMachineEditor editor, Transition t)
    {
        _inPoint = to;
        _outPoint = from;
        _toState = _inPoint.Node.BaseState;
        _fromState = _outPoint.Node.BaseState;
        OnClickRemoveTransition = onClickRemoveTransition;
        
        _rect = new Rect(0, 0, Width, Height);
        _rect.center = (_inPoint.Rect.center + _outPoint.Rect.center) * 0.5f;
        _defaultStyle = defaultStyle;
        _selectedStyle = selectedStyle;

        _editor = editor;
        _transition = t;
    }
    
    //TODO: can't drag currently, need to adjust to dragging + moving the bezier curve
    public void Drag(Vector2 delta)
    {
        _isDragged = true;
        if (_transition.positionDictionary.TryGetValue(_fromState, out var v2))
        {
            _transition.positionDictionary[_fromState] = v2 + delta;
        }
    }
    
    public void Draw()
    {
        //TODO: change bezier curve instead of rect center, should smoothly connect at center of transition node
        _rect.center = (_inPoint.Rect.center + _outPoint.Rect.center) * 0.5f 
                       + _transition.positionDictionary.GetValueOrDefault(_fromState, Vector2.zero);
        DrawBezier();
        DrawNode();
        DrawDeleteButton();
    }

    public void DrawBezier()
    {
        Vector2 startPos = _outPoint.Rect.center + _outPoint.Rect.width / 2 * Vector2.right;
        Vector2 endPos = _inPoint.Rect.center + _outPoint.Rect.width / 2 * Vector2.left;
        Vector2 midPos = _rect.center;

        Vector2 startTangent1 = startPos + (endPos - startPos).normalized * BezierTangent;
        Vector2 endTangent1 = midPos - (endPos - midPos).normalized * BezierTangent;
        
        Vector2 startTangent2 = midPos + (midPos - startPos).normalized * BezierTangent;
        Vector2 endTangent2 = endPos + (startPos - endPos).normalized * BezierTangent;
        

        Handles.DrawBezier(
            startPos,
            midPos,
            startTangent1,
            endTangent1,
            Color.white,
            null,
            BezierWidth
        );
        Handles.DrawBezier(
            midPos,
            endPos,
            startTangent2,
            endTangent2,
            Color.white,
            null,
            BezierWidth
        );
    }
    
    private void DrawNode()
    {
        GUI.Box(_rect, _title, _isSelected ? _selectedStyle: _defaultStyle);
    }

    private void DrawDeleteButton()
    {
        if (!_isSelected) return;
        GUIContent icon = EditorGUIUtility.IconContent("CrossIcon");
        Vector2 size = EditorStyles.iconButton.CalcSize(icon);
        
        GUILayout.BeginArea(new Rect(_rect.min - size/2, size));
        if(GUILayout.Button(icon, EditorStyles.iconButton))
            RemoveTransition();
        GUILayout.EndArea();
    }

    public bool ProcessEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    _isSelected = _rect.Contains(e.mousePosition);
                    if (_isSelected)
                    {
                        _editor.ClearSelectionExceptTransition(this);
                        _isCLicked = true;

                        if (AssetDatabase.OpenAsset(_transition))
                            return true;
                        Debug.LogError("transition for this node cannot be opened.");
                    }
                }
                break;

            case EventType.MouseUp:
                if (_isDragged) SaveTransitionAsset();
                _isCLicked = false;
                _isDragged = false;
                break;

            case EventType.MouseDrag:
                if (e.button == 0 && _isCLicked)
                {
                    Drag(e.delta);
                    e.Use();
                    return true;
                }
                break;
        }
        return false;
    }

    public void RemoveTransition()
    {
        OnClickRemoveTransition?.Invoke(this);
    }
    
    public void Deselect()
    {
        _isSelected = false;
    }

    private void SaveTransitionAsset()
    {
        EditorUtility.SetDirty(_transition);
    }
}