using System;
using UnityEngine;

public enum ConnectionPointType { In, Out }

public class ConnectionPoint
{
    private Rect _rect;
    public Rect Rect => _rect;
    
    private float _height = 20f;
    private float _width = 10f;

    private ConnectionPointType _type;

    private StateNode _node;
    public StateNode Node => _node;

    private GUIStyle _style;

    public Action<ConnectionPoint> OnClickConnectionPoint;
    
    public ConnectionPoint(StateNode node, ConnectionPointType type, GUIStyle style, Action<ConnectionPoint> onClickConnectionPoint)
    {
        _node = node;
        _type = type;
        _style = style;
        OnClickConnectionPoint = onClickConnectionPoint;
        _rect = new Rect(0, 0, _width, _height);
        
    }

    public void Draw()
    {
        float cy = _node.Rect.center.y;
        float cx = _node.Rect.center.x + (_node.Rect.width/2) * (_type == ConnectionPointType.In? -1: 1);
        _rect.center = new Vector2(cx, cy);
        
        if (GUI.Button(_rect, "", _style))
        {
            OnClickConnectionPoint?.Invoke(this);
        }
    }
}