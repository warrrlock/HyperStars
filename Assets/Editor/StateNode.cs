using System;
using System.Collections.Generic;
using FiniteStateMachine;
using UnityEditor;
using UnityEngine;

// based off of https://oguzkonya.com/creating-node-based-editor-unity/
public class StateNode
{
    public Rect Rect => _state ? _state.NodeInfo.rect: Rect.zero;
    private string _title;
    private bool _isDragged;
    private bool _isSelected;

    private GUIStyle _style;
    private GUIStyle _defaultNodeStyle;
    private GUIStyle _selectedNodeStyle;
    private ConnectionPoint _inPoint;
    private ConnectionPoint _outPoint;
    public ConnectionPoint InPoint => _inPoint;
    public ConnectionPoint OutPoint => _outPoint;
    
    private Action<StateNode> OnClickRemoveState;
    private StateMachineEditor _editor;
    private BaseState _state;
    
    private List<TransitionNode> _transitionNodes = new ();
    
    
    public StateNode(Vector2 position, float width, float height, 
        GUIStyle nodeStyle, GUIStyle selectedStyle, 
        GUIStyle inPointStyle, GUIStyle outPointStyle, 
        Action<ConnectionPoint> onClickInPoint, Action<ConnectionPoint> onClickOutPoint,
        Action<StateNode> onClickRemoveState, StateMachineEditor editor)
    {
        _style = nodeStyle;
        _inPoint = new ConnectionPoint(this, ConnectionPointType.In, inPointStyle, onClickInPoint);
        _outPoint = new ConnectionPoint(this, ConnectionPointType.Out, outPointStyle, onClickOutPoint);
        _defaultNodeStyle = nodeStyle;
        _selectedNodeStyle = selectedStyle;
        OnClickRemoveState = onClickRemoveState;
        
        //TODO: showing naming parameter, and do not create asset if empty name
        _editor = editor;
        _state = _editor.CreateStateAsset("test");
        _state.NodeInfo.rect = new Rect(position.x, position.y, width, height);
        SaveChanges();
    }
    
    public StateNode(BaseState state, 
        GUIStyle nodeStyle, GUIStyle selectedStyle, 
        GUIStyle inPointStyle, GUIStyle outPointStyle, 
        Action<ConnectionPoint> onClickInPoint, Action<ConnectionPoint> onClickOutPoint,
        Action<StateNode> onClickRemoveState, StateMachineEditor editor)
    {
        _style = nodeStyle;
        _inPoint = new ConnectionPoint(this, ConnectionPointType.In, inPointStyle, onClickInPoint);
        _outPoint = new ConnectionPoint(this, ConnectionPointType.Out, outPointStyle, onClickOutPoint);
        _defaultNodeStyle = nodeStyle;
        _selectedNodeStyle = selectedStyle;
        OnClickRemoveState = onClickRemoveState;
        
        _editor = editor;
        _state = state;
    }

    public void Drag(Vector2 delta)
    {
        _state.NodeInfo.rect.position += delta;
        SaveChanges();
    }

    public void Draw()
    {
        _inPoint.Draw();
        _outPoint.Draw();
        GUI.Box(_state.NodeInfo.rect, _title, _style);
        DrawDeleteButton();
    }
    
    private void DrawDeleteButton()
    {
        if (Handles.Button(_state.NodeInfo.rect.center, 
                Quaternion.identity, 4, 8, Handles.RectangleHandleCap))
        {
            OnClickRemoveState?.Invoke(this);
        }
    }

    public bool ProcessEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    bool contains = _state.NodeInfo.rect.Contains(e.mousePosition);
                    _isDragged = contains;
                    GUI.changed = contains;
                    _isSelected = contains;
                    _style = _isSelected ? _selectedNodeStyle : _defaultNodeStyle;
                    if (_isSelected) 
                        if (AssetDatabase.OpenAsset(_state))
                            e.Use();
                        else
                            Debug.LogError("state for this node cannot be opened.");
                }
                break;

            case EventType.MouseUp:
                _isDragged = false;
                _style = _state.NodeInfo.rect.Contains(e.mousePosition) ? _selectedNodeStyle : _defaultNodeStyle;
                break;

            case EventType.MouseDrag:
                if (e.button == 0 && _isDragged)
                {
                    Drag(e.delta);
                    e.Use();
                    return true;
                }
                break;
        }
        return false;
    }

    public void AddTransition(TransitionNode transition)
    {
        _transitionNodes.Add(transition);
    }
    
    public void ClearTransitions()
    {
        foreach (var transition in _transitionNodes)
        {
            transition.RemoveTransition();
        }
    }

    private void SaveChanges()
    { 
        EditorUtility.SetDirty(_state);
    }
}