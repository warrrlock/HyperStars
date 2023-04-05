using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FiniteStateMachine;
using TMPro;
using UnityEngine;
using Util;
using Object = System.Object;

[Serializable]
public class KeyHurtStatePair
{
    public enum HurtStateName
    {
        HitStun, KnockBack, AirKnockBack, WallBounce, GroundBounce
    }
    public HurtStateName key;
    public HurtState value;
}

[Serializable]
public class StateEvent
{
    public Action execute;
    public Action stop;
}

namespace FiniteStateMachine {
    [RequireComponent(typeof(Animator))] 
    [RequireComponent(typeof(Fighter))] 
    public class BaseStateMachine : MonoBehaviour
    {
        //Meta info
        public bool IgnoreExecuteState { get; set; }
        public SerializedDictionary<BaseState, StateEvent> States;

        public BaseState CurrentState {get; private set;}
        public bool CanCombo(bool alwaysHitConfirm = false) => _canCombo && (HitOpponent || alwaysHitConfirm);
        private bool _canCombo;
        private bool CanInputQueue { get; set; }
        public AttackInfo AttackInfo => CurrentState.GetAttackInfo();
        
        private bool _canRecover;

        [Header("Input Queuing")] 
        [SerializeField] private int _framesBeforeEnd = 5;
        
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI _stateInfoText;
        
        [Header("States")]
        [SerializeField] private BaseState _initialState;
        [SerializeField] private BaseState _jumpState;
        [SerializeField] private BaseState _jumpLandState;
        [SerializeField] private BaseState _crouchState;
        [SerializeField] private BaseState _crouchUpState;
        [SerializeField] private BaseState _hurtRollState;
        public bool IsIdle => CurrentState == _initialState;
        public bool IsCrouch => CurrentState == _crouchState;
        private bool InAir => CurrentState is InAirState;
    
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
        private Coroutine _waitToAnimateRoutine;
        
        private BaseState _queuedState;
        private BaseState _queuedAtEndState;
        private BaseState _returnState;
        
        private bool _rejectInput;
        private int _currentAnimation;
        private bool _isAttacking;
        private bool _holdingCrouch;

        public bool HitOpponent { get; private set; }

        // private bool _queueJumpOnGround;

        public string LastExecutedInput { get; set; }

        public BaseState QueuedState => _queuedState ? _queuedState : _queuedAtEndState;
        public InputManager.Action LastInvokedInput { get; private set; }

        public Fighter Fighter { get; private set; }
        private Animator _animator;


        #region Methods
        #region Unity
        private void Awake()
        {
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
                
                float frameRate = clip.frameRate;
                float numberOfFrames = clip.length * frameRate;
                AnimationEvent inputBufferEvent = new AnimationEvent
                {
                    time = Math.Max(numberOfFrames - _framesBeforeEnd, 0) / frameRate,
                    functionName = "EnableInputQueue"
                };
                clip.AddEvent(inputBufferEvent);
            }

            CreateStateEvents();
        }

        private void Start()
        {
            SubscribeActions();
            ResetStateMachine();
            UpdateStateInfoText();
        }

        private void OnDestroy()
        {
            UnSubscribeActions();
            StopAllCoroutines();
        }
        #endregion

        private void CreateStateEvents()
        {
            //create state events
            foreach (BaseState state in Services.Characters[Fighter.PlayerId].States)
            {
                States.Add(state, new StateEvent());
            }
        }
        
        public void ResetStateMachine()
        {
            IgnoreExecuteState = false;
            _airCoroutine = null;
            _waitToAnimateRoutine = null;
            CancelDisableTime();
            
            CurrentState = _initialState;
            _returnState = _initialState;

            ClearQueues();
            CurrentState.Execute(this, "");
        }

        public void BypassQueuing(BaseState state)
        {
            CurrentState = _initialState;
        }
        
