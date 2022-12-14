using System;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FiniteStateMachine
{
    [CreateAssetMenu(menuName = "StateMachine/States/Combo State")]
    public class ComboState: BaseState
    {
        [SerializeField] private string _animationName;
        [SerializeField] private List<StateAction> _inputStopActions = new List<StateAction>();
        [SerializeField] private List<Transition> _transitions = new List<Transition>();
        [Tooltip("If you would like the animation to start with the combo option as true. Otherwise, set to false.")]
        [SerializeField] private bool _defaultCombo = true;
        
        [Header("Attack Information")]
        [SerializeField] private AttackInfo _attackInfo;
        
        [Header("Special")]
        [SerializeField] private bool _isSpecial;
        [Tooltip("number of bars the special costs. 1 means 1 bar.")]
        [SerializeField] private int _specialBarCost;
        
        [HideInInspector]
        [SerializeField] private int _animationHash;

        // ==========  methods ========== //
        private void OnValidate()
        {
            _animationHash = Animator.StringToHash(_animationName);
        }
        
        private void OnEnable()
        {
            _transitions.RemoveAll(t => !t);
            _inputStopActions.RemoveAll(a => !a);
        }
        
        public override void Execute(BaseStateMachine stateMachine, string inputName)
        {
            stateMachine.Fighter.OpposingFighter.ResetFighterHurtboxes();

            if (stateMachine.PlayAnimation(_animationHash, _defaultCombo))
            {
                stateMachine.EnableAttackStop();
                foreach(StateAction action in _inputStopActions){
                    action.Execute(stateMachine);
                }
                if (_isSpecial) stateMachine.Fighter.SpecialMeterManager?.DecrementBar(_specialBarCost);
            }

            foreach (Transition transition in _transitions)
            {
                // Debug.Log($"{stateMachine.name} executing transition {transition.name}");
                transition.Execute(stateMachine, inputName, stateMachine.CanCombo);
            }
        }

        public override AttackInfo GetAttackInfo()
        {
            return _attackInfo;
        }
        
        public override void Stop(BaseStateMachine stateMachine, string inputName)
        {
            // Debug.Log($"{stateMachine.name} stopped action {name}");
            foreach(StateAction action in _inputStopActions){
                action.Stop(stateMachine);
            }
        }

        public override int GetSpecialBarCost()
        {
            return _isSpecial ? _specialBarCost : -1;
        }
        
//         #region Editor
// #if UNITY_EDITOR
//         [CustomEditor(typeof(ComboState))]
//         class ComboStateEditor : Editor
//         {
//             public override void OnInspectorGUI()
//             {
//                 base.OnInspectorGUI();
//                 ComboState state = (ComboState)target;
//                 if (state.isSpecial)
//                 {
//                     state.specialBarCost = EditorGUILayout.IntField("Number of Bars (COST)", state.specialBarCost);
//                 }
//             }
//         }
// #endif
//         #endregion
    }
}