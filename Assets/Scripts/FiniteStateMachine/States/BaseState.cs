using System;
using System.Collections.Generic;
using Managers;
using UnityEngine;
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
        public abstract void Execute(BaseStateMachine machine, string inputName);
        public virtual void Stop(BaseStateMachine machine, string inputName) {}
        public virtual AttackInfo GetAttackInfo()
        {
            return null;
        }
        
        public virtual void DisableCombo(){}
        public virtual void EnableCombo(){}
        public virtual void SpawnProjectile(BaseStateMachine machine, Bounds bounds){}
        public virtual void HandleExit(BaseStateMachine machine){}
        
        public virtual int GetSpecialBarCost()
        {
            return -1;
        }

        
        //editor stuff
        [HideInInspector][SerializeField] public NodeInfo NodeInfo = new NodeInfo();
        [HideInInspector][SerializeField] private List<FSMFilter> _filters = new();
        public IReadOnlyList<FSMFilter> Filters => _filters;
        public CharacterManager.CharacterSelection character = CharacterManager.CharacterSelection.None;
        public abstract IReadOnlyList<Transition> GetTransitions();
        private bool _showFilters;
        public bool ShowFilters => _showFilters;
        
        public void AddFilter(FSMFilter f)
        {
            _filters.Add(f);
            SaveChanges();
        }

        public bool RemoveFilter(FSMFilter f)
        {
            bool r = _filters.Remove(f);
            SaveChanges();
            return r;
        }

        public void ToggleShowFilter()
        {
            _showFilters = !_showFilters;
            SaveChanges();
        }
        
        private void SaveChanges()
        { 
            EditorUtility.SetDirty(this);
        }
    }
}
