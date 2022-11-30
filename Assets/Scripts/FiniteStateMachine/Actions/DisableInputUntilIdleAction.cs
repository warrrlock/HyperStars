using System.Collections.Generic;
using UnityEngine;

namespace FiniteStateMachine
{
    [CreateAssetMenu(menuName = "StateMachine/Actions/Disable Input Until Idle Action")]
    public class DisableInputUntilIdleAction: StateAction
    {
        [SerializeField] private List<string> _inputsToDisable;
        [Tooltip("only execute this action if the following return state hasn't been set yet.")]

        public override void Execute(BaseStateMachine stateMachine)
        {
            stateMachine.DisableInputs(_inputsToDisable, 
                () => stateMachine.IsIdle || stateMachine.CurrentState is HurtState, 
                false);
        }

        public override void Stop(BaseStateMachine stateMachine)
        {
            //do nothing
        }
    }
}