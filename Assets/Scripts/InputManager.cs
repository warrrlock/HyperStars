using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputManager : MonoBehaviour
{
    public readonly Dictionary<string, Action> Actions = new();

    private PlayerInput _playerInput;

    public class Action
    {
        public int disabledCount;
        public bool isBeingPerformed;
        public delegate void Perform(Action action);
        public Perform perform;
        public delegate void Stop(Action action);
        public Stop stop;
        //public bool isBeingInput;
        public bool isQueued;
        public IEnumerator queue;
        public InputAction inputAction;
    }

    private void Awake()
    {
        AssignComponents();
        CreateDictionary();
    }

    private void Start()
    {
        _playerInput.onActionTriggered += ResolveActions;
    }

    private void OnDestroy()
    {
        _playerInput.onActionTriggered -= ResolveActions;
    }

    private void AssignComponents()
    {
        _playerInput = GetComponent<PlayerInput>();
    }

    private void CreateDictionary()
    {
        foreach(InputAction action in _playerInput.actions)
        {
            Actions.Add(action.name, new Action());
        }
    }

    private void ResolveActions(InputAction.CallbackContext context)
    {
        foreach (KeyValuePair<string, Action> actionPair in Actions)
        {
            if (context.action != _playerInput.actions[actionPair.Key])
            {
                continue;
            }
            Action action = actionPair.Value;
            if (context.action.WasPerformedThisFrame())
            {
                //action.isBeingInput = true;
                action.inputAction = context.action;
                if (action.disabledCount == 0)
                {
                    action.perform?.Invoke(action);
                }
                else if (action.disabledCount > 0)
                {
                    action.queue = QueueAction(action);
                    StartCoroutine(action.queue);
                }
                else
                {
                    throw new System.Exception("Action's disabled count is less than 0.");
                }
            }
            if (context.action.WasReleasedThisFrame())
            {
                //action.isBeingInput = false;
                action.stop?.Invoke(action);
                if (action.isQueued)
                {
                    StopCoroutine(action.queue);
                }
            }
        }
    }

    private IEnumerator QueueAction(Action action)
    {
        action.isQueued = true;
        yield return new WaitUntil(() => action.disabledCount == 0);
        action.isQueued = false;
        action.perform.Invoke(action);
        yield break;
    }
}
