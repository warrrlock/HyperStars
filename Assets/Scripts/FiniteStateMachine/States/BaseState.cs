using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NodeInfo
{
    public Rect rect = Rect.zero;
    
}

namespace FiniteStateMachine
{
    public abstract class BaseState : ScriptableObject
    {
        [HideInInspector][SerializeField] public NodeInfo NodeInfo = new NodeInfo();
        public abstract IReadOnlyList<Transition> GetTransitions();
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
    }
}
