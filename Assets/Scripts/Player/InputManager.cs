using System;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Search;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputManager : MonoBehaviour
{
    public readonly Dictionary<string, Action> Actions = new();

    private PlayerInput _playerInput;

    public class Action
    {
        public string name;
        public int disabledCount;
        public bool isBeingPerformed;
        public delegate void Perform(Action action);
        public Perform perform;
        public delegate void Stop(Action action);
        public Stop stop;
        public delegate void Finish(Action action);
        public Finish finish;
        //public bool isBeingInput;
        public bool isPerformQueued;
        public bool isStopQueued;
        public IEnumerator queuePerform;
        public IEnumerator queueStop;
        public InputAction inputAction;

        public Action(string nam)
        {
            name = nam;

            perform += IsPerformed;
            stop += IsntPerformed;
        }

        public void Destroy()
        {

        }

        private void IsPerformed(Action action)
        {
            isBeingPerformed = true;
        }

        private void IsntPerformed(Action action)
        {
            isBeingPerformed = false;
        }
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
        foreach (KeyValuePair<string, Action> actionPair in Actions)
        {
            actionPair.Value.Destroy();
        }
    }

    private void AssignComponents()
    {
        _playerInput = GetComponent<PlayerInput>();
    }

    private void CreateDictionary()
    {
        foreach(InputAction action in _playerInput.actions)
        {
            Actions.Add(action.name, new Action(action.name));
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
            action.inputAction = context.action;
            if (context.action.WasPerformedThisFrame())
            {
                //action.isBeingInput = true;
                if (action.disabledCount == 0)
                {
                    action.perform?.Invoke(action);
                }
                else if (action.disabledCount > 0)
                {
                    action.queuePerform = QueuePerform(action);
                    StartCoroutine(action.queuePerform);
                }
                else
                {
                    throw new System.Exception("Action's disabled count is less than 0.");
                }

                if (action.isStopQueued)
                {
                    StopCoroutine(action.queueStop);
                }
            }
            if (context.action.WasReleasedThisFrame())
            {
                //action.isBeingInput = false;
                if (action.disabledCount == 0)
                {
                    //TODO: either find a better solution to this or change based off gamepad or keyboard because now on keyboard if you input move right after letting go you'll stop
                    if (_playerInput.currentControlScheme == "Gamepad")
                    {
                        StartCoroutine(Stop(action));
                    }
                    else
                    {
                        action.stop?.Invoke(action);
                    }
                }
                else if (action.disabledCount > 0)
                {
                    action.queueStop = QueueStop(action);
                    StartCoroutine(action.queueStop);
                }
                else
                {
                    throw new System.Exception("Action's disabled count is less than 0.");
                }

                if (action.isPerformQueued)
                {
                    StopCoroutine(action.queuePerform);
                }
            }
        }
    }

    public IEnumerator Disable(float duration, params Action[] actionsToDisable)
    {
        foreach (InputManager.Action action in actionsToDisable)
        {
            action.disabledCount++;
        }
        yield return new WaitForSeconds(duration);

        foreach (InputManager.Action action in actionsToDisable)
        {
            action.disabledCount--;
        }
        yield break;
    }

    public IEnumerator Disable(Func<bool> enableCondition, params Action[] actionsToDisable)
    {
        foreach (InputManager.Action action in actionsToDisable)
        {
            action.disabledCount++;
        }
        yield return null;
        yield return new WaitForFixedUpdate();
        yield return new WaitUntil(enableCondition);

        foreach (InputManager.Action action in actionsToDisable)
        {
            action.disabledCount--;
        }
        yield break;
    }

    private IEnumerator QueuePerform(Action action)
    {
        action.isPerformQueued = true;
        yield return new WaitUntil(() => action.disabledCount == 0);
        action.isPerformQueued = false;
        action.perform?.Invoke(action);
        yield break;
    }

    private IEnumerator QueueStop(Action action)
    {
        action.isStopQueued = true;
        yield return new WaitUntil(() => action.disabledCount == 0);
        action.isStopQueued = false;
        action.stop?.Invoke(action);
        yield break;
    }

    /// <summary>
    /// This prevents a bug where sometimes the gamepad registers the control stick as performed and released in the same frame and thus keeps moving in the last performed direction without stopping.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    private IEnumerator Stop(Action action)
    {
        bool isAwaitingStop = true;
        StartCoroutine(Disable(() => isAwaitingStop == false, Actions["Move"]));
        yield return null;
        yield return null;
        yield return null;
        action.stop?.Invoke(action);
        isAwaitingStop = false;
        yield break;
    }

    public void StopMove()
    {
        StartCoroutine(Stop(Actions["Move"]));
    }
}
