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
        [SerializeField] private GameObject _characterPrefab;

        private void OnEnable()
        {
            _filtersSet = new HashSet<FSMFilter>(_filters, new FilterEqualityComparer());
        }
        
        public bool AddFilter(string n, Color c)
        {
            FSMFilter f = new FSMFilter(n, c);
            bool r = _filtersSet.Add(f);
            if (r) _filters.Add(f);
            return r;
        }

        public bool DeleteFilter(FSMFilter f)
        {
            bool r = _filtersSet.Remove(f);
            if (r) _filters.Remove(f);
            return r;
        }

        public void EditFilter(FSMFilter f, string n, Color c)
        {
            f.filterName = n;
            f.color = c;
        }
         
        //TODO: limit filter selection for states to its character
        //TODO: when deleting a filter, go through folder and move assets to correct folder
    }
    
    
    #region Editor
#if UNITY_EDITOR
        [CustomEditor(typeof(Character))]
        class CharacterEditor : Editor
        {
            private string name = "";
            private Color color = Color.white;
            private int test;
            private bool showButton;
            private bool showFilters;
            private string[] buttonText = new[] {"create new filter", "cancel"};
            private int textIndex;
            private string pattern = "^[a-zA-Z {1}-]+$";
            
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                Character character = (Character)target;
                EditorGUILayout.Space(20);
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(buttonText[textIndex], GUILayout.MaxWidth(300)))
                {
                    if (showButton) ResetValues();
                    showButton = !showButton;
                    showFilters = !showFilters; //TODO: move to own button
                    textIndex ^= 1;
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                
                if (showButton)
                    Draw();
                if (showFilters)
                    DrawFilters();
            }

            private void Draw()
            {
                EditorGUILayout.Space();
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("name");
                name = EditorGUILayout.TextField(name);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                color = EditorGUILayout.ColorField("color", color);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space();
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("create filter", GUILayout.MaxWidth(100)))
                {
                    if (CreateNewFSMFilter(name, color))
                    {
                        //TODO: de-highlight name text box
                        EditorGUI.FocusTextInControl("");
                        ResetValues();
                    }
                    else Debug.LogError($"error creating new FSM filter for {target.name}." +
                                        $"\nPlease check filter name consists of only letters, single spaces, and hyphens.");
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            private void DrawFilters()
            {
                //TODO: draw in scrollview
                Character character = (Character)target;
                EditorGUILayout.Space(20);
                EditorGUILayout.BeginVertical("box");
                foreach (FSMFilter filter in character.FiltersSet)
                {
                    EditorGUILayout.BeginHorizontal("box");
                    GUILayout.Label(filter.filterName);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.ColorField(filter.color);
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }

            private bool CreateNewFSMFilter(string n, Color c)
            {
                if (!Regex.IsMatch(n, pattern)) 
                    return false;
                if (!((Character)target).AddFilter(n, c))
                    Debug.LogError($"error creating new FSM filter for {target.name}." +
                                   $"\nThe wanted filter name already exists for this character.");
                return true;
            }

            private void ResetValues()
            {
                name = "";
                color = Color.white;
            }
        }
#endif
        #endregion
}