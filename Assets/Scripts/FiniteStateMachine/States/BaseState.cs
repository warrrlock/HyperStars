using UnityEngine;

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
        public virtual void HandleExit(BaseStateMachine machine){}
    }
}
