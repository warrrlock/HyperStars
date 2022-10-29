using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FiniteStateMachine
{
    [CreateAssetMenu(menuName = "StateMachine/Hurt State")]
    public class HurtState : BaseState
    {
        [SerializeField] private KeyHurtStatePair.HurtStateName _hurtType;
        [SerializeField] private string _animationName;
        [HideInInspector] [SerializeField] private int _animationHash;
        [HideInInspector] [SerializeField] private string _animationName2;
        [HideInInspector] [SerializeField] private int _animationHash2;
        // ==========  methods ========== //
        private void OnValidate()
        {
            _animationHash = Animator.StringToHash(_animationName);
            if (_hurtType == KeyHurtStatePair.HurtStateName.KnockBack)
                if (_animationName2 != "")
                    _animationHash2 = Animator.StringToHash(_animationName2);
                else
                    _animationHash2 = -1;
        }

        public override void Execute(BaseStateMachine stateMachine, string inputName)
        {
            //always play animation again?
            stateMachine.PlayAnimation(_animationHash, replay: true);
            if (_hurtType == KeyHurtStatePair.HurtStateName.KnockBack)
                stateMachine.StartInAir(() => stateMachine.WaitToMove(_animationHash2));
            else stateMachine.WaitToMove();
        }
        
        #region Editor
#if UNITY_EDITOR
        [CustomEditor(typeof(HurtState))]
        class HurtStateEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                HurtState hurtState = (HurtState)target;
                if (hurtState._hurtType == KeyHurtStatePair.HurtStateName.KnockBack)
                {
                    EditorGUILayout.Space();
                    hurtState._animationName2 = EditorGUILayout.TextField("Ending Animation Name", hurtState._animationName2);
                }
            }
        }
#endif
        #endregion
    }
}