        private void SubscribeActions()
        {
            Fighter.InputManager.Actions["Jump"].perform += ExecuteJump;
            
            foreach (KeyValuePair<string, InputManager.Action> entry in Fighter.InputManager.Actions)
                entry.Value.perform += Invoke;
            Fighter.InputManager.Actions["Dash"].finish += Finish;
            if (Fighter.InputManager.Actions.ContainsKey("Dash Left"))
            {
                Fighter.InputManager.Actions["Dash Left"].finish += Finish;
            }
            if (Fighter.InputManager.Actions.ContainsKey("Dash Right"))
            {
                Fighter.InputManager.Actions["Dash Right"].finish += Finish;
            }
            
            Fighter.InputManager.Actions["Move"].stop += Stop;
            Fighter.InputManager.Actions["Crouch"].stop += Stop;

            Fighter.Events.onAttackHit += SetHitOpponent;
            Fighter.Events.wallBounce += () => UpdateHurtState(KeyHurtStatePair.HurtStateName.WallBounce);
            Fighter.Events.groundBounce += () => UpdateHurtState(KeyHurtStatePair.HurtStateName.GroundBounce);
        }
        
        private void UnSubscribeActions()
        {
            Fighter.InputManager.Actions["Jump"].perform -= ExecuteJump;
            
            foreach (KeyValuePair<string, InputManager.Action> entry in Fighter.InputManager.Actions)
                entry.Value.perform -= Invoke;
            Fighter.InputManager.Actions["Dash"].finish -= Finish;
            if (Fighter.InputManager.Actions.ContainsKey("Dash Left"))
            {
                Fighter.InputManager.Actions["Dash Left"].finish -= Finish;
            }
            if (Fighter.InputManager.Actions.ContainsKey("Dash Right"))
            {
                Fighter.InputManager.Actions["Dash Right"].finish -= Finish;
            }
            
            Fighter.InputManager.Actions["Move"].stop -= Stop;
            Fighter.InputManager.Actions["Crouch"].stop -= Stop;
        }
        
        private void Invoke(InputManager.Action action)
        {
            if (_rejectInput) return;
            if (CurrentState is HurtState)
            {
                if (_canRecover)
                {
                    // Debug.Log("setting roll state.");
                    ForceSetState(_hurtRollState);
                }
                return;
            }
            LastInvokedInput = action;

            // Debug.Log(this.name + " invoked " + action.name + " with current State: " + CurrentState.name);
            //TODO: should wait until idle from crouch to begin queuing attacks, or go straight into attack?
            // if (!_holdingCrouch && CurrentState.IsCrouchState) return; //waiting to return to idle. otherwise, go to queue execute
            if (_holdingCrouch && !CurrentState.IsCrouchState)
            {
                SetReturnState(_crouchState);
                _crouchState.QueueExecute(this, action.name);
                return;
            }

            if (!CurrentState.Execute(this, action.name))
            {
                if (!InAir && (CanInputQueue || action.name == "Crouch"))
                {
                    // Debug.Log($"queue execute {action.name}, current state: {CurrentState.name}, return state: {_returnState.name}");
                    _returnState.QueueExecute(this, action.name);
                }
            }

            if (action.name == "Crouch")
            {
                _holdingCrouch = true;
            }
        }

        private void Stop(InputManager.Action action)
        {
            // Debug.LogWarning($"Stop called by {action.name}, with last played action being {_lastExecutedInput}, current State in {CurrentState.name}");
            if (action.name == "Crouch")
            {
                _holdingCrouch = false;
                SetReturnState();
                ClearQueues();
                if (CurrentState.IsCrouchState)
                {
                    QueueStateAtEnd(_crouchUpState);
                }
            }
            // States[CurrentState].stop?.Invoke();
            CurrentState.Stop(this, action.name);
        }
        
        private void Finish(InputManager.Action action)
        {
            CurrentState.Finish(this);
        }

        private void ExecuteJump(InputManager.Action action)
        {
            QueueState(_jumpState);
            ExecuteQueuedState();
        }

        public bool PlayAnimation(int animationState, bool defaultCombo = false, bool replay = false)
        {
            if (_currentAnimation == animationState && !replay) return false;
            // Debug.Log($"playing animation for {CurrentState.name}");
            _currentAnimation = animationState;
            DisableInputQueue();
            _canCombo = defaultCombo;
            _animator.Play(animationState, -1, 0);
            if (States.ContainsKey(CurrentState)) States[CurrentState].execute?.Invoke();
            return true;
        }
        
        private void SetHitOpponent(Dictionary<string, Object> message)
        {
            HitOpponent = true;
        }

