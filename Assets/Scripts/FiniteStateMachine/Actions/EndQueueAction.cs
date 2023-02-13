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
            // Debug.LogWarning($"queueing {_toQueueState.name}");
            stateMachine.QueueState(_toQueueState);
        }
    }
}