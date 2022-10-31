using System;
using UnityEngine;
using System.Collections.Generic;
using FiniteStateMachine;

namespace FiniteStateMachine
{
    [CreateAssetMenu(menuName = "StateMachine/States/State")]
    public class State: BaseState
    {
        // ========== variables ========== //
        [SerializeField] private List<StateAction> _actions = new List<StateAction>();
        [SerializeField] private List<Transition> _transitions = new List<Transition>();
        
        // ==========  methods ========== //
        public void OnEnable()
        {
            _actions.RemoveAll(a => !a);
            _transitions.RemoveAll(t => !t);
        }
        
        public override void Execute(BaseStateMachine stateMachine, string inputName){
            foreach(StateAction action in _actions){
                action.Execute(stateMachine);
            }
            foreach (Transition transition in _transitions)
            {
                transition.Execute(stateMachine, inputName);
            }
        }
        public override void Stop(BaseStateMachine stateMachine, string inputName)
        {
            stateMachine.SetReturnState();
            foreach(StateAction action in _actions){
                action.Stop(stateMachine);
            }
        }
    }
    
}