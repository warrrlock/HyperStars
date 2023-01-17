using System;
using System.Collections.Generic;
using FiniteStateMachine;
using Managers;
using UnityEditor;
using UnityEngine;

// based off of https://oguzkonya.com/creating-node-based-editor-unity/
public class StateNode
{
    public Rect Rect => _state ? _state.NodeInfo.rect: Rect.zero;
    private bool _isDragged;
    private bool _isSelected;

    private GUIStyle _defaultNodeStyle;
    private GUIStyle _selectedNodeStyle;
    private ConnectionPoint _inPoint;
    private ConnectionPoint _outPoint;
    public ConnectionPoint InPoint => _inPoint;
    public ConnectionPoint OutPoint => _outPoint;
    
    private Action<StateNode> OnClickRemoveState;
    private StateMachineEditor _editor;
    private BaseState _state;
    public BaseState BaseState => _state;
    
    private List<TransitionNode> _transitionNodes = new ();
    public IReadOnlyList<TransitionNode> TransitionNodes => _transitionNodes;

    private Vector2 _filterScale = new Vector2(0.2f, 0.2f);

    public StateNode(GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle, 
        Action<ConnectionPoint> onClickInPoint, Action<ConnectionPoint> onClickOutPoint,
        Action<StateNode> onClickRemoveState, StateMachineEditor editor)
    {
        _inPoint = new ConnectionPoint(this, ConnectionPointType.In, inPointStyle, onClickInPoint);
        _outPoint = new ConnectionPoint(this, ConnectionPointType.Out, outPointStyle, onClickOutPoint);
        _defaultNodeStyle = nodeStyle;
        _selectedNodeStyle = selectedStyle;
        OnClickRemoveState = onClickRemoveState;
        _editor = editor;
    }
    public StateNode(Vector2 position, float width, float height, 
        GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle, 
        Action<ConnectionPoint> onClickInPoint, Action<ConnectionPoint> onClickOutPoint,
        Action<StateNode> onClickRemoveState, StateMachineEditor editor) 
        :this(nodeStyle, selectedStyle, inPointStyle, outPointStyle, onClickInPoint, onClickOutPoint, onClickRemoveState, editor)
    {
        //TODO: showing naming parameter, and do not create asset if empty name
        _state = _editor.CreateStateAsset("test");
        _state.NodeInfo.rect = new Rect(position.x, position.y, width, height);
        SaveChanges();
    }
    
    public StateNode(BaseState state, 
        GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle, 
        Action<ConnectionPoint> onClickInPoint, Action<ConnectionPoint> onClickOutPoint,
        Action<StateNode> onClickRemoveState, StateMachineEditor editor) 
        :this(nodeStyle, selectedStyle, inPointStyle, outPointStyle, onClickInPoint, onClickOutPoint, onClickRemoveState, editor)
    {
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
        if (_state.HasTransitions()) _outPoint.Draw();
        GUI.Box(_state.NodeInfo.rect, _state.name, _isSelected ? _selectedNodeStyle: _defaultNodeStyle);
        DrawDeleteButton();
        // DrawState();
        DrawFilters();
    }

    private void DrawDeleteButton()
    {
        if (!_isSelected) return;
        if(Handles.Button(new Vector3(_state.NodeInfo.rect.xMax, _state.NodeInfo.rect.yMin, 0), 
                Quaternion.identity, 4, 8, Handles.RectangleHandleCap))
            OnClickRemoveState?.Invoke(this);
    }

    private void DrawFilters()
    {
        Rect info = _state.NodeInfo.rect;
        float width = info.width * _filterScale.x;
        float height = info.height * _filterScale.y;
        float space = 10;
        Rect filterRect = new Rect(info.x, info.y - info.height * _filterScale.y, 
            width, height);
        foreach (var filter in _state.Filters)
        {
            GUI.Box(filterRect, filter.filterName, CreateFilterStyle(filter, (int)width, (int)height));
            filterRect.x += width + space;
        }
        GUI.changed = true;
    }
    
    private GUIStyle CreateFilterStyle(FSMFilter filter, int width, int height)
    {
        return new GUIStyle()
        {
            normal =
            {
                background = MakeTex(width, height, filter.color)
            },
            alignment = TextAnchor.MiddleLeft,
            clipping = TextClipping.Clip,
            
        };
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
                    _isSelected = contains;
                    if (_isSelected) 
                        if (AssetDatabase.OpenAsset(_state))
                        {
                            e.Use();
                            _editor.ClearSelectionExceptState(this);
                        }
                        else
                            Debug.LogError("state for this node cannot be opened.");
                    GUI.changed = contains;
                }
                break;

            case EventType.MouseUp:
                _isDragged = false;
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

    public void AddTransitionNode(TransitionNode transition)
    {
        _transitionNodes.Add(transition);
    }
    
    public void RemoveTransitionNode(TransitionNode transition)
    {
        _transitionNodes.Remove(transition);
    }
    
    public void ClearTransitions()
    {
        for (int i = _transitionNodes.Count-1; i >= 0; i--)
            _transitionNodes[i].RemoveTransition();
        _transitionNodes.Clear();
    }

    public void Deselect()
    {
        _isSelected = false;
    }

    private void SaveChanges()
    { 
        EditorUtility.SetDirty(_state);
    }
    
    private Texture2D MakeTex( int width, int height, Color col ) //via https://forum.unity.com/threads/change-gui-box-color.174609/
    {
        Color[] pix = new Color[width * height];
        for( int i = 0; i < pix.Length; ++i )
        {
            pix[ i ] = col;
        }
        Texture2D result = new Texture2D( width, height );
        result.SetPixels( pix );
        result.Apply();
        return result;
    }
}