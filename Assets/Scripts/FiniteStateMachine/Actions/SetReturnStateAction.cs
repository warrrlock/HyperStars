using UnityEngine;

namespace FiniteStateMachine
{
    [CreateAssetMenu(menuName = "StateMachine/Actions/SetReturnAction")]
    public class SetReturnStateAction: StateAction
    {
        [SerializeField] private BaseState _returnState;
        
        public override void Execute(BaseStateMachine stateMachine)
        {
            if(_returnState) stateMachine.SetReturnState(_returnState);
        }

        public override void Stop(BaseStateMachine stateMachine)
        {
            stateMachine.SetReturnState();
        }
    }
}