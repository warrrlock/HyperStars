using System;
using UnityEngine;

namespace FiniteStateMachine
{
    [CreateAssetMenu(menuName = "StateMachine/Transition")]
    public class Transition: ScriptableObject
    {
        [SerializeField] private String inputActionName;
        //[SerializeField] private Decision decision;
        [Tooltip("If the State allows for combos, the true State is the combo State, " +
                 "and the false State is the single State. Otherwise, if there are no combos, the true State is if the input is accepted, " +
                 "and false State otherwise.")]
        [SerializeField] private BaseState trueState;
        [Tooltip("If the State allows for combos, the true State is the combo State, " +
                 "and the false State is the single State. Otherwise, if there are no combos, the true State is if the input is accepted, " +
                 "and false State otherwise.")]
        [SerializeField] private BaseState falseState;
        // [SerializeField] private BaseState falseState;

        public void Execute (BaseStateMachine stateMachine, string inputName, bool canCombo, Action action = null)
        {
            if (action != null) action();
            bool decision = inputActionName.Equals(inputName, StringComparison.OrdinalIgnoreCase);
            if (decision)
            {
                if (canCombo && !(trueState is RemainInState))
                {
                    stateMachine.QueueState(trueState);
                }
                else if (!canCombo && !(falseState is RemainInState))
                {
                    stateMachine.QueueState(falseState);
                }
            }
        }
        public void Execute (BaseStateMachine stateMachine, string inputName, Action action = null)
        {
            if (action != null) action();
            bool decision = inputActionName.Equals(inputName, StringComparison.OrdinalIgnoreCase);
            Debug.Log("checking " + inputName +" equals " + inputActionName);
            if (decision)
            {
                if (!(trueState is RemainInState))
                {
                    stateMachine.QueueState(trueState);
                    stateMachine.ExecuteQueuedState();
                }
            }
            else if (!(falseState is RemainInState))
            {
                stateMachine.QueueState(falseState);
                stateMachine.ExecuteQueuedState();
            }
        }
    }
}