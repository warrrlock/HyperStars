using System;
using System.Collections.Generic;
using UnityEngine;

namespace FiniteStateMachine
{
    [CreateAssetMenu(menuName = "StateMachine/Actions/Disable Input Until Idle Action")]
    public class DisableInputUntilIdleAction: StateAction
    {
        [SerializeField] private List<string> _inputsToDisable;
        [SerializeField] private BaseState _returnState;
        public override void Execute(BaseStateMachine stateMachine)
        {
            if (!stateMachine.CheckReturnState(_returnState)) 
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