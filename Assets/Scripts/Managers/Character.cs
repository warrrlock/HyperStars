using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using System.Text.RegularExpressions;
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
    
    class FilterEqualityComparer : IEqualityComparer<FSMFilter>
    {
        public bool Equals(FSMFilter a, FSMFilter b)
        {
            return b != null && a != null && a.filterName.Equals(b.filterName);
        }

        public int GetHashCode(FSMFilter obj)
        {
            return StringComparer.InvariantCultureIgnoreCase.GetHashCode(obj.filterName);
        }
    }
    
    [CreateAssetMenu(menuName = "StateMachine/Character")]
    public class Character: ScriptableObject
    {
        [HideInInspector][SerializeField]private List<FSMFilter> _filters = new();
        private HashSet<FSMFilter> _filtersSet = new(new FilterEqualityComparer());
        public HashSet<FSMFilter> FiltersSet => _filtersSet;
        public List<FSMFilter> Filters => _filters;
        [SerializeField] private GameObject _characterPrefab;

        private void OnEnable()
        {
            _filtersSet = new HashSet<FSMFilter>(_filters, new FilterEqualityComparer());
        }
        
        public bool AddFilter(string n, Color c)
        {
            FSMFilter f = new FSMFilter(n, c);
            bool r = _filtersSet.Add(f);
            if (r)
            {
                _filters.Add(f);
                SaveChanges();
            }
            return r;
        }

        public bool DeleteFilter(FSMFilter f)
        {
            bool r = _filtersSet.Remove(f);
            if (r)
            {
                _filters.Remove(f);
                SaveChanges();
            }
            return r;
        }

        public bool EditFilter(FSMFilter f, string n, Color c)
        {
            if (!_filters.Contains(f)) return false;
            f.filterName = n;
            f.color = c;
            SaveChanges();
            return true;
        }
        
        private void SaveChanges()
        { 
            EditorUtility.SetDirty(this);
        }
         
        //TODO: limit filter selection for states to its character
        //TODO: when deleting a filter, go through folder and move assets to correct folder
    }
}