using System;
using UnityEngine;

namespace FiniteStateMachine
{
    [CreateAssetMenu(menuName = "StateMachine/Actions/Animation Action")]
    public class AnimationAction: StateAction
    {
        [SerializeField] private bool _endOnFinish;
        [SerializeField] private string _animationName;

        [HideInInspector]
        [SerializeField] private int _animationHash;

        private void OnValidate()
        {
            _animationHash = Animator.StringToHash(_animationName);
        }

        public override void Execute(BaseStateMachine stateMachine)
        {
            stateMachine.PlayAnimation(_animationHash);
        }

        public override void Stop(BaseStateMachine stateMachine)
        {
            if (!_endOnFinish) stateMachine.HandleAnimationExit();
        }

        public override void Finish(BaseStateMachine stateMachine)
        {
            if (_endOnFinish) stateMachine.HandleAnimationExit();
        }
    }
}