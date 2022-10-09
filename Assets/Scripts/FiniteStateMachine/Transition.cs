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
                    stateMachine.ExecuteQueuedState();
                }
                else
                {
                    if (!(_customFalseState) || _customFalseState is RemainInState)
                    {
                        stateMachine.QueueState(_customFalseState);
                        stateMachine.ExecuteQueuedState();
                    }
                    stateMachine.QueueState(_customFalseState);
                }
            }
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
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
            else if (!(_customFalseState) || _customFalseState is RemainInState)
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
                    transition._customFalseState = EditorGUILayout.ObjectField("custom false state", transition._customFalseState, 
                        typeof(BaseState), false) as BaseState;
                }
                
            }
        }
#endif
        #endregion
    }
    #endregion
}