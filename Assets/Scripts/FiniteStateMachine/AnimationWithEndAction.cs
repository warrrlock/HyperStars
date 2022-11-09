using System;
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
            _animationHash = Animator.StringToHash(_animationName);
            _endAnimationHash = Animator.StringToHash(_endAnimationName);
        }

        public override void Execute(BaseStateMachine stateMachine)
        {
            stateMachine.PlayAnimation(_animationHash);
        }

        public override void Stop(BaseStateMachine stateMachine)
        {
            Debug.Log("should play end anim");
            stateMachine.PlayAnimation(_endAnimationHash);
            // if (stateMachine.AnimatorComponent.GetCurrentAnimatorStateInfo(0).loop)
            // {
            //     stateMachine.HandleAnimationExit();
            // }
        }
    }
}