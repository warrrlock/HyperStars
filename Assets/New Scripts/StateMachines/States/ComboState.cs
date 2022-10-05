using System;
using UnityEngine;
using System.Collections.Generic;

namespace FiniteStateMachine
{
    [CreateAssetMenu(menuName = "StateMachine/States/ComboState")]
    public class ComboState: BaseState
    {
        [SerializeField] private string animationName;
        [SerializeField] private string animationParameter;
        [SerializeField] private List<Transition> transitions = new List<Transition>();
        [Tooltip("If you would like the animation to start with the combo option as true. Otherwise, set to false.")]
        [SerializeField] private bool defaultCombo = true;
        private bool _combo;
        private bool _executed;
        
        // ==========  methods ========== //
        public void OnEnable()
        {
            Debug.Log(this.name + " enabled");
            ResetVariables();
        }
        public override void Execute(BaseStateMachine stateMachine, string inputName){
            if (!_executed)
            {
                Debug.Log(this.name);
                _executed = true;
                _combo = defaultCombo;
                stateMachine.AnimatorComponent.SetTrigger(animationParameter);
                //stateMachine.AnimatorComponent.SetBool(animationParameter, false);
            }
            
            foreach (Transition transition in transitions)
            {
                transition.Execute(stateMachine, inputName, _combo);
            }
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
            _combo = defaultCombo;
        }
        public override void HandleExit()
        {
            ResetVariables();
        }
    }
}