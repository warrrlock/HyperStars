
    using System;
    using FiniteStateMachine;
    using UnityEditor;
    using UnityEngine;

    public class StateNodePopup : EditorWindow
    {
        private Action[] _drawActions;
        public StateNode StateNode { get; set; }

        public static StateNodePopup ActivatePopup(Action[] drawActions, Vector2 editorPosition, Vector2 editorSize, StateNode stateNode)
        {
            StateNodePopup window = CreateInstance(typeof(StateNodePopup)) as StateNodePopup;
            window._drawActions = drawActions;
            window.StateNode = stateNode;
            window.titleContent = new GUIContent("State Node");
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