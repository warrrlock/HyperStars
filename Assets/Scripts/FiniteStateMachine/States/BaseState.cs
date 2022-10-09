using UnityEngine;

namespace FiniteStateMachine
{
    public class BaseState : ScriptableObject
    {
        public virtual void Execute(BaseStateMachine machine, string inputName) {}
        public virtual void Stop(BaseStateMachine machine, string inputName) {}
        public virtual AttackInfo GetAttackInfo()
        {
            return null;
        }
        
        public virtual void DisableCombo(){}
        public virtual void EnableCombo(){}
        public virtual void HandleExit(BaseStateMachine machine){}
        // public virtual void ResetVariables(){}
        // public virtual void Stop(BaseStateMachine machine){}
    }
}
