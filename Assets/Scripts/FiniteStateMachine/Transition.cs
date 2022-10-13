using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FiniteStateMachine
{
    #region Transition
    [CreateAssetMenu(menuName = "StateMachine/Transition")]
    public class Transition: ScriptableObject
    {
        enum FalseState { DoNothing, CustomFalseState }
        
        [SerializeField] private String _inputActionName;
        
        [Tooltip("If the State allows for combos, the true State is if the combo is successful, " +
                 "and the false State is if the combo fails.\n\nOtherwise, if there are no combos, the true State is if the input is accepted, " +
                 "and false State otherwise.")]
        [SerializeField] private BaseState _trueState;
        [Tooltip("If the State allows for combos, the true State is if the combo is successful, " +
                 "and the false State is if the combo fails.\n\nOtherwise, if there are no combos, the true State is if the input is accepted, " +
                 "and false State otherwise.")]
        [SerializeField] private FalseState _falseState;
        private BaseState _customFalseState;

        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// Queues the next state (true state if successful combo and false otherwise),
        /// which will be executed upon end of animation, or as indicated in animation.
        /// </summary>
        /// <param name="stateMachine">state machine</param>
        /// <param name="inputName">name of input action from player</param>
        /// <param name="canCombo">whether or not the combo is successful.</param>
        /// <param name="action">action to perform at start</param>
        public void Execute (BaseStateMachine stateMachine, string inputName, bool canCombo, Action action = null)
        {
            if (action != null) action();
            bool decision = _inputActionName.Equals(inputName, StringComparison.OrdinalIgnoreCase);
            if (decision)
            {
                if (canCombo)
                {
                    if (!_trueState)
                    {
                        Debug.LogWarning("no true state was assigned to this transition", this);
                        return;
                    }
                    stateMachine.QueueState(_trueState);
                }
                else
                {
                    if ((_customFalseState))
                        stateMachine.QueueState(_customFalseState);
                }
            }
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// Executes the true state.
        /// </summary>
        /// <param name="stateMachine">the state machine.</param>
        /// <param name="inputName">name of input action from player.</param>
        /// <param name="action">action to perform at start, default none.</param>
        public void Execute (BaseStateMachine stateMachine, string inputName, Action action = null)
        {
            if (action != null) action();
            bool decision = _inputActionName.Equals(inputName, StringComparison.OrdinalIgnoreCase);
            // Debug.Log("checking " + inputName +" equals " + _inputActionName);
            if (decision)
            {
                if (!_trueState)
                {
                    Debug.LogWarning("no true state was assigned to this transition", this);
                    return;
                }
                stateMachine.QueueState(_trueState);
                stateMachine.ExecuteQueuedState();
            }
            else if ((_customFalseState) || _customFalseState is RemainInState)
            {
                stateMachine.QueueState(_customFalseState);
                stateMachine.ExecuteQueuedState();
            }
        }
        
        #region Editor
#if UNITY_EDITOR
        [CustomEditor(typeof(Transition))]
        class TransitionEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                Transition transition = (Transition)target;
                if (transition._falseState == FalseState.CustomFalseState)
                {
                    EditorGUILayout.Space();EditorGUILayout.Space();
                    transition._customFalseState = EditorGUILayout.ObjectField("Custom False State", transition._customFalseState, 
                        typeof(BaseState), false) as BaseState;
                }
                
            }
        }
#endif
        #endregion
    }
    #endregion
}