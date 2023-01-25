using Managers;
using UnityEngine;

namespace FiniteStateMachine
{
    public abstract class StateAction: ScriptableObject
    {
        public CharacterManager.CharacterSelection character = CharacterManager.CharacterSelection.None;
        public abstract void Execute(BaseStateMachine stateMachine);
        public abstract void Stop(BaseStateMachine stateMachine);
    }
}