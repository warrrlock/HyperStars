using System;
using UnityEngine;

namespace FiniteStateMachine
{
    [CreateAssetMenu(menuName = "StateMachine/Transition")]
    public class Transition: ScriptableObject
    {
        [SerializeField] private String _inputActionName;
        //[SerializeField] private Decision decision;
        [Tooltip("If the State allows for combos, the true State is the combo State, " +
                 "and the false State is the single State. Otherwise, if there are no combos, the true State is if the input is accepted, " +
                 "and false State otherwise.")]
        [SerializeField] private BaseState _trueState;
        [Tooltip("If the State allows for combos, the true State is the combo State, " +
                 "and the false State is the single State. Otherwise, if there are no combos, the true State is if the input is accepted, " +
                 "and false State otherwise.")]
        [SerializeField] private BaseState _falseState;
        // [SerializeField] private BaseState falseState;

        public void Execute (BaseStateMachine stateMachine, string inputName, bool canCombo, Action action = null)
        {
            if (action != null) action();
            bool decision = _inputActionName.Equals(inputName, StringComparison.OrdinalIgnoreCase);
            if (decision)
            {
                if (canCombo && !(_trueState is RemainInState))
                {
                    stateMachine.QueueState(_trueState);
                }
                else if (!canCombo && !(_falseState is RemainInState))
                {
                    stateMachine.QueueState(_falseState);
                }
            }
        }
        public void Execute (BaseStateMachine stateMachine, string inputName, Action action = null)
        {
            if (action != null) action();
            bool decision = _inputActionName.Equals(inputName, StringComparison.OrdinalIgnoreCase);
            // Debug.Log("checking " + inputName +" equals " + inputActionName);
            if (decision)
            {
                if (!(_trueState is RemainInState))
                {
                    stateMachine.QueueState(_trueState);
                    stateMachine.ExecuteQueuedState();
                }
            }
            else if (!(_falseState is RemainInState))
            {
                stateMachine.QueueState(_falseState);
                stateMachine.ExecuteQueuedState();
            }
        }
    }
}