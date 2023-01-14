using System;
using System.Collections.Generic;
using System.Linq;
using FiniteStateMachine;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Managers
{
    [Serializable]
    public class FSMFilter
    {
        public string filterName;
        public Color color;
        public bool focus;
        public bool edit;

        public FSMFilter(string n, Color c)
        {
            filterName = n;
            color = c;
        }
    }

    public class FilterEqualityComparer : IEqualityComparer<FSMFilter>
    {
        public bool Equals(FSMFilter a, FSMFilter b)
        {
            return b != null && a != null && a.filterName.Equals(b.filterName);
        }

        public int GetHashCode(FSMFilter obj)
        {
            return obj.ToString().GetHashCode();
        }
    }
    
    [CreateAssetMenu(menuName = "ScriptableObjects/Character")]
    public class Character: ScriptableObject
    {
        [HideInInspector][SerializeField]private List<FSMFilter> _filters = new();
        private HashSet<FSMFilter> _filtersSet = new(new FilterEqualityComparer());
        public List<FSMFilter> Filters => _filters;
        [SerializeField] private GameObject _characterPrefab;
        private string _originPath = "Assets/Scriptable Objects/[TEST] editor";

        public void OnEnable()
        {
            string[] folders = AssetDatabase.GetSubFolders(_originPath);
            List<string> folderNames = new ();
            foreach (string folder in folders)
            {
                string folderName = folder.Split('/').Last();
                if (folderName.Equals("multi-filtered") || folderName.Equals("unfiltered")) continue;
                folderNames.Add(folderName);
                FSMFilter newFilter = new FSMFilter(folderName, Color.white);
                if (!_filters.Contains(newFilter, new FilterEqualityComparer()))
                    _filters.Add(newFilter);
            }

            for (int i = _filters.Count - 1; i >= 0; i--)
            {
                if (!folderNames.Contains(_filters[i].filterName))
                    _filters.Remove(_filters[i]);
            }
            _filtersSet = new HashSet<FSMFilter>(_filters, new FilterEqualityComparer());
        }
        
        public bool AddFilter(string n, Color c)
        {
            FSMFilter f = new FSMFilter(n, c);
            bool r = _filtersSet.Add(f);
            if (r)
            {
                _filters.Add(f);
                AssetDatabase.CreateFolder(_originPath, f.filterName);
                SaveChanges();
            }
            return r;
        }

        public bool DeleteFilter(FSMFilter f) //remove filter from all states in folder
        {
            // bool r = _filtersSet.RemoveWhere(o => o.filterName.Equals(f.filterName)) > 0;
            bool r = _filtersSet.Remove(f);
            if (r)
            {
                _filters.Remove(f);
                
                string[] guids = AssetDatabase.FindAssets("t:BaseState",
                    new[] {_originPath}); //TODO: find only from filter folder, and multi filter folder
                BaseState[] states = 
                    guids.Select(guid => (BaseState)AssetDatabase.LoadAssetAtPath(
                        AssetDatabase.GUIDToAssetPath(guid), typeof(BaseState))).ToArray();
                for (int i = 0; i < guids.Length; i++)
                {
                    states[i].RemoveFilter(f);
                    if (true)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                        string ending = $"{states[i].name}.asset";
                        
                        if (states[i].Filters.Count == 0) 
                            AssetDatabase.MoveAsset(path, $"{_originPath}/{ending}");
                        else if (states[i].Filters.Count == 1) 
                            AssetDatabase.MoveAsset(path,$"{_originPath}/{states[i].Filters[0].filterName}/{ending}");
                    }
                }
                SaveChanges();
                AssetDatabase.MoveAssetToTrash($"{_originPath}/{f.filterName}");
                SaveChanges();
            }
            return r;
        }

        public bool EditFilter(FSMFilter f, string n, Color c)
        {
            if (!_filters.Contains(f)) return false;
            AssetDatabase.MoveAsset($"{_originPath}/{f.filterName}", $"{_originPath}/{n}");
            f.filterName = n;
            f.color = c;
            SaveChanges();
            return true;
        }
        
        private void SaveChanges()
        { 
            EditorUtility.SetDirty(this);
            AssetDatabase.Refresh();
        }
         
        //TODO: limit filter selection for states to its character
        //TODO: when deleting a filter, go through folder and move assets to correct folder
    }
}