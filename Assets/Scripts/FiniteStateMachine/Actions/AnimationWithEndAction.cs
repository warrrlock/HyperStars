using UnityEngine;

namespace FiniteStateMachine
{
    [CreateAssetMenu(menuName = "StateMachine/Actions/Animation with End Action")]
    public class AnimationWithEndAction: StateAction
    {
        [SerializeField] private string _animationName;
        [SerializeField] private string _endAnimationName;

        [HideInInspector] [SerializeField] private int _animationHash;
        [HideInInspector] [SerializeField] private int _endAnimationHash;
        private void OnValidate()
        {
            _animationHash = _animationName == "" ? -1 : Animator.StringToHash(_animationName);
            _endAnimationHash = _endAnimationName == "" ? -1 : Animator.StringToHash(_endAnimationName);
        }

        public override void Execute(BaseStateMachine stateMachine)
        {
            if (_animationHash != -1) stateMachine.PlayAnimation(_animationHash);
        }

        public override void Stop(BaseStateMachine stateMachine)
        {
            // Debug.Log("should play end anim");
            if (_animationHash != -1) stateMachine.PlayAnimation(_endAnimationHash);
        }
    }
}