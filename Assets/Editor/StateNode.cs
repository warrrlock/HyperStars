using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// based off of https://oguzkonya.com/creating-node-based-editor-unity/
public class StateNode
{
    private Rect _rect;
    public Rect Rect => _rect;
    private string _title;
    private bool _isDragged;
    private bool _isSelected;

    private GUIStyle _style;
    private GUIStyle _defaultNodeStyle;
    private GUIStyle _selectedNodeStyle;
    private ConnectionPoint _inPoint;
    private ConnectionPoint _outPoint;
    
    private Action<StateNode> OnClickRemoveState;
    private List<TransitionNode> _transitionNodes = new ();

    public StateNode(Vector2 position, float width, float height, 
        GUIStyle nodeStyle, GUIStyle selectedStyle, 
        GUIStyle inPointStyle, GUIStyle outPointStyle, 
        Action<ConnectionPoint> onClickInPoint, Action<ConnectionPoint> onClickOutPoint,
        Action<StateNode> onClickRemoveState)
    {
        _rect = new Rect(position.x, position.y, width, height);
        _style = nodeStyle;
        _inPoint = new ConnectionPoint(this, ConnectionPointType.In, inPointStyle, onClickInPoint);
        _outPoint = new ConnectionPoint(this, ConnectionPointType.Out, outPointStyle, onClickOutPoint);
        _defaultNodeStyle = nodeStyle;
        _selectedNodeStyle = selectedStyle;
        OnClickRemoveState = onClickRemoveState;
    }

    public void Drag(Vector2 delta)
    {
        _rect.position += delta;
    }

    public void Draw()
    {
        _inPoint.Draw();
        _outPoint.Draw();
        GUI.Box(_rect, _title, _style);
        DrawDeleteButton();
    }
    
    private void DrawDeleteButton()
    {
        if (Handles.Button(_rect.center, 
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
                    bool contains = _rect.Contains(e.mousePosition);
                    _isDragged = contains;
                    GUI.changed = contains;
                    _isSelected = contains;
                    _style = _isSelected ? _selectedNodeStyle : _defaultNodeStyle;
                }
                break;

            case EventType.MouseUp:
                _isDragged = false;
                _isSelected = _rect.Contains(e.mousePosition);
                _style = _isSelected ? _selectedNodeStyle : _defaultNodeStyle;
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
}