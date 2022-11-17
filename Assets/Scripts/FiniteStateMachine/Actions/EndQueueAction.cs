using System;
using UnityEngine;

namespace FiniteStateMachine
{
    [CreateAssetMenu(menuName = "StateMachine/Actions/Reset Queue State")]
    public class EndQueueAction: StateAction
    {
        [SerializeField] private BaseState _toQueueState;
        public override void Execute(BaseStateMachine stateMachine)
        {
            //do nothing
        }

        public override void Stop(BaseStateMachine stateMachine)
        {
            stateMachine.SetReturnState();
            stateMachine.QueueState(_toQueueState);
        }
    }
}