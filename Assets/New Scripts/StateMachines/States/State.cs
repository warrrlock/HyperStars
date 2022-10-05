using UnityEngine;
using System.Collections.Generic;
using FiniteStateMachine;

namespace FiniteStateMachine
{
    [CreateAssetMenu(menuName = "StateMachine/States/State")]
    public class State: BaseState
    {
        // ========== variables ========== //
        [SerializeField] private List<StateAction> actions = new List<StateAction>();
        [SerializeField] private List<Transition> transitions = new List<Transition>();
        
        
        // ==========  methods ========== //
        public override void Execute(BaseStateMachine stateMachine, string inputName){
            foreach(StateAction action in actions){
                action.Execute(stateMachine);
            }
            foreach (Transition transition in transitions)
            {
                transition.Execute(stateMachine, inputName);
            }
        }
        
    }
    
}