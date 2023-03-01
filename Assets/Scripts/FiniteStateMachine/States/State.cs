using System;
using UnityEngine;
using System.Collections.Generic;

namespace FiniteStateMachine
{
    [CreateAssetMenu(menuName = "StateMachine/States/State")]
    public class State: BaseState
    {
        // ========== variables ========== //
        [SerializeField] private bool _endOnFinish;
        [FsmList(typeof(StateAction))] [SerializeField] private List<StateAction> _actions = new List<StateAction>();
        [FsmList(typeof(Transition))] [SerializeField] private List<Transition> _transitions = new List<Transition>();
        
        // ==========  methods ========== //
        public void OnEnable()
        {
            _actions.RemoveAll(a => !a);
            _transitions.RemoveAll(t => !t);
        }

        public override bool Execute(BaseStateMachine stateMachine, string inputName){
            // Debug.Log($"{stateMachine.name} is executing {name}");
            if (stateMachine.PlayAnimation(_animationHash))
            {
                CheckSpecialMeter(stateMachine);
            }
            
            foreach(StateAction action in _actions){
                action.Execute(stateMachine);
            }
            foreach (Transition transition in _transitions)
            {
                if (transition.Execute(stateMachine, inputName)) return true;
            }
            return false;
        }
        
        public override void QueueExecute(BaseStateMachine stateMachine, string inputName){
            // Debug.Log($"failed to transition, checking transitions for {name}");
            foreach (Transition transition in _transitions)
            {
                transition.Execute(stateMachine, inputName, action: null, queueAtEndOfAnim: true);
            }
        }
        
        public override void Stop(BaseStateMachine stateMachine, string inputName)
        {
            // Debug.Log($"{stateMachine.name} stopped action {name}");
            if (!_endOnFinish) stateMachine.HandleAnimationExit();
            foreach(StateAction action in _actions){
                action.Stop(stateMachine);
            }
        }
        
        public override void Finish(BaseStateMachine stateMachine)
        {
            if (_endOnFinish) stateMachine.HandleAnimationExit();
        }
        
#if UNITY_EDITOR
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
#endif
    }
}