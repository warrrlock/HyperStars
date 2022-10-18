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
    public float hitStopDuration;
    public float wallBounceDuration;
    [Tooltip("Leave this at 0 if wall bounce should not create a new force.")]
    public float wallBounceDistance;
    [Tooltip("Assume the hit fighter collides with the wall while going right.")]
    public Vector3 wallBounceDirection;
    public float wallBounceHitStopDuration;
    public float hangTime;
}

namespace FiniteStateMachine {
    [RequireComponent(typeof(Animator))] 
    public class BaseStateMachine : MonoBehaviour 
    {
        [SerializeField] private BaseState _initialState; 
        public BaseState CurrentState {get; private set;}
        private BaseState _queuedState;
        private bool _rejectInput;
        
        private int _currentAnimation;

        private MovementController _movementController;
        public bool CanCombo { get; private set; }

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
            _movementController = GetComponent<MovementController>();
            AnimatorComponent = GetComponent<Animator>();
            
            
            foreach (AnimationClip clip in AnimatorComponent.runtimeAnimatorController.animationClips)
            {
                if (_noEndEventClips.Contains(clip)) continue;

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

        public void PlayAnimation(int animationState, bool defaultCombo = false)
        {
            if (_currentAnimation == animationState) return;
            // _movementController.StopMoving(null); //need to stop for duration of animation
            //Debug.Log(this.name);
            _currentAnimation = animationState;
            CanCombo = defaultCombo;
            AnimatorComponent.Play(animationState, -1, 0);
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

        /// <summary>
        /// Invoked at the end of an animation. Automatically added to each animation,
        /// and either executes a queued State, or sets the State back to the initial State.
        /// </summary>
        public void HandleAnimationExit()
        {
            //Debug.Log(this.name + " exiting anim");
            TrySetQueueInitial();
            ExecuteQueuedState();   
        }

        private void HandleStateExit()
        {
            _currentAnimation = 0;
        }
    }
}
