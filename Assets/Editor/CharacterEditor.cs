using System.Text.RegularExpressions;
using Managers;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Character))]
public class CharacterEditor : Editor
{
    private string createName = "";
    private Color createColor = Color.white;
    private int test;
    private bool createFilterButton;
    private string[] buttonText = new[] {"create new filter", "cancel"};
    private int textIndex;
    private string pattern = "^[a-zA-Z {1}-]+$";
    private Vector2 scrollPos;
    private string editName;
    private Color editColor;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Character character = (Character)target;
        
        EditorGUILayout.Space(20);
        DrawFilters();
        DrawRefresh(character);
        EditorGUILayout.Space(20);
        
        DrawCreateButton();
        if (createFilterButton)
            DrawCreateFilter();
    }

    private void DrawCreateButton()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(buttonText[textIndex], GUILayout.MaxWidth(300)))
        {
            if (createFilterButton) ResetCreationValues();
            createFilterButton = !createFilterButton;
            textIndex ^= 1;
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawCreateFilter()
    {
        EditorGUILayout.Space();
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("name");
        createName = EditorGUILayout.TextField(createName);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        createColor = EditorGUILayout.ColorField("color", createColor);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("create filter", GUILayout.MaxWidth(100)))
        {
            if (CreateNewFSMFilter(createName, createColor))
            {
                EditorGUI.FocusTextInControl("");
                ResetCreationValues();
            }
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawFilters()
    {
        EditorGUILayout.LabelField("Filters");
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginVertical("Box");
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos,
            GUILayout.Width(210),
            GUILayout.Height(150));
        
        Character character = (Character)target;
        for (int i = 0; i < character.Filters.Count; i++)
        {
            FSMFilter filter = character.Filters[i];
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal(GUILayout.Width(200));
            if (!filter.edit && GUILayout.Button(filter.filterName))
            {
                foreach (FSMFilter f in character.Filters)
                    if (f != filter) CancelFocus(f);
                filter.focus = true;
            }
            else if (filter.edit)
            {
                editName = GUILayout.TextField(editName);
            }
            Color c = EditorGUILayout.ColorField(new GUIContent(""), filter.edit ? editColor : filter.color, 
                filter.edit, true, false, GUILayout.Width(50));
            if (filter.edit) editColor = c;
            EditorGUILayout.EndHorizontal();
            if (filter.focus)
                DrawFilterFocused(filter);
            else if (filter.edit) 
                DrawFilterEdit(filter);
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawRefresh(Character character)
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Refresh", GUILayout.MaxWidth(210)))
            character.OnEnable();
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawFilterFocused(FSMFilter filter)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(25);
        if (GUILayout.Button("delete", GUILayout.Width(50)))
        {
            DeleteFSMFilter(filter);
        }
        if (GUILayout.Button("edit", GUILayout.Width(50)))
        {
            filter.focus = false;
            filter.edit = true;
            editName = filter.filterName;
            editColor = filter.color;
        }
        if (GUILayout.Button("cancel", GUILayout.Width(50)))
            CancelFocus(filter);
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawFilterEdit(FSMFilter filter)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(25);
        if (GUILayout.Button("save", GUILayout.Width(50)))
        {
            EditFSMFilter(filter);
            CancelFocus(filter);
            ResetEditValues();
        }

        if (GUILayout.Button("cancel", GUILayout.Width(50)))
        {
            CancelFocus(filter);
            ResetEditValues();
        }
            
        EditorGUILayout.EndHorizontal();
    }

    private void CancelFocus(FSMFilter filter)
    {
        filter.focus = false;
        filter.edit = false;
    }

    private bool CreateNewFSMFilter(string n, Color c)
    {
        if (!Regex.IsMatch(n, pattern))
        {
            Debug.LogError($"error creating new FSM filter for {target.name}." +
                           $"\nPlease check filter name consists of only letters, single spaces, and hyphens.");
            return false;
        }
        
        bool r = ((Character)target).AddFilter(n, c);
        if (!r)
            Debug.LogError($"error creating new FSM filter for {target.name}." +
                           $"\nThe wanted filter name already exists for this character.");
        return r;
    }

    private void DeleteFSMFilter(FSMFilter filter)
    {
        Character character = (Character)target;
        if (!((Character)target).DeleteFilter(filter))
            Debug.LogError($"error deleting {filter.filterName} for {target.name}." +
                           $"\nThe filter does not exist.");
    }
    
    private void EditFSMFilter(FSMFilter filter)
    {
        if (!((Character)target).EditFilter(filter, editName, editColor))
            Debug.LogError($"error editing {filter.filterName} for {target.name}." +
                           $"\nThe filter does not exist.");
    }

    private void ResetCreationValues()
    {
        createName = "";
        createColor = Color.white;
    }
    private void ResetEditValues()
    {
        editName = "";
        editColor = Color.white;
    }
}