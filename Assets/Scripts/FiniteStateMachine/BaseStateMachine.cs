using System;
using System.Collections.Generic;
using System.Linq;
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
    public float hitStopDuration;
}

namespace FiniteStateMachine {
    [RequireComponent(typeof(Animator))] 
    [RequireComponent(typeof(Fighter))] 
    public class BaseStateMachine : MonoBehaviour 
    {
        
        public BaseState CurrentState {get; private set;}
        public bool CanCombo { get; private set; }
        public Animator AnimatorComponent { get; private set; }
        public AttackInfo AttackInfo => CurrentState.GetAttackInfo();
        
        [SerializeField] private BaseState _initialState;
        [Tooltip("Clips that should not have a end event automatically added. " +
                 "The end event resets variables of the current state, then returns the player to the initial state." +
                 "\n\n****Add all looping animations.")]
        [SerializeField] private List<AnimationClip> _noEndEventClips;

        private BaseState _queuedState;
        private bool _rejectInput;
        private int _currentAnimation = -1;
        private bool _isAttacking;

        private Fighter _fighter;
        private Dictionary<Type, Component> _cachedComponents;

        
        private void Awake()
        {
            CurrentState = _initialState;
            _cachedComponents = new Dictionary<Type, Component>();
            _fighter = GetComponent<Fighter>();
            AnimatorComponent = GetComponent<Animator>();
            
            
            foreach (AnimationClip clip in AnimatorComponent.runtimeAnimatorController.animationClips)
            {
                if (_noEndEventClips.Contains(clip) ||
                    (clip.events.Count(c => c.functionName == "HandleAnimationExit") > 0)) continue;

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
            Debug.Log(this.name + " invoked " + action.inputAction.name + " with current State: " + CurrentState.name);
            CurrentState.Execute(this, action.inputAction.name);
        }

        private void Stop(InputManager.Action action)
        {
            CurrentState.Stop(this, action.inputAction.name);
        }

        public bool PlayAnimation(int animationState, bool defaultCombo = false)
        {
            if (_currentAnimation == animationState) return false;
            //Debug.Log(this.name);
            _currentAnimation = animationState;
            CanCombo = defaultCombo;
            AnimatorComponent.Play(animationState, -1, 0);
            
            return true;
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
            _rejectInput = true;
            
            HandleStateExit();
            CurrentState = _queuedState;
            _queuedState = null;
           
            _rejectInput = false;

            CurrentState.Execute(this, "");
        }
        
        /// <summary>
        /// Invoked at the end of an animation. Automatically added to each animation,
        /// and either executes a queued State, or sets the State back to the initial State.
        /// </summary>
        public void HandleAnimationExit()
        {
            // Debug.Log(this.name + " exiting anim " + animationHash);
            TrySetQueueInitial();
            ExecuteQueuedState();
            if (_isAttacking)
            {
                DisableAttackStop();
            }
        }
        
        //ANIMATION USE
        public void DisableCombo()
        {
            CanCombo = false;
        }
        
        public void EnableCombo()
        {
            CanCombo = true;
        }
        
        private void TrySetQueueInitial()
        {
            if (!_queuedState && CurrentState != _initialState) _queuedState = _initialState;
        }

        private void HandleStateExit()
        {
            _currentAnimation = -1;
        }

        public void EnableAttackStop()
        {
            if (_isAttacking) return;
            _isAttacking = true;
            _fighter.MovementController.EnableAttackStop();
        }
        
        private void DisableAttackStop()
        {
            if (!_isAttacking) return;
            _isAttacking = false;
            _fighter.MovementController.DisableAttackStop();
            //need some way to return to walk state upon exit, if player is still holding onto move input
        }
    }
}
