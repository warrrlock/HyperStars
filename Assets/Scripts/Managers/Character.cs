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
        public GameObject CharacterPrefab => _characterPrefab;
        
        [HideInInspector][SerializeField]private List<FSMFilter> _filters = new();
        private HashSet<FSMFilter> _filtersSet = new(new FSMFilterEqualityComparer());
        public List<FSMFilter> Filters => _filters;
        
        private string _originPath = "Assets/Scriptable Objects/State Machine";
        [HideInInspector][SerializeField]private string _characterPath = "";
        public string CharacterPath => _characterPath;

        public IReadOnlyList<BaseState> States => _states;
        public IReadOnlyList<AttackInfo> AttackInfos => _attackInfo;
        [HideInInspector] [SerializeField] List<BaseState> _states;
        [HideInInspector] [SerializeField] List<AttackInfo> _attackInfo;

        public Sprite IndicatorSprite => _indicatorSprite;
        [SerializeField] private Sprite _indicatorSprite;
        public Sprite IndicatorGlowSprite => _indicatorGlowSprite;
        [SerializeField] private Sprite _indicatorGlowSprite;

        public Sprite[] CharacterPortrait => _characterPortrait;
        [SerializeField] private Sprite[] _characterPortrait;
        [field: SerializeField] public CharacterColorScriptable CharacterPalettes { get; private set; }

        public HurtState LoseState => _loseState;
        [SerializeField] private HurtState _loseState;

#if UNITY_EDITOR
        public void OnEnable()
        {
            CreateCharacterFolder();
            if (_characterPath.Equals("")) SetCharacterPath();
            if (_characterPath.Equals("")) return;

            CreateStateList();
            CreateAttackInfoList();
            
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
                {
                    AssetDatabase.CreateFolder(_characterPath, _filters[i].filterName);
                    //_filters.Remove(_filters[i]);
                }
            }
            _filtersSet = new HashSet<FSMFilter>(_filters, new FSMFilterEqualityComparer());
        }

        private void SetCharacterPath()
        {
            _characterPath = $"{_originPath}/{this.name}";
        }

        private void CreateCharacterFolder()
        {
            if (this.name.Equals("")) return;
            SetCharacterPath();
            if (!AssetDatabase.IsValidFolder(_characterPath))
            {
                System.IO.Directory.CreateDirectory(_characterPath);
                AssetDatabase.Refresh();
                AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(this), $"{_characterPath}/{this.name}.asset");
            }
            if (!AssetDatabase.IsValidFolder($"{_characterPath}/unfiltered"))
                System.IO.Directory.CreateDirectory($"{_characterPath}/unfiltered");
            if (!AssetDatabase.IsValidFolder($"{_characterPath}/multi-filtered"))
                System.IO.Directory.CreateDirectory($"{_characterPath}/multi-filtered");
            if (!AssetDatabase.IsValidFolder($"{_characterPath}/transitions"))
                System.IO.Directory.CreateDirectory($"{_characterPath}/transitions");
            if (!AssetDatabase.IsValidFolder($"{_characterPath}/actions"))
                System.IO.Directory.CreateDirectory($"{_characterPath}/actions");
            
            AssetDatabase.Refresh();
        }

        private void CreateStateList()
        {
            string[] guids = AssetDatabase.FindAssets("t:BaseState",new[] { _characterPath });
            _states = 
                guids.Select(guid => (BaseState)AssetDatabase.LoadAssetAtPath(
                    AssetDatabase.GUIDToAssetPath(guid), typeof(BaseState))).ToList();
        }

        private void CreateAttackInfoList()
        {
            _attackInfo = new List<AttackInfo>();
            int idSO = 0;
            foreach (BaseState _state in _states)
            {
                AttackInfo attackInfo = _state.GetAttackInfo();
                if (attackInfo != null)
                {
                    // Debug.Log($"{_state.name} of {_state.GetType()} has attackInfo");
                    attackInfo.idSO = idSO;
                    idSO++;
                    _attackInfo.Add(attackInfo);
                }
            }
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
                    states[i].RemoveFilter(f, _characterPath);
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
#endif
         
        //TODO: limit filter selection for states to its character
        //TODO: when deleting a filter, go through folder and move assets to correct folder
    }
}