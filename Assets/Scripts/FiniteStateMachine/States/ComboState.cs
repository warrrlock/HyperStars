using System;
using UnityEngine;
using System.Collections.Generic;

namespace FiniteStateMachine
{
    [CreateAssetMenu(menuName = "StateMachine/States/Combo State")]
    public class ComboState: BaseState
    {
        [SerializeField] private string _animationName;
        [SerializeField] private List<Transition> _transitions = new List<Transition>();
        [Tooltip("If you would like the animation to start with the combo option as true. Otherwise, set to false.")]
        [SerializeField] private bool _defaultCombo = true;
        
        [SerializeField] private AttackInfo _attackInfo;

        private int _animationHash;

        // ==========  methods ========== //
        private void OnValidate()
        {
            _animationHash = Animator.StringToHash(_animationName);
            _transitions.RemoveAll(t => !t);
        }
        
        public override void Execute(BaseStateMachine stateMachine, string inputName)
        {
            if (stateMachine.PlayAnimation(_animationHash, _defaultCombo))
                stateMachine.EnableAttackStop();

            foreach (Transition transition in _transitions)
                transition.Execute(stateMachine, inputName, stateMachine.CanCombo);
        }

        public override AttackInfo GetAttackInfo()
        {
            return _attackInfo;
        }
    }
}