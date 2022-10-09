using UnityEngine;

namespace FiniteStateMachine
{
    [CreateAssetMenu(menuName = "StateMachine/Actions/Animation Action")]
    public class AnimationAction: StateAction
    {
        [SerializeField] private string _animationName;
        // [SerializeField] private string _animationParameter;
        public override void Execute(BaseStateMachine stateMachine)
        {
            stateMachine.AnimatorComponent.Play(_animationName, -1, 0.0f);
        }

        public override void Stop(BaseStateMachine stateMachine)
        {
            if (stateMachine.AnimatorComponent.GetCurrentAnimatorStateInfo(0).loop)
            {
                stateMachine.HandleAnimationExit();
            }
        }
    }
}