        public void ClearQueues()
        {
            // Debug.Log("clearing queues");
            QueueState();
            QueueStateAtEnd();
        }
        
        public void QueueState(BaseState state = null)
        {
            // Debug.Log($"queued state {state?.name}");
            _queuedState = state;
        }
        
        public void TryQueueState(BaseState state)
        {
            if (!_queuedState) _queuedState = state;
        }

        public void QueueStateAtEnd(BaseState state = null)
        {
            // Debug.Log($"queued at end {state?.name}");
            _queuedAtEndState = state;
        }
        
        /// <summary>
        /// Execute the queued State.
        /// In animation, this can be invoked through an event,
        /// and occurs at the end of an animation if there exists a queued State.
        /// </summary>
        public void ExecuteQueuedState()
        {
            if (IgnoreExecuteState) return;
            if (!_queuedState) return;
            // Debug.LogError($"executing queued state {_queuedState.name}" +
            //                $"\nwith queued state: {_queuedState?.name}\nand queued at end: {_queuedAtEndState?.name}");
            _rejectInput = true;
            
            HandleStateExit();
            CurrentState = _queuedState;
            UpdateStateInfoText();
            Fighter.Events.onStateChange?.Invoke(CurrentState);
            _queuedState = null;

            _rejectInput = false;

            CurrentState.Execute(this, "");
        }

        private bool _noExitThisFrame;
        private IEnumerator RefuseThisFrame()
        {
            // Debug.Log("refuse this frame");
            _noExitThisFrame = true;
            yield return new WaitForFixedUpdate();
            _noExitThisFrame = false;
        }
        
        /// <summary>
        /// Invoked at the end of an animation. Automatically added to each animation,
        /// and either executes a queued State, or sets the State back to the initial State.
        /// </summary>
        public void HandleAnimationExit()
        {
            // Debug.LogError("handling exit animation");
            // if (_noExitThisFrame) return;
            
            TrySetQueueQueuedAtEndState();
            TrySetQueueReturn();
            ExecuteQueuedState();
        }
        
        private void HandleStateExit()
        {
            // Debug.Log("handling state animation");
            if (States.ContainsKey(CurrentState)) States[CurrentState].stop?.Invoke();
            _currentAnimation = -1;
            HitOpponent = false;
            if (_isAttacking) DisableAttackStop();
            Fighter.OpposingFighter.ResetFighterHurtboxes();
        }

        //ANIMATION USE
        public void DisableCombo()
        {
            _canCombo = false;
        }
        
        public void EnableCombo()
        {
            _canCombo = true;
        }

        private void EnableInputQueue()
        {
            CanInputQueue = true;
        }

