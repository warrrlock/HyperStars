using System;
using FiniteStateMachine;
using UnityEditor;
using UnityEngine;

// based off of https://oguzkonya.com/creating-node-based-editor-unity/
public class TransitionNode
{
    private Rect _rect;
    private const float Width = 100f;
    private const float Height = 30f;
    private string _title;
    
    private bool _isCLicked;
    private bool _isDragged;
    private bool _isSelected;
    private Rect _buttonRect;

    private const float BezierTangent = 25f;
    private const float BezierWidth = 2f;

    private GUIStyle _defaultStyle;
    private GUIStyle _selectedStyle;
    
    private ConnectionPoint _inPoint; //to state
    private ConnectionPoint _outPoint; //from state
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
        _rect.position += delta;
    }
    
    public void Draw()
    {
        Handles.DrawBezier(
            _outPoint.Rect.center + _outPoint.Rect.width/2*Vector2.right,
            _inPoint.Rect.center + _outPoint.Rect.width/2*Vector2.left,
            _outPoint.Rect.center + Vector2.right * BezierTangent,
            _inPoint.Rect.center + Vector2.left * BezierTangent,
            Color.white,
            null,
            BezierWidth
        );
        
        //TODO: change bezier curve instead of rect center, should smoothly connect at center of transition node
        _rect.center = (_inPoint.Rect.center + _outPoint.Rect.center) * 0.5f;
        DrawNode();
        DrawDeleteButton();
        
        // if (Event.current.type == EventType.Repaint) _buttonRect = GUILayoutUtility.GetLastRect();
    }

    public void DrawBezier()
    {
        Vector2 rectIn = _rect.center + Vector2.left * (_rect.width / 2 - 5f);
        Vector2 rectOut = _rect.center + Vector2.right * (_rect.width / 2 - 5f);

        Handles.DrawBezier(
            _outPoint.Rect.center + _outPoint.Rect.width/2*Vector2.right,
            rectIn,
            _outPoint.Rect.center + Vector2.right * BezierTangent,
            rectIn + Vector2.left * BezierTangent,
            Color.white,
            null,
            BezierWidth
        );
        
        Handles.DrawBezier(
            rectOut,
            _inPoint.Rect.center + _outPoint.Rect.width/2*Vector2.left,
            rectOut + Vector2.right * BezierTangent,
            _inPoint.Rect.center + Vector2.left * BezierTangent,
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
        if (Handles.Button(_rect.center, 
                Quaternion.identity, 4, 8, Handles.RectangleHandleCap))
        {
            RemoveTransition();
        }
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
                        _isCLicked = true;
                        
                        if (AssetDatabase.OpenAsset(_transition))
                        {
                            e.Use();
                            _editor.ClearSelectionExceptTransition(this);
                        }
                        else
                            Debug.LogError("transition for this node cannot be opened.");
                        GUI.changed = true;
                    }
                }
                break;

            case EventType.MouseUp:
                // Debug.Log("mouse up");
                if (_isCLicked && !_isDragged)
                {
                    // DisplayPopup(e);
                    GUI.changed = false;
                }
                _isCLicked = false;
                _isDragged = false;
                // Debug.Log($"ending displaying popup, {_isCLicked}, {_isDragged}");
                break;

            case EventType.MouseDrag:
                if (e.button == 0 && _isCLicked)
                {
                    // Debug.Log("mouse dragged, left mouse button");
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
}