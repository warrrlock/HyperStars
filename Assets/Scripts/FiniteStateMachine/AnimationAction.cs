using System;
using UnityEngine;

namespace FiniteStateMachine
{
    [CreateAssetMenu(menuName = "StateMachine/Actions/Animation Action")]
    public class AnimationAction: StateAction
    {
        [SerializeField] private string _animationName;

        [HideInInspector]
        [SerializeField] private int _animationHash;

        private void OnValidate()
        {
            _animationHash = Animator.StringToHash(_animationName);
        }

        public override void Execute(BaseStateMachine stateMachine)
        {
            stateMachine.PlayAnimation(_animationName);
        }

        public override void Stop(BaseStateMachine stateMachine)
        {
            stateMachine.HandleAnimationExit();
            // if (stateMachine.AnimatorComponent.GetCurrentAnimatorStateInfo(0).loop)
            // {
            //     stateMachine.HandleAnimationExit();
            // }
        }
    }
}