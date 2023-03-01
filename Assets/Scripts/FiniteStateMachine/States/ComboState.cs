using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FiniteStateMachine
{
    [CreateAssetMenu(menuName = "StateMachine/States/Combo State")]
    public class ComboState: BaseState
    {
        [FsmList(typeof(StateAction))] [FormerlySerializedAs("_inputStopActions")] [SerializeField] private List<StateAction> _onInputPlayOrStopActions = new List<StateAction>();
        [FsmList(typeof(StateAction))] [SerializeField] private List<StateAction> _onInputInvokeActions = new List<StateAction>();
        [FsmList(typeof(Transition))] [SerializeField] private List<Transition> _transitions = new List<Transition>();
        [Tooltip("If you would like the animation to start with the combo option as true. Otherwise, set to false.")]
        [SerializeField] private bool _defaultCombo = true;
        [SerializeField] private bool _alwaysHitConfirm;
        
        [Header("Attack Information")]
        [SerializeField] private AttackInfo _attackInfo;

        [Header("Projectile")] [SerializeField]
        private GameObject _projectilePrefab;
        

        // ==========  methods ========== //
        private void OnEnable()
        {
            _transitions.RemoveAll(t => !t);
            _onInputPlayOrStopActions.RemoveAll(a => !a);
            _onInputInvokeActions.RemoveAll(a => !a);
        }
        

        public override bool Execute(BaseStateMachine stateMachine, string inputName)
        {
            foreach(StateAction action in _onInputInvokeActions){
                action.Execute(stateMachine);
            }
            
            if (stateMachine.PlayAnimation(_animationHash, _defaultCombo))
            {
                stateMachine.EnableAttackStop();
                foreach(StateAction action in _onInputPlayOrStopActions){
                    action.Execute(stateMachine);
                }
                CheckSpecialMeter(stateMachine);
            }

            foreach (Transition transition in _transitions)
            {
                // Debug.Log($"{stateMachine.name} executing transition {transition.name}");
                if (transition.Execute(stateMachine, inputName, stateMachine.CanCombo(_alwaysHitConfirm))) return true;
            }

            return false;
        }

        public override AttackInfo GetAttackInfo()
        {
            return _attackInfo;
        }
        
        public override void Stop(BaseStateMachine stateMachine, string inputName)
        {
            // Debug.Log($"{stateMachine.name} stopped action {name}");
            foreach(StateAction action in _onInputPlayOrStopActions){
                action.Stop(stateMachine);
            }
            foreach(StateAction action in _onInputInvokeActions){
                action.Stop(stateMachine);
            }
        }

        public override void SpawnProjectile(BaseStateMachine stateMachine, Bounds bounds)
        {
            Projectile projectile = Instantiate(_projectilePrefab, stateMachine.Fighter.transform.position, Quaternion.identity).GetComponent<Projectile>();
            projectile.Spawn(stateMachine.Fighter, bounds);
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