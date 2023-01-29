
using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class AssetNode
{
    private Object _asset;
    public Object Asset => _asset;
    private AssetFinderWindow _window;
    private readonly Color _highlightColor = new (.24f, .38f, .57f, 1f);
    private Rect _rect;
    private Texture _selectedTex = EditorGUIUtility.IconContent("d_ScriptableObject Icon").image;
    private Texture _defaultTex = EditorGUIUtility.IconContent("d_ScriptableObject On Icon").image;

    public bool IsSelected => _window.SelectedAsset == _asset;

    public AssetNode(Object asset, AssetFinderWindow window)
    {
        _asset = asset; 
        _window = window;
        _rect = Rect.zero;
    }
    
    public bool ProcessEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0 && _rect.Contains(e.mousePosition))
                {
                    _window.SelectAsset(_asset);
                    e.Use();
                    return true;
                }
                break;
        }
        return false;
    }
    
    public void Draw(Rect r, float additionalY)
    {
        _rect = r;
        if (IsSelected)
        {
            r.y -= additionalY;
            EditorGUI.DrawRect(r, _highlightColor);
        }
        EditorGUILayout.LabelField(new GUIContent($"\t{(_asset ? _asset.name : "None")}", IsSelected ? _selectedTex : _defaultTex));
    }
    
    private Texture GetIcon()
    {
        GUIContent icon =  IsSelected ? EditorGUIUtility.IconContent("d_ScriptableObject Icon")
            : EditorGUIUtility.IconContent("d_ScriptableObject On Icon");
        return icon.image;
    }
    
}
