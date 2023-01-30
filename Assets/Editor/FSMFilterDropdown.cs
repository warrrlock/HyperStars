
using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using UnityEditor;
using UnityEngine;

public class FSMFilterDropdown: PopupWindowContent
{
    private List<FSMFilter> _options;
    private StateMachineEditor _editor;

    private GUIContent _addSymbol;
    private GUIContent _removeSymbol;

    private Action<FSMFilter> _selectAction;
    private Action<FSMFilter> _deselectAction;

    private const float NodeHeight = 25f;
    private Vector2 _scrollPosition;

    public FSMFilterDropdown(List<FSMFilter> options, IComparer<FSMFilter> comparer, 
        Action<FSMFilter> selectAction, Action<FSMFilter> deselectAction, StateMachineEditor editor)
    {
        _options = options ?? new ();
        _options.Sort(comparer);
        _selectAction = selectAction;
        _deselectAction = deselectAction;
        _editor = editor;
    }
    
    public override Vector2 GetWindowSize()
    {
        return new Vector2(300, 150);
    }

    public override void OnOpen()
    {
        _addSymbol = EditorGUIUtility.IconContent("d_Toolbar Plus");
        _removeSymbol = EditorGUIUtility.IconContent("d_Toolbar Minus");
    }

    public override void OnGUI(Rect rect)
    {
        Draw();
    }
    
    public void Draw()
    {
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        DrawSelected();
        DrawNotSelected();
        EditorGUILayout.EndScrollView();
    }

    private void DrawSelectNode(FSMFilter selection, Texture symbol, Action a)
    {
        EditorGUILayout.BeginHorizontal("box");
        if (GUILayout.Button(new GUIContent(symbol), GUILayout.Height(NodeHeight), GUILayout.Width(NodeHeight))) a?.Invoke();
        EditorGUILayout.LabelField(selection.ToString());
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawSelected()
    {
        for (int i = 0; i < _editor.SelectedFilters.Count; i++)
        {
            FSMFilter filter = _editor.SelectedFilters[i];
            DrawSelectNode(filter, _removeSymbol.image, () => _deselectAction?.Invoke(filter));
        }
    }

    private void DrawNotSelected()
    {
        EditorGUILayout.LabelField("available filters");
        for (int i = 0; i < _options.Count; i++)
        {
            FSMFilter filter = _options[i];
            if (!_editor.SelectedFilters.Contains(filter)) DrawSelectNode(filter, _addSymbol.image, () => _selectAction.Invoke(filter));
        }
    }
}