        private void DisableInputQueue()
        {
            CanInputQueue = false;
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

        public IEnumerator SetHurtState(KeyHurtStatePair.HurtStateName stateName, float duration)
        {
            yield return new WaitForFixedUpdate();
            _hurtStates.TryGetValue(stateName, out HurtState newHurtState);
            if (!newHurtState) yield break;
            
            _canRecover = false;
            
            // Debug.LogWarning($"setting hurtstate to {stateName}");
            SetReturnState();
            if (CurrentState is HurtState hurtState)
            {
                if (hurtState != newHurtState) //if current hurt state is not the new hurt state, give non hit-stun hits priority
                {
                    if (newHurtState.HurtType != KeyHurtStatePair.HurtStateName.HitStun)
                    {
                        PassHurtState(newHurtState, duration);
                    }
                    yield break;
                }
                // Debug.Log($"re-executing current state of type {hurtState.HurtType}");
                PassHurtState(newHurtState, duration);
            }
            else
                PassHurtState(newHurtState, duration);
        }

        private void UpdateHurtState(KeyHurtStatePair.HurtStateName stateName)
        {
            StartCoroutine(SetHurtState(stateName, 0f));
        }

        private void PassHurtState(HurtState hurtState, float duration)
        {
            if (duration > 0) {
                DisableTime = duration;
                ExecuteDisableTime();
            }
            ForceSetState(hurtState);
            if (duration > 0) DisableInputs(new List<string>{"Move", "Dash", "Jump", "Dash Left", "Dash Right", "Crouch"}, 
                () => IsIdle, false);
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

        private void TrySetQueueQueuedAtEndState()
        {
            if (_queuedState || !_queuedAtEndState) return;
            _queuedState = _queuedAtEndState;
            _queuedAtEndState = null;
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
        }
        
        public void StartInAir(Action onGroundAction = null, bool setJumpReturnState = true)
        {
            if (setJumpReturnState)
                SetReturnState(_jumpState);
            else SetReturnState();
            
            if (_airCoroutine != null) StopCoroutine(_airCoroutine);
            _airCoroutine = StartCoroutine(HandleExitInAir(onGroundAction));
            
            StartCoroutine(Fighter.InputManager.Disable(
                () => Fighter.MovementController.IsGrounded, 
                Fighter.InputManager.Actions["Crouch"]));
        }

        private IEnumerator HandleExitInAir(Action onGroundAction)
        {
            yield return new WaitForFixedUpdate();
            yield return new WaitUntil(() => Fighter.MovementController.IsGrounded);
            SetReturnState();
            
            if (CurrentState is HurtState)
            {
                Fighter.Events.onLandedHurt?.Invoke();
                EnableRecovery();
            }
            else
            {
                Fighter.Events.onLandedNeutral?.Invoke();
                TryQueueState(_jumpLandState);
            }

            if (CurrentState is HurtState { HurtType: KeyHurtStatePair.HurtStateName.HitStun })
            {
                _airCoroutine = null;
                yield break;
            }
            //when out of air, return to idle or execute given action
            onGroundAction ??= HandleAnimationExit;
            onGroundAction();
            _airCoroutine = null;
        }

        public void WaitToAnimate(int nextAnimation = -1, Func<bool> condition = null)
        {
            // Debug.Log("wait to animate, wait to move");
            if (_waitToAnimateRoutine != null) StopCoroutine(_waitToAnimateRoutine);
            _waitToAnimateRoutine = StartCoroutine(HandleWaitToMove(nextAnimation, condition));
        }

        private IEnumerator HandleWaitToMove(int nextAnimation, Func<bool> condition)
        {
            // Debug.Log("handle animate, wait to move");
            if (nextAnimation != -1) PlayAnimation(nextAnimation);
            yield return new WaitUntil(condition ?? (() => !_isDisabled && Fighter.MovementController.IsGrounded));

            _waitToAnimateRoutine = null;
            HandleAnimationExit();
        }

        public void DisableInputs(List<string> inputs, Func<bool> condition, bool returnToIdle = true)
        {
            IEnumerable<InputManager.Action> actionIEnum = inputs.Select(input => Fighter.InputManager.Actions[input]);
            InputManager.Action[] actions = actionIEnum.ToArray();
            
            StartCoroutine(Fighter.InputManager.Disable(condition, actions));
            // _waitToAnimateRoutine = StartCoroutine(HandleWaitToMove(-1, condition, !returnToIdle));
        }

        public void ExecuteDisableTime()
        {
            _isDisabled = true;
            if (_disableCoroutine != null) StopCoroutine(_disableCoroutine);
            _disableCoroutine = StartCoroutine(HandleDisableTime());
            // Debug.Log($"disabling inputs for {DisableTime} courtesy of {CurrentState.name}");
        }

        private IEnumerator HandleDisableTime()
        {
            float time = DisableTime;
            while (time > 0)
            {
                time -= Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
            _isDisabled = false;
            _disableCoroutine = null;
            if (_holdingCrouch)
            {
                SetReturnState(_crouchState);
                // Debug.Log($"player {Fighter.PlayerId} is still holding crouch");
            }
        }

        private void CancelDisableTime()
        {
            if (_disableCoroutine != null) StopCoroutine(_disableCoroutine);
            _isDisabled = false;
            _disableCoroutine = null;
            DisableRecovery();
        }

        private void EnableRecovery()
        {
            _canRecover = true;
            Fighter.InputManager.EnableOneShot(Fighter.InputManager.Actions["Roll"]);
        }
        
        public void DisableRecovery()
        {
            // Debug.Log("disabling recovery");
            Fighter.InputManager.Disable(Fighter.InputManager.Actions["Roll"]);
            _canRecover = false;
        }

        private void UpdateStateInfoText()
        {
            if (!_stateInfoText) return;
            _stateInfoText.text = "State: " + CurrentState.name;
        }
        #endregion
    }
}
