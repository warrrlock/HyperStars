using System;
using UnityEditor;
using UnityEngine;

// based off of https://oguzkonya.com/creating-node-based-editor-unity/
public class StateNode
{
    private Rect _rect;
    public Rect Rect => _rect;
    private string _title;
    private bool _isDragged;

    private GUIStyle _style;
    public ConnectionPoint inPoint;
    public ConnectionPoint outPoint;

    public StateNode(Vector2 position, float width, float height, GUIStyle style, GUIStyle inPointStyle, GUIStyle outPointStyle, Action<ConnectionPoint> onClickInPoint, Action<ConnectionPoint> onClickOutPoint)
    {
        _rect = new Rect(position.x, position.y, width, height);
        _style = style;
        inPoint = new ConnectionPoint(this, ConnectionPointType.In, inPointStyle, onClickInPoint);
        outPoint = new ConnectionPoint(this, ConnectionPointType.Out, outPointStyle, onClickOutPoint);
    }

    public void Drag(Vector2 delta)
    {
        _rect.position += delta;
    }

    public void Draw()
    {
        inPoint.Draw();
        outPoint.Draw();
        GUI.Box(_rect, _title, _style);
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