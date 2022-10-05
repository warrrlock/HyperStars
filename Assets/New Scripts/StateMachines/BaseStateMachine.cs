using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FiniteStateMachine {
    [RequireComponent(typeof(Animator))]
    public class BaseStateMachine : MonoBehaviour
{
    [SerializeField] private BaseState initialState;
    public BaseState CurrentState {get; set;}
    private BaseState _queuedState;
    
    [Tooltip("Components get cached upon first GetComponent.")]
    private Dictionary<Type, Component> _cachedComponents;
    public Animator AnimatorComponent { get; private set; }
    [Tooltip("The current state the player is in. This state is used to check for transitions.")]
    
    private void Awake(){
        CurrentState = initialState;
        _cachedComponents = new Dictionary<Type, Component>();
        AnimatorComponent = GetComponent<Animator>();
        
        foreach (AnimationClip clip in AnimatorComponent.runtimeAnimatorController.animationClips)
        {
            if (clip.name == "nlisa_idle") continue;
            AnimationEvent animationStartEvent = new AnimationEvent
            {
                time = 0,
                functionName = "AnimationEnterHandler"
            };
            clip.AddEvent(animationStartEvent);

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
    public new T GetComponent<T>() where T: Component {
        if (_cachedComponents.ContainsKey(typeof(T)))
            return _cachedComponents[typeof(T)] as T;
        
        var component = base.GetComponent<T>();
        
        if (component != null)
            _cachedComponents.Add(typeof(T), component);

        return component;
    }

    private void Invoke(InputManager.Action action) {
        Debug.Log("invoked " + action.inputAction.name + " with current State: " + CurrentState.name);
        CurrentState.Execute(this, action.inputAction.name);
    }
    
    public void ExecuteState(String inputName = "") {
        Debug.Log("executed");
        CurrentState.Execute(this, inputName);
    }

    public void ExecuteQueuedState()
    {
        Debug.Log("executing queued state");
        if (_queuedState == null) return;
        
        Debug.Log("want to execute "+ _queuedState.name);
        CurrentState.ResetVariables();
        
        CurrentState = _queuedState;
        _queuedState = null;
        CurrentState.Execute(this, "");
    }
    
    //for animation
    public void DisableCombo()
    {
        CurrentState.DisableCombo();
    }
    
    public void EnableCombo()
    {
        CurrentState.EnableCombo();
    }

    public void QueueState(BaseState state)
    {
        Debug.Log("queueing state " + state.name);
        if (_queuedState == null) _queuedState = state;
    }

    public void TrySetStateInitial()
    {
        if (_queuedState != null) CurrentState = initialState;
    }
    
    public void ForceSetStateInitial()
    {
        CurrentState = initialState;
        _queuedState = null;
    }
    
    private void AnimationEnterHandler()
    {
        Debug.Log("entering anim");
    }
    
    private void AnimationExitHandler()
    {
        Debug.Log("exiting anim");
        BaseState temp = CurrentState;
        TrySetStateInitial();
        
        if (_queuedState) 
            ExecuteQueuedState();
        else
        {
            CurrentState = initialState;
            temp.HandleExit();
        }
            
    }
}
}
