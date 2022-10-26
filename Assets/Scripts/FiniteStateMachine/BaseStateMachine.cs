using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FiniteStateMachine;
using TMPro;
using UnityEngine;

[Serializable]
public class KeyHurtStatePair
{
    public enum HurtStateName
    {
        HitStun, KnockBack
    }
    public HurtStateName key;
    public HurtState value;
}

namespace FiniteStateMachine {
    [RequireComponent(typeof(Animator))] 
    [RequireComponent(typeof(Fighter))] 
    public class BaseStateMachine : MonoBehaviour 
    {
        
        public BaseState CurrentState {get; private set;}
        public bool CanCombo { get; private set; }
        public AttackInfo AttackInfo => CurrentState.GetAttackInfo();

        [SerializeField] private TextMeshProUGUI _stateInfoText;
        [SerializeField] private BaseState _initialState;
        [Tooltip("Clips that should not have a end event automatically added. " +
                 "The end event resets variables of the current state, then returns the player to the initial state." +
                 "\n\n****Add all looping animations.")]
        [SerializeField] private List<AnimationClip> _noEndEventClips;
        
        [SerializeField] private List<KeyHurtStatePair> _hurtStatePairs;
        private Dictionary<KeyHurtStatePair.HurtStateName, HurtState> _hurtStates;
        private Coroutine _hurtCoroutine;
        
        private BaseState _queuedState;
        private bool _rejectInput;
        private int _currentAnimation;
        private bool _isAttacking;

        public Fighter Fighter { get; private set; }
        private Animator _animator;
        private Dictionary<Type, Component> _cachedComponents;

        private void Awake()
        {
            //CurrentState = _initialState;
            _cachedComponents = new Dictionary<Type, Component>();
            Fighter = GetComponent<Fighter>();
            _animator = GetComponent<Animator>();

            _hurtStates = new Dictionary<KeyHurtStatePair.HurtStateName, HurtState>();
            foreach (KeyHurtStatePair entry in _hurtStatePairs)
            {
                //string key = Enum.GetName(typeof(KeyHurtStatePair.HurtStateName), entry.key);
                _hurtStates.Add(entry.key, entry.value);
            }
            
            foreach (AnimationClip clip in _animator.runtimeAnimatorController.animationClips)
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
            foreach (KeyValuePair<string, InputManager.Action> entry in Fighter.InputManager.Actions)
                entry.Value.perform += Invoke;
            Fighter.InputManager.Actions["Dash"].finish += Stop;
            Fighter.InputManager.Actions["Move"].stop += Stop;

            CurrentState = _initialState;
            CurrentState.Execute(this, "");
            UpdateStateInfoText();
        }

        private void OnDestroy()
        {
            foreach (KeyValuePair<string, InputManager.Action> entry in Fighter.InputManager.Actions)
                entry.Value.perform -= Invoke;
            Fighter.InputManager.Actions["Dash"].finish -= Stop;
            Fighter.InputManager.Actions["Move"].stop -= Stop;
            StopAllCoroutines();
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
            // Debug.Log(this.name + " invoked " + action.name + " with current State: " + CurrentState.name);
            CurrentState.Execute(this, action.name);
        }

        private void Stop(InputManager.Action action)
        {
            //Debug.Log(action.inputAction.name);
            // Debug.Log(this);
            CurrentState.Stop(this, action.name);
        }

        public bool PlayAnimation(int animationState, bool defaultCombo = false, bool replay = false)
        {
            if (_currentAnimation == animationState && !replay) return false;
            //Debug.Log(this.name);
            _currentAnimation = animationState;
            CanCombo = defaultCombo;
            _animator.Play(animationState, -1, 0);
            
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
            // Debug.Log("going to execute "+ _queuedState.name);
            _rejectInput = true;
            
            HandleStateExit();
            CurrentState = _queuedState;
            UpdateStateInfoText();
            _queuedState = null;
            _hurtCoroutine = null;
           
            _rejectInput = false;

            CurrentState.Execute(this, "");
        }
        
        /// <summary>
        /// Invoked at the end of an animation. Automatically added to each animation,
        /// and either executes a queued State, or sets the State back to the initial State.
        /// </summary>
        public void HandleAnimationExit()
        {
            // Debug.Log(this.name + " exiting anim " + callee);
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

        public IEnumerator SetHurtState(KeyHurtStatePair.HurtStateName stateName)
        {
            // yield return null;
            yield return new WaitForFixedUpdate();
            _hurtStates.TryGetValue(stateName, out HurtState state);
            if (state) ForceSetState(state);
        }
        
        private void ForceSetState(BaseState state)
        {
            _queuedState = state;
            ExecuteQueuedState();
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
            Fighter.MovementController.EnableAttackStop();
        }
        
        private void DisableAttackStop()
        {
            if (!_isAttacking) return;
            _isAttacking = false;
            Fighter.MovementController.DisableAttackStop();
            //need some way to return to walk state upon exit, if player is still holding onto move input
        }
        
        public void StartInAir(Action onGroundAction = null)
        {
            StartCoroutine(HandleExitInAir(onGroundAction));
        }

        private IEnumerator HandleExitInAir(Action onGroundAction)
        {
            yield return new WaitForFixedUpdate();
            yield return new WaitUntil(() => Fighter.MovementController.CollisionData.y.isNegativeHit);
            //when out of air, return to idle
            onGroundAction ??= HandleAnimationExit;
            onGroundAction();
        }

        public void WaitToMove(int nextAnimation = -1)
        {
            if (_hurtCoroutine != null) StopCoroutine(_hurtCoroutine);
            _hurtCoroutine = StartCoroutine(HandleWaitToMove(nextAnimation));
        }

        private IEnumerator HandleWaitToMove(int nextAnimation)
        {
            if (nextAnimation != -1) PlayAnimation(nextAnimation);
            yield return new WaitUntil(() => Fighter.InputManager.Actions["Move"].disabledCount == 0);
            HandleAnimationExit();
        }

        private void UpdateStateInfoText()
        {
            if (!_stateInfoText) return;
            _stateInfoText.text = "State: " + CurrentState.name;
        }
    }
}
