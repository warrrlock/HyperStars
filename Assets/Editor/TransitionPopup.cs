using UnityEngine;
using UnityEditor;

public class TransitionPopup : PopupWindowContent
{
    bool toggle1 = true;
    bool toggle2 = true;
    bool toggle3 = true;
    
    public override Vector2 GetWindowSize()
    {
        return new Vector2(200, 150);
    }

    public override void OnGUI(Rect rect)
    {
        GUILayout.Label("Popup Options Example", EditorStyles.boldLabel);
        toggle1 = EditorGUILayout.Toggle("Toggle 1", toggle1);
        toggle2 = EditorGUILayout.Toggle("Toggle 2", toggle2);
        toggle3 = EditorGUILayout.Toggle("Toggle 3", toggle3);
    }

    public override void OnOpen()
    {
        Debug.Log("Popup opened: " + this);
    }

    public override void OnClose()
    {
        Debug.Log("Popup closed: " + this);
    }
}