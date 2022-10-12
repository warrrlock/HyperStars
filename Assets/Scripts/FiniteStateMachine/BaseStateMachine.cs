using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class AttackInfo
{
    public float knockbackDuration;
    public float knockbackDistance;
    public float hitStunDuration;
    public float damage;
    public Vector3 knockBackAngle;
    public bool causesWallBounce;
}

namespace FiniteStateMachine {
    [RequireComponent(typeof(Animator))] 
    public class BaseStateMachine : MonoBehaviour 
    {
        [SerializeField] private BaseState _initialState; 
        public BaseState CurrentState {get; private set;}
        private BaseState _queuedState;
        private bool _rejectInput;
        
        public Animator AnimatorComponent { get; private set; }
        [Tooltip("Clips that should not have a end event automatically added. " +
                 "The end event resets variables of the current state, then returns the player to the initial state." +
                 "\n\n****Add all looping animations.")]
        [SerializeField] private List<AnimationClip> _noEndEventClips;
        
        private Dictionary<Type, Component> _cachedComponents;
        public AttackInfo AttackInformation => CurrentState.GetAttackInfo();
        
        private void Awake()
        {
            CurrentState = _initialState;
            _cachedComponents = new Dictionary<Type, Component>();
            AnimatorComponent = GetComponent<Animator>();
            
            
            foreach (AnimationClip clip in AnimatorComponent.runtimeAnimatorController.animationClips)
            {
                if (_noEndEventClips.Contains(clip)) continue;
                // AnimationEvent animationStartEvent = new AnimationEvent
                // {
                //     time = 0,
                //     functionName = "HandleAnimationEnter"
                // };
                // clip.AddEvent(animationStartEvent);

                AnimationEvent animationEndEvent = new AnimationEvent
                {
                    time = clip.length,
                    functionName = "HandleAnimationExit"
                };
                clip.AddEvent(animationEndEvent);
            }
        }

        private void Start()
        {
            Fighter fighter = GetComponent<Fighter>();
            foreach (KeyValuePair<string, InputManager.Action> entry in fighter.InputManager.Actions)
            {
                entry.Value.perform += Invoke;
                entry.Value.stop += Stop;
            }
            CurrentState.Execute(this, "");
        }
        
        //methods
        public new T GetComponent<T>() where T: Component 
        {
            if (_cachedComponents.ContainsKey(typeof(T)))
                return _cachedComponents[typeof(T)] as T;
            
            var component = base.GetComponent<T>();
            
            if (component != null)
                _cachedComponents.Add(typeof(T), component);

            return component;
        }
        
        private void Invoke(InputManager.Action action)
        {
            if (_rejectInput) return;
            Debug.Log("invoked " + action.inputAction.name + " with current State: " + CurrentState.name);
            CurrentState.Execute(this, action.inputAction.name);
        }

        private void Stop(InputManager.Action action)
        {
            CurrentState.Stop(this, action.inputAction.name);
        }
        
        public void QueueState(BaseState state)
        {
            // Debug.Log("queueing state " + state.name);
            if (_queuedState == null) _queuedState = state;
        }
        
        /// <summary>
        /// Execute the queued State.
        /// In animation, this can be invoked through an event,
        /// and occurs at the end of an animation if there exists a queued State.
        /// </summary>
        public void ExecuteQueuedState()
        {
            // Debug.Log("executing queued state");
            if (!_queuedState) return;
            //Debug.Log("going to execute "+ _queuedState.name);
            
            //race condition D:
            //reject input here
            _rejectInput = true;
            
            CurrentState.HandleExit(this);
            CurrentState = _queuedState;
            _queuedState = null;
           
            _rejectInput = false;
            //allow input here
            
            CurrentState.Execute(this, "");
        }
        
        //ANIMATION USE
        public void DisableCombo()
        {
            CurrentState.DisableCombo();
        }
        
        public void EnableCombo()
        {
            CurrentState.EnableCombo();
        }
        
        private void TrySetQueueInitial()
        {
            if (!_queuedState) _queuedState = _initialState;
        }

        private void TrySetStateInitial()
        {
            if (_queuedState != null) CurrentState = _initialState;
        }
        
        private void ForceSetStateInitial()
        {
            CurrentState = _initialState;
            _queuedState = null;
        }

        private void HandleAnimationEnter()
        {
            // Debug.Log("entering anim");
        }
        
        /// <summary>
        /// Invoked at the end of an animation. Automatically added to each animation,
        /// and either executes a queued State, or sets the State back to the initial State.
        /// </summary>
        public void HandleAnimationExit()
        {
            //Debug.Log("exiting anim");
            TrySetQueueInitial();
            ExecuteQueuedState();   
        }
    }
}
