using Managers;
using UnityEngine;

namespace FiniteStateMachine
{
    public abstract class StateAction: ScriptableObject
    {
        public abstract void Execute(BaseStateMachine stateMachine);
        public abstract void Stop(BaseStateMachine stateMachine);
        public virtual void Finish(BaseStateMachine stateMachine){}
    }
}