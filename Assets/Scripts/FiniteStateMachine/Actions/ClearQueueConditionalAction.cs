using System.Linq;
using UnityEngine;

namespace FiniteStateMachine
{
    [CreateAssetMenu(menuName = "StateMachine/Actions/Clear Queue Conditional")]
    public class ClearQueueConditionalAction: StateAction
    {
        [SerializeField] private BaseState[] _conditionalQueuedStates;

        public override void Execute(BaseStateMachine stateMachine)
        {
            if (_conditionalQueuedStates.Any(state => state == stateMachine.QueuedState))
                stateMachine.ClearQueues();
        }

        public override void Stop(BaseStateMachine stateMachine)
        {
            // do nothing
        }
    }
}