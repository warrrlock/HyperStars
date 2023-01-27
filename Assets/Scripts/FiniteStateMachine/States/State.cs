using System;
using UnityEngine;
using System.Collections.Generic;

namespace FiniteStateMachine
{
    [CreateAssetMenu(menuName = "StateMachine/States/State")]
    public class State: BaseState
    {
        // ========== variables ========== //
        [FsmList(typeof(StateAction))] [SerializeField] private List<StateAction> _actions = new List<StateAction>();
        [FsmList(typeof(Transition))] [SerializeField] private List<Transition> _transitions = new List<Transition>();
        
        // ==========  methods ========== //
        public void OnEnable()
        {
            _actions.RemoveAll(a => !a);
            _transitions.RemoveAll(t => !t);
        }
        
        public override void AddTransition(Transition t)
        {
            _transitions.Add(t);
            SaveChanges();
        }

        public override void DeleteTransition(Transition t)
        {
            _transitions.Remove(t);
            SaveChanges();
        }

        public override bool HasTransitions()
        {
            return true;
        }

        public override IReadOnlyList<Transition> GetTransitions()
        {
            return _transitions;
        }

        public override void Execute(BaseStateMachine stateMachine, string inputName){
            // Debug.Log($"{stateMachine.name} is executing {name}");
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
            // Debug.Log($"{stateMachine.name} stopped action {name}");
            foreach(StateAction action in _actions){
                action.Stop(stateMachine);
            }
        }
    }
}