using System;
using UnityEditor;
using UnityEngine;

// based off of https://oguzkonya.com/creating-node-based-editor-unity/
public class TransitionNode
{
    private Rect _rect;
    private string _title;
    private bool _isDragged;

    private float _bezierTangent = 25f;
    private float _bezierWidth = 2f;

    private GUIStyle _style;
    
    private ConnectionPoint _inPoint; //to state
    private ConnectionPoint _outPoint; //from state
    private Action<TransitionNode> OnClickRemoveTransition;

    private float _width = 100f;
    private float _height = 30f;

    public TransitionNode(GUIStyle style, 
        ConnectionPoint to, ConnectionPoint from, Action<TransitionNode> onClickRemoveTransition)
    {
        _inPoint = to;
        _outPoint = from;
        OnClickRemoveTransition = onClickRemoveTransition;
        
        _rect = new Rect(0, 0, _width, _height);
        _rect.center = (_inPoint.Rect.center + _outPoint.Rect.center) * 0.5f;
        _style = style;
    }
    
    public void Drag(Vector2 delta)
    {
        _rect.position += delta;
    }

    Rect buttonRect;
    public void Draw()
    {
        Handles.DrawBezier(
            _outPoint.Rect.center + _outPoint.Rect.width/2*Vector2.right,
            _inPoint.Rect.center + _outPoint.Rect.width/2*Vector2.left,
            _outPoint.Rect.center + Vector2.right * _bezierTangent,
            _inPoint.Rect.center + Vector2.left * _bezierTangent,
            Color.white,
            null,
            _bezierWidth
        );
        
        //TODO: change bezier curve instead of rect center, should smoothly connect at center of transition node
        _rect.center = (_inPoint.Rect.center + _outPoint.Rect.center) * 0.5f;
        DrawBox();
        DrawDeleteButton();
        
        //TODO: show transition popup when clicking the node at any location
        GUILayout.Label("Editor window with Popup example", EditorStyles.boldLabel);
        if (GUILayout.Button("Popup Options", GUILayout.Width(200)))
        {
            PopupWindow.Show(_rect, new TransitionPopup());
        }
        if (Event.current.type == EventType.Repaint) buttonRect = GUILayoutUtility.GetLastRect();
    }

    public void DrawBezier()
    {
        Vector2 rectIn = _rect.center + Vector2.left * (_rect.width / 2 - 5f);
        Vector2 rectOut = _rect.center + Vector2.right * (_rect.width / 2 - 5f);

        Handles.DrawBezier(
            _outPoint.Rect.center + _outPoint.Rect.width/2*Vector2.right,
            rectIn,
            _outPoint.Rect.center + Vector2.right * _bezierTangent,
            rectIn + Vector2.left * _bezierTangent,
            Color.white,
            null,
            _bezierWidth
        );
        
        Handles.DrawBezier(
            rectOut,
            _inPoint.Rect.center + _outPoint.Rect.width/2*Vector2.left,
            rectOut + Vector2.right * _bezierTangent,
            _inPoint.Rect.center + Vector2.left * _bezierTangent,
            Color.white,
            null,
            _bezierWidth
        );
    }
    
    private void DrawBox()
    {
        GUI.Box(_rect, _title, _style);
    }

    private void DrawDeleteButton()
    {
        if (Handles.Button(_rect.center, 
                Quaternion.identity, 4, 8, Handles.RectangleHandleCap))
        {
            OnClickRemoveTransition?.Invoke(this);
        }
    }

    public bool ProcessEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    if (_rect.Contains(e.mousePosition))
                        _isDragged = true;
                    GUI.changed = true;
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
}