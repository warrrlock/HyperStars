using System;
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
        _transition.NodePosition += delta;
    }
    
    public void Draw()
    {
        //TODO: change bezier curve instead of rect center, should smoothly connect at center of transition node
        _rect.center = (_inPoint.Rect.center + _outPoint.Rect.center) * 0.5f + _transition.NodePosition;
        DrawBezier();
        DrawNode();
        DrawDeleteButton();
        
        // if (Event.current.type == EventType.Repaint) _buttonRect = GUILayoutUtility.GetLastRect();
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
        Vector2 size = new Vector2(4, 4);
        if (GUI.Button(new Rect(_rect.min - size, size), 
                EditorGUIUtility.IconContent("CrossIcon"), EditorStyles.iconButton))
            RemoveTransition();
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
                            e.Use();
                        else
                            Debug.LogError("transition for this node cannot be opened.");
                        GUI.changed = true;
                    }
                }
                break;

            case EventType.MouseUp:
                if (_isCLicked && !_isDragged)
                {
                    GUI.changed = false;
                }
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