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
        HitStun, KnockBack, AirKnockBack
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

        [Header("UI")]
        [SerializeField] private TextMeshProUGUI _stateInfoText;
        
        [Header("States")]
        [SerializeField] private BaseState _initialState;
        [SerializeField] private BaseState _jumpState;
        private BaseState _returnState;
        public bool IsIdle => CurrentState == _initialState;
    
        [Tooltip("Clips that should not have a end event automatically added. " +
                 "The end event resets variables of the current state, then returns the player to the initial state." +
                 "\n\n****Add all looping animations.")]
        [Header("Special cases")]
        [SerializeField] private List<AnimationClip> _noEndEventClips;
        [SerializeField] private List<KeyHurtStatePair> _hurtStatePairs;
        [SerializeField] private Collider _hitCollider;
        
        private Dictionary<KeyHurtStatePair.HurtStateName, HurtState> _hurtStates;
        public float DisableTime { get; set; }
        private bool _isDisabled;
        private Coroutine _disableCoroutine;
        private Coroutine _airCoroutine;
        
        private BaseState _queuedState;
        private bool _rejectInput;
        private int _currentAnimation;
        private bool _isAttacking;
        private string _lastExecutedInput;
        private bool _crouchStop;

        public string LastExecutedInput
        {
            get => _lastExecutedInput;
            set => _lastExecutedInput = value;
        }
        public BaseState QueuedState => _queuedState;
        public InputManager.Action LastInvokedInput { get; private set; }

        public Fighter Fighter { get; private set; }
        private Animator _animator;
        // private Dictionary<Type, Component> _cachedComponents;

        #region Methods
        #region Unity
        private void Awake()
        {
            // _cachedComponents = new Dictionary<Type, Component>();
            Fighter = GetComponent<Fighter>();
            _animator = GetComponent<Animator>();

            _hurtStates = new Dictionary<KeyHurtStatePair.HurtStateName, HurtState>();
            foreach (KeyHurtStatePair entry in _hurtStatePairs)
            {
                _hurtStates.TryAdd(entry.key, entry.value);
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
            Fighter.InputManager.Actions["Crouch"].stop += Stop;

            ResetStateMachine();
            UpdateStateInfoText();
        }

        private void OnDestroy()
        {
            foreach (KeyValuePair<string, InputManager.Action> entry in Fighter.InputManager.Actions)
                entry.Value.perform -= Invoke;
            Fighter.InputManager.Actions["Dash"].finish -= Stop;
            Fighter.InputManager.Actions["Move"].stop -= Stop;
            Fighter.InputManager.Actions["Crouch"].stop -= Stop;
            StopAllCoroutines();
        }
        #endregion

        // private new T GetComponent<T>() where T: Component 
        // {
        //     if (_cachedComponents.ContainsKey(typeof(T)))
        //         return _cachedComponents[typeof(T)] as T;
        //     
        //     var component = base.GetComponent<T>();
        //     
        //     if (component != null)
        //         _cachedComponents.Add(typeof(T), component);
        //
        //     return component;
        // }
        public void ResetStateMachine()
        {
            CurrentState = _initialState;
            _returnState = _initialState;
            QueueState();
            CurrentState.Execute(this, "");
        }
        
        private void Invoke(InputManager.Action action)
        {
            if (_rejectInput || CurrentState is HurtState) return;
            if (_crouchStop) _crouchStop = false;
            LastInvokedInput = action;
            // Debug.Log(this.name + " invoked " + action.name + " with current State: " + CurrentState.name);
            CurrentState.Execute(this, action.name);
        }

        private void Stop(InputManager.Action action)
        {
            // Debug.Log($"Stop called by {action.name}, with last played action being {_lastExecutedInput}");
            // Debug.Log($"current state is {CurrentState.name}");
            if (action.name == "Crouch")
                _crouchStop = true;
            
            if (_returnState == _initialState     && _lastExecutedInput != action.name
                                                  && _lastExecutedInput != "Crouch" 
                                                  && _lastExecutedInput != "") return;
            CurrentState.Stop(this, action.name);
        }

        public bool CheckReturnState(BaseState state = null)
        {
            return _returnState == state ? state : _initialState;
        }

        public bool PlayAnimation(int animationState, bool defaultCombo = false, bool replay = false)
        {
            if (_currentAnimation == animationState && !replay) return false;
            _currentAnimation = animationState;
            CanCombo = defaultCombo;
            _animator.Play(animationState, -1, 0);
            return true;
        }
        
        public void QueueState(BaseState state = null)
        {
            // Debug.Log($"{name} queuing state {state?.name}");
            // if (!_queuedState) _queuedState = state;
            _queuedState = state;
        }
        
        /// <summary>
        /// Execute the queued State.
        /// In animation, this can be invoked through an event,
        /// and occurs at the end of an animation if there exists a queued State.
        /// </summary>
        public void ExecuteQueuedState()
        {
            if (!_queuedState) return;
            
            _rejectInput = true;
            
            HandleStateExit();
            CurrentState = _queuedState;
            UpdateStateInfoText();
            Fighter.Events.onStateChange.Invoke(CurrentState);
            _queuedState = null;

            _rejectInput = false;

            CurrentState.Execute(this, "");
            if (_crouchStop)
            {
                Stop(Fighter.InputManager.Actions["Crouch"]);
                _crouchStop = false;
            }
        }
        
        /// <summary>
        /// Invoked at the end of an animation. Automatically added to each animation,
        /// and either executes a queued State, or sets the State back to the initial State.
        /// </summary>
        public void HandleAnimationExit()
        {
            TrySetQueueReturn();
            // TrySetQueueInitial();
            ExecuteQueuedState();
        }
        
        private void HandleStateExit()
        {
            _currentAnimation = -1;
            if (_isAttacking) DisableAttackStop();
            Fighter.OpposingFighter.ResetFighterHurtboxes();
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
        
        public void SpawnProjectile()
        {
            Bounds? bounds = _hitCollider?.bounds;
            if (_hitCollider) _hitCollider.enabled = false;
            bounds ??= GetComponent<Renderer>()?.bounds;
            if (bounds == null) return;
            
            CurrentState.SpawnProjectile(this, (Bounds)bounds);
        }
        
        //OTHER METHODS

        public IEnumerator SetHurtState(KeyHurtStatePair.HurtStateName stateName)
        {
            yield return new WaitForFixedUpdate();
            _hurtStates.TryGetValue(stateName, out HurtState state);
            if (!state) yield break;
            SetReturnState();
            if (CurrentState is HurtState hurtState && hurtState == state)
                CurrentState.Execute(this, "");
            else  ForceSetState(state);
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
        
        private void TrySetQueueReturn()
        {
            if (!_queuedState && CurrentState != _returnState) _queuedState = _returnState;
        }

        public void SetReturnState(BaseState state = null)
        {
            _returnState = state ? state : _initialState;
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
            //TODO: return to walk state upon exit, if player is still holding onto move input
        }
        
        public void StartInAir(Action onGroundAction = null, bool setJumpReturnState = true)
        {
            if (_airCoroutine != null) StopCoroutine(_airCoroutine);
            if (setJumpReturnState)
                SetReturnState(_jumpState);
            else SetReturnState();
            _airCoroutine = StartCoroutine(HandleExitInAir(onGroundAction));
            StartCoroutine(Fighter.InputManager.Disable(
                () => Fighter.MovementController.CollisionData.y.isNegativeHit, 
                Fighter.InputManager.Actions["Crouch"]));
        }

        private IEnumerator HandleExitInAir(Action onGroundAction)
        {
            yield return new WaitForFixedUpdate();
            yield return new WaitUntil(() => Fighter.MovementController.CollisionData.y.isNegativeHit);
            SetReturnState();
            
            if (CurrentState is HurtState) Fighter.Events.onLandedHurt?.Invoke();
            else Fighter.Events.onLandedNeutral?.Invoke();
            
            //when out of air, return to idle or execute given action
            onGroundAction ??= HandleAnimationExit;
            onGroundAction();
            _airCoroutine = null;
        }

        public void WaitToMove(int nextAnimation = -1, Func<bool> condition = null)
        {
            StartCoroutine(HandleWaitToMove(nextAnimation, condition));
        }

        private IEnumerator HandleWaitToMove(int nextAnimation, Func<bool> condition, bool stateExit = false)
        {
            if (nextAnimation != -1) PlayAnimation(nextAnimation);
            // Debug.Log($"{name} waiting to move, current move is disabled at {Fighter.InputManager.Actions["Move"].disabledCount}");
            yield return new WaitUntil(condition ?? (() => !_isDisabled));
            // Debug.Log($"{name} starting to move, current move is disabled at {Fighter.InputManager.Actions["Move"].disabledCount}");
            
            if (stateExit) HandleStateExit();
            else HandleAnimationExit();
        }

        public void DisableInputs(List<string> inputs, Func<bool> condition, bool returnToIdle = true)
        {
            IEnumerable<InputManager.Action> actionIEnum = inputs.Select(input => Fighter.InputManager.Actions[input]);
            InputManager.Action[] actions = actionIEnum.ToArray();
            
            StartCoroutine(Fighter.InputManager.Disable(condition, actions));
            StartCoroutine(HandleWaitToMove(-1, condition, !returnToIdle));
        }

        public void ExecuteDisableTime()
        {
            _isDisabled = true;
            if (_disableCoroutine != null) StopCoroutine(_disableCoroutine);
            _disableCoroutine = StartCoroutine(HandleDisableTime());
        }

        private IEnumerator HandleDisableTime()
        {
            yield return new WaitForSeconds(DisableTime);
            _isDisabled = false;
            _disableCoroutine = null;
        }

        private void UpdateStateInfoText()
        {
            if (!_stateInfoText) return;
            _stateInfoText.text = "State: " + CurrentState.name;
        }
        #endregion
    }
}
