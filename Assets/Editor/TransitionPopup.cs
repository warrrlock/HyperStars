using UnityEngine;
using UnityEditor;

public class TransitionPopup : PopupWindowContent
{
    private Vector2 _size;
    bool toggle1 = true;
    bool toggle2 = true;
    bool toggle3 = true;
    
    public override Vector2 GetWindowSize()
    {
        return _size;
    }

    public TransitionPopup(Vector2 size)
    {
        _size = size;
    }

    public override void OnGUI(Rect rect)
    {
        GUILayout.Label("transition popup", EditorStyles.boldLabel);
        toggle1 = EditorGUILayout.Toggle("Toggle 1", toggle1);
        toggle2 = EditorGUILayout.Toggle("Toggle 2", toggle2);
        toggle3 = EditorGUILayout.Toggle("Toggle 3", toggle3);
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
}