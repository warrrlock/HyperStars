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
        [SerializeField] private List<Transition> _transitions = new List<Transition>();
        [Tooltip("If you would like the animation to start with the combo option as true. Otherwise, set to false.")]
        [SerializeField] private bool _defaultCombo = true;
        
        [Header("Attack Information")]
        [SerializeField] private AttackInfo _attackInfo;
        
        [Header("Special")]
        public bool isSpecial;
        [Tooltip("number of bars the special costs. 1 means 1 bar.")]
        public int specialBarCost;
        
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
        }
        
        public override void Execute(BaseStateMachine stateMachine, string inputName)
        {
            stateMachine.Fighter.OpposingFighter.ResetFighterHurtboxes();

            if (stateMachine.PlayAnimation(_animationHash, _defaultCombo))
            {
                stateMachine.EnableAttackStop();
                if (isSpecial) stateMachine.Fighter.SpecialMeterManager?.DecrementBar(specialBarCost);
            }

            foreach (Transition transition in _transitions)
                transition.Execute(stateMachine, inputName, stateMachine.CanCombo);
        }

        public override AttackInfo GetAttackInfo()
        {
            return _attackInfo;
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