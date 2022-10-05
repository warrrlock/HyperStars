using UnityEngine;

namespace FiniteStateMachine
{
    public abstract class StateAction: ScriptableObject
    {
        public abstract void Execute(BaseStateMachine stateMachine);
    }
}