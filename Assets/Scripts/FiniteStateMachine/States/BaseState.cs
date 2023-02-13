using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class NodeInfo
{
    public Rect rect = Rect.zero;
    
}

namespace FiniteStateMachine
{
    public abstract class BaseState : ScriptableObject
    {
        [SerializeField] private bool _isCrouchState;
        public bool IsCrouchState => _isCrouchState;
        
        public abstract bool Execute(BaseStateMachine machine, string inputName);
        public virtual void QueueExecute(BaseStateMachine machine, string inputName){}
        public virtual void Stop(BaseStateMachine machine, string inputName) {}
        public virtual AttackInfo GetAttackInfo()
        {
            return null;
        }
        
        public virtual void SpawnProjectile(BaseStateMachine machine, Bounds bounds){}
        public virtual int GetSpecialBarCost()
        {
            return -1;
        }

#if UNITY_EDITOR
        //editor stuff
        [HideInInspector][SerializeField] public NodeInfo NodeInfo = new NodeInfo();
        [HideInInspector][SerializeField] private List<FSMFilter> _filters = new();
        public IReadOnlyList<FSMFilter> Filters => _filters;
        [FormerlySerializedAs("character")] public CharacterManager.CharacterSelection characterSelection = CharacterManager.CharacterSelection.None;
        public abstract IReadOnlyList<Transition> GetTransitions();
        public abstract void AddTransition(Transition t);
        public abstract void DeleteTransition(Transition t);
        public abstract bool HasTransitions();
        
        private bool _showFilters;
        public bool ShowFilters => _showFilters;
        
        public void ReloadFilters(Character character)
        {
            for (int i = _filters.Count-1; i >= 0; i--)
            {
                FSMFilter filter = character.Filters.Find(f => f.filterName.Equals(_filters[i].filterName));
                if (filter != null) _filters[i] = filter;
                else _filters.RemoveAt(i);
            }
        }
        
        public void AddFilter(FSMFilter f, string path)
        {
            _filters.Add(f);
            _filters.Sort(); //TODO: optimization - add sorted
            MoveToCorrectFolder(f, path);
            SaveChanges();
        }

        public bool RemoveFilter(FSMFilter f, string path)
        {
            bool r = _filters.Remove(f);
            if (r)
            {
                MoveToCorrectFolder(f, path);
                SaveChanges();
            }
            return r;
        }
        
        private void MoveToCorrectFolder(FSMFilter f, string characterPath)
        {
            string ending = $"{name}.asset";
            string moved = "";
            if (Filters.Count == 0)
                moved = AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(this), $"{characterPath}/unfiltered/{ending}");
            else if (Filters.Count == 1)
                moved = AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(this), $"{characterPath}/{Filters[0].filterName}/{ending}");
            else
                moved = AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(this), $"{characterPath}/multi-filtered/{ending}");
            AssetDatabase.Refresh();
        
            Debug.Log($"adding/removing {f.filterName}, moving {name} with {Filters.Count} filters." +
                      $"\nMessage: {moved}");
        }

        public void ToggleShowFilter()
        {
            _showFilters = !_showFilters;
            SaveChanges();
        }
        
        //states sometimes don't save transitions if closing application soon after making adjustments
        protected void SaveChanges()
        { 
            EditorUtility.SetDirty(this);
        }
#endif
    }
}
