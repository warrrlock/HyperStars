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
    [CreateAssetMenu(menuName = "ScriptableObjects/Character")]
    public class Character: ScriptableObject
    {
        [SerializeField] private GameObject _characterPrefab;
        
        [HideInInspector][SerializeField]private List<FSMFilter> _filters = new();
        private HashSet<FSMFilter> _filtersSet = new(new FSMFilterEqualityComparer());
        public List<FSMFilter> Filters => _filters;
        
        private string _originPath = "Assets/Scriptable Objects/[TEST] editor";
        [SerializeField]private string _characterPath = "";
        
        public void OnEnable()
        {
            CreateCharacterFolder();
            if (_characterPath.Equals("")) return;
            string[] folders = AssetDatabase.GetSubFolders(_characterPath);
            List<string> folderNames = new ();
            foreach (string folder in folders)
            {
                string folderName = folder.Split('/').Last();
                if (folderName.Equals("multi-filtered") 
                    || folderName.Equals("unfiltered") 
                    || folderName.Equals("actions")
                    || folderName.Equals("transitions")) continue;
                folderNames.Add(folderName);
                FSMFilter newFilter = new FSMFilter(folderName, Color.white);
                if (!_filters.Contains(newFilter, new FSMFilterEqualityComparer()))
                    _filters.Add(newFilter);
            }

            for (int i = _filters.Count - 1; i >= 0; i--)
            {
                if (!folderNames.Contains(_filters[i].filterName))
                    _filters.Remove(_filters[i]);
            }
            _filtersSet = new HashSet<FSMFilter>(_filters, new FSMFilterEqualityComparer());
        }

        private void CreateCharacterFolder()
        {
            if (this.name.Equals("") || AssetDatabase.IsValidFolder($"{_originPath}/{this.name}")) return;
            _characterPath = $"{_originPath}/{this.name}";
            System.IO.Directory.CreateDirectory(_characterPath);
            System.IO.Directory.CreateDirectory($"{_characterPath}/unfiltered");
            System.IO.Directory.CreateDirectory($"{_characterPath}/multi-filtered");
            System.IO.Directory.CreateDirectory($"{_characterPath}/transitions");
            System.IO.Directory.CreateDirectory($"{_characterPath}/actions");
            
            Debug.Log($"creating directories at path {_characterPath}\nfrom {System.IO.Directory.GetCurrentDirectory()}");
            AssetDatabase.Refresh();
            AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(this), $"{_characterPath}/{this.name}.asset");
            AssetDatabase.Refresh();
        }

        public bool AddFilter(string n, Color c)
        {
            if (_characterPath.Equals("")) OnEnable(); //create folders & set character path
            FSMFilter f = new FSMFilter(n, c);
            bool r = _filtersSet.Add(f);
            if (r)
            {
                _filters.Add(f);
                _filters.Sort(); //TODO: optimize
                AssetDatabase.CreateFolder(_characterPath, f.filterName);
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
                    new[] {_characterPath}); //TODO: find only from filter folder, and multi filter folder
                BaseState[] states = 
                    guids.Select(guid => (BaseState)AssetDatabase.LoadAssetAtPath(
                        AssetDatabase.GUIDToAssetPath(guid), typeof(BaseState))).ToArray();
                for (int i = 0; i < guids.Length; i++)
                {
                    if (states[i].RemoveFilter(f))
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                        string ending = $"{states[i].name}.asset";
                        
                        if (states[i].Filters.Count == 0) 
                            AssetDatabase.MoveAsset(path, $"{_characterPath}/unfiltered/{ending}");
                        else if (states[i].Filters.Count == 1) 
                            AssetDatabase.MoveAsset(path,$"{_characterPath}/{states[i].Filters[0].filterName}/{ending}");
                    }
                }
                SaveChanges();
                AssetDatabase.MoveAssetToTrash($"{_characterPath}/{f.filterName}");
                SaveChanges();
            }
            return r;
        }

        public bool EditFilter(FSMFilter f, string n, Color c)
        {
            if (!_filters.Contains(f)) return false;
            AssetDatabase.MoveAsset($"{_characterPath}/{f.filterName}", $"{_characterPath}/{n}");
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