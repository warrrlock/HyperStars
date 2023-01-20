using System;
using System.Collections.Generic;
using System.Linq;
using FiniteStateMachine;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class AssetFinderWindow: EditorWindow
{
    private Object _selectedAsset;
    public Object SelectedAsset => _selectedAsset;
    
    private string _originPath;
    private Action<Object> _onSelectAction;
    
    private List<AssetNode> _filteredAssets = new List<AssetNode>();
    private Dictionary<Object, AssetNode> _nodes = new Dictionary<Object, AssetNode>();
    private AssetNode _noneNode;

    private string _searchText;
    private Vector2 _scrollPos;
    private Vector2 _windowSize;
    private Vector2 _componentRatio = new Vector2(0.9f, 0.8f);
    
    private readonly Color _darkColor = new (.16f, .16f, .16f);
    private const float NodeHeight = 20;
    

    public static AssetFinderWindow CreateFinderWindow(Vector2 editorPosition, Vector2 editorSize, string path, Type type, Action<Object> action)
    {
        AssetFinderWindow window = CreateInstance(typeof(AssetFinderWindow)) as AssetFinderWindow;
        
        window._originPath = path; 
        window._noneNode = new AssetNode(null, window);
        window.GetAssetsOfTypeFromPath(path, type);
        window._filteredAssets = window._nodes.Values.ToList();
        window.titleContent = new GUIContent("Asset Finder");
        window.maxSize = editorSize;
        window.minSize = editorSize;
        window._windowSize = editorSize;
        window._onSelectAction = action;
        
        window.ShowUtility();
        return window;
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginVertical();
        Rect r = DrawSearchBar();
        DrawStates(r.height);
        DrawSelectedInformation();
        EditorGUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        
        ProcessEvents(Event.current);
    }

    public void SelectAsset(Object asset)
    {
        _selectedAsset = asset;
        _onSelectAction?.Invoke(asset);
    }
    
    private void ProcessEvents(Event e)
    {
        if (_filteredAssets == null) return;
        for (int i = -1; i < _filteredAssets.Count; i++)
        {
            AssetNode node = i < 0 ? _noneNode : _filteredAssets[i];
            if (node.ProcessEvents(e))
            {
                GUI.changed = true;
            }
        }
    }

    private void GetAssetsOfTypeFromPath(string path, Type t)
    {
        string[] guids = AssetDatabase.FindAssets($"t:{t.Name}",new[] { path });
        Object[] assets = 
            guids.Select(guid => (Object)AssetDatabase.LoadAssetAtPath(
                AssetDatabase.GUIDToAssetPath(guid), t)).ToArray();
        Debug.Log($"getting assets at {path}, of type {t}, found {guids.Length} assets");
        foreach (Object obj in assets)
            _nodes.Add(obj, new AssetNode(obj, this));
    }

    private Rect DrawSearchBar()
    {
        Rect r = EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(_windowSize.x));
        _searchText = EditorGUILayout.TextField(_searchText, EditorStyles.toolbarSearchField);
        if (GUILayout.Button("search", EditorStyles.miniButton, GUILayout.Width(100)))
        {
            //TODO: create filtered search
        }
        EditorGUILayout.EndHorizontal();
        return r;
    }
    
    private void DrawStates(float additionalY)
    {
        if (_filteredAssets == null) return;
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos,
            GUILayout.MaxWidth(_windowSize.x), GUILayout.Height(_windowSize.y * _componentRatio.y * 0.8f));
        
        for (int i = -1; i < _filteredAssets.Count; i++)
        {
            Rect rect = EditorGUILayout.BeginHorizontal(GUILayout.Height(NodeHeight));
            AssetNode node = i < 0 ? _noneNode : _filteredAssets[i];
            rect.y += additionalY;
            node.Draw(rect, additionalY);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawSelectedInformation()
    {
        Rect r = EditorGUILayout.BeginVertical(GUILayout.MaxWidth(_windowSize.x), 
            GUILayout.MaxHeight(_windowSize.y));
        EditorGUI.DrawRect(r, _darkColor);
        EditorGUILayout.SelectableLabel($"{(_selectedAsset ? _selectedAsset.name : "None")}" +
                                        $"\nat path: {(_selectedAsset ? AssetDatabase.GetAssetPath(_selectedAsset) : "")}");
        EditorGUILayout.EndVertical();
    }
}
