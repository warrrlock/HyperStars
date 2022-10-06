using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class AttackInfo
{
    public float _knockbackDuration;
    public float _knockbackMagnitude;
    public float _hitStunDuration;
}

namespace FiniteStateMachine {
    [RequireComponent(typeof(Animator))] 
    public class BaseStateMachine : MonoBehaviour 
    {
        [SerializeField] private BaseState _initialState; 
        private BaseState CurrentState {get; set;}
        private BaseState _queuedState;
        
        private Dictionary<Type, Component> _cachedComponents;
        public Animator AnimatorComponent { get; private set; }
        public AttackInfo AttackInformation => CurrentState.GetAttackInfo();

        
        private void Awake()
        {
            CurrentState = _initialState;
            _cachedComponents = new Dictionary<Type, Component>();
            AnimatorComponent = GetComponent<Animator>();
            
            foreach (AnimationClip clip in AnimatorComponent.runtimeAnimatorController.animationClips)
            {
                if (clip.name == "lisa_ground_idle") continue;
                // AnimationEvent animationStartEvent = new AnimationEvent
                // {
                //     time = 0,
                //     functionName = "AnimationEnterHandler"
                // };
                // clip.AddEvent(animationStartEvent);

                AnimationEvent animationEndEvent = new AnimationEvent
                {
                    time = clip.length,
                    functionName = "AnimationExitHandler"
                };
                clip.AddEvent(animationEndEvent);
            }
        }

        private void Start()
        {
            foreach (KeyValuePair<string, InputManager.Action> entry in FindObjectOfType<InputManager>().Actions)
            {
                entry.Value.perform += Invoke;
            }
        }
        //utils
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
            // Debug.Log("invoked " + action.inputAction.name + " with current State: " + CurrentState.name);
            CurrentState.Execute(this, action.inputAction.name);
        }
        
        public void QueueState(BaseState state)
        {
            // Debug.Log("queueing state " + state.name);
            if (_queuedState == null) _queuedState = state;
        }

        public void TrySetStateInitial()
        {
            if (_queuedState != null) CurrentState = _initialState;
        }
        
        public void ForceSetStateInitial()
        {
            CurrentState = _initialState;
            _queuedState = null;
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
            
            // Debug.Log("want to execute "+ _queuedState.name);
            CurrentState.ResetVariables();
            
            CurrentState = _queuedState;
            _queuedState = null;
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

        private void AnimationEnterHandler()
        {
            // Debug.Log("entering anim");
        }
        /// <summary>
        /// Invoked at the end of an animation. Automatically added to each animation,
        /// and either executes a queued State, or sets the State back to the initial State.
        /// </summary>
        private void AnimationExitHandler()
        {
            // Debug.Log("exiting anim");
            BaseState temp = CurrentState;
            
            if (_queuedState) 
                ExecuteQueuedState();
            else
            {
                CurrentState = _initialState;
                temp.HandleExit();
                CurrentState.Execute(this, "");
            }
                
        }
    }
}
