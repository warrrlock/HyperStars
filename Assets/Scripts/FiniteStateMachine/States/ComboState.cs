using System;
using UnityEngine;
using System.Collections.Generic;

namespace FiniteStateMachine
{
    [CreateAssetMenu(menuName = "StateMachine/States/Combo State")]
    public class ComboState: BaseState
    {
        [SerializeField] private string _animationName;
        //[SerializeField] private string _animationParameter;
        [SerializeField] private List<Transition> _transitions = new List<Transition>();
        [Tooltip("If you would like the animation to start with the combo option as true. Otherwise, set to false.")]
        [SerializeField] private bool _defaultCombo = true;
        
        [SerializeField] private AttackInfo _attackInfo;
        
        private bool _combo;
        private bool _executed;
        
        // ==========  methods ========== //
        public void OnEnable()
        {
            // Debug.Log(this.name + " enabled");
            ResetVariables();
        }
        public override void Execute(BaseStateMachine stateMachine, string inputName)
        {
            if (!_executed)
            {
                Debug.Log(this.name);
                _executed = true;
                _combo = _defaultCombo;
                //stateMachine.AnimatorComponent.SetTrigger(animationParameter);
                stateMachine.AnimatorComponent.Play(_animationName);
            }
            
            foreach (Transition transition in _transitions)
            {
                transition.Execute(stateMachine, inputName, _combo);
            }
        }

        public override AttackInfo GetAttackInfo()
        {
            return _attackInfo;
        }

        public override void DisableCombo()
        {
            _combo = false;
        }
        
        public override void EnableCombo()
        {
            _combo = true;
        }
        
        public override void ResetVariables()
        {
            _executed = false;
            _combo = _defaultCombo;
        }
        
        public override void HandleExit()
        {
            ResetVariables();
        }
    }
}