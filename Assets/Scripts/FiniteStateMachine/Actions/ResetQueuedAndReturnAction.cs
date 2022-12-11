using System;
using UnityEngine;

namespace FiniteStateMachine
{
    [CreateAssetMenu(menuName = "StateMachine/Actions/Reset Queued and Return States")]
    public class ResetQueuedAndReturnAction: StateAction
    {
        public override void Execute(BaseStateMachine stateMachine)
        {
            //do nothing
        }

        public override void Stop(BaseStateMachine stateMachine)
        {
            stateMachine.SetReturnState();
            stateMachine.QueueState();
        }
    }
}