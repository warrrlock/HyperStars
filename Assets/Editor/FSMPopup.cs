using System;
using FiniteStateMachine;
using UnityEditor;
using UnityEngine;

public class FSMPopup : EditorWindow
{
    private Action[] _drawActions;

    public static FSMPopup ActivatePopup(Action[] drawActions, Vector2 editorSize, string title)
    {
        FSMPopup window = CreateInstance(typeof(FSMPopup)) as FSMPopup;
        window._drawActions = drawActions;
        window.titleContent = new GUIContent(title);
        window.maxSize = editorSize;
        window.minSize = editorSize;
        window.ShowUtility();
        return window;
    }

    private void OnGUI()
    {
        foreach (var action in _drawActions)
        {
            action?.Invoke();
        }
    }
}