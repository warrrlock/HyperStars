using System;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Search;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using static InputManager;

// [RequireComponent(typeof(PlayerInput))]
public class InputManager : MonoBehaviour
{
    public bool IgnoreQueuePerform { get; set; }
    [Tooltip("How long (in secs) after pressing jump in midair should it be queued to perform upon landing?")]
    private float _jumpQueueTime = 0.1f;

    public readonly Dictionary<string, Action> Actions = new();

    private PlayerInput _playerInput;

    private bool _isAwaitingStop;

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
        public bool isBeingInput;
        public bool isPerformQueued;
        public bool isStopQueued;
        public IEnumerator queuePerform;
        public IEnumerator queueStop;
        public InputAction inputAction;
        public List<Func<bool>> enableConditions = new();
        public IEnumerator queuePerformTime;
        public bool isPerformTimeQueued = false;

        public Action(string nam)
        {
            name = nam;



            //if (inputAction.interactions.Contains("asdf"))
            //{
            //    //IInputInteraction interaction = inputAction.interactions;
            //}

            perform += IsPerformed;
            stop += IsntPerformed;


            //enableConditions = () => true;
        }

        public IEnumerator AddOneShotEnableCondition(Func<bool> condition)
        {
            enableConditions.Add(condition);
            yield return new WaitUntil(condition);

            ClearEnableCondition(condition);
            yield break;
        }

        private void ClearEnableCondition(Func<bool> condition)
        {
            enableConditions.Remove(condition);
        }

        public void Destroy()
        {
            perform -= IsPerformed;
            stop -= IsntPerformed;
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
        SubscribeActions();
    }

    private void Start()
    {
        Disable(Actions["Roll"]); //TODO: better way to do this?
    }

    private void OnDestroy()
    {
        UnsubscribeActions();
        foreach (KeyValuePair<string, Action> actionPair in Actions)
        {
            actionPair.Value.Destroy();
        }
        StopAllCoroutines();
    }

    public void ResetValues()
    {
        StopAllCoroutines();
        IgnoreQueuePerform = false;
        EnableAll();
        Disable(Actions["Roll"]); //TODO: better way to do this?
    }

    private void AssignComponents()
    {
        _playerInput = transform.parent.GetComponent<PlayerInput>();
    }
    
    private void SubscribeActions()
    {
        _playerInput.onActionTriggered += ResolveActions;
    }
    private void UnsubscribeActions()
    {
        _playerInput.onActionTriggered -= ResolveActions;
    }

    private void CreateDictionary()
    {
        foreach(InputAction action in _playerInput.actions)
        {
            Actions.Add(action.name, new Action(action.name));
            //InputState.AddChangeMonitor(action, new());
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
                if (context.action.name == "Submit")
                {
                    Services.CameraManager.FastForwardCutscene();
                }

                action.isBeingInput = true;
                if (action.enableConditions.Count == 0 || action.enableConditions.TrueForAll(x => x()))
                {
                    if (action.disabledCount == 0)
                    {
                        action.perform?.Invoke(action);
                    }
                    else if (action == Actions["Roll"])
                    {
                        //Debug.Log("can't roll");
                    }
                    else if (action.disabledCount > 0)
                    {
                        if (!action.isPerformQueued)
                        {
                            action.queuePerform = QueuePerform(action);
                            StartCoroutine(action.queuePerform);
                        }
                    }
                    else
                    {
                        throw new System.Exception("Action's disabled count is less than 0.");
                    }
                } else if (action == Actions["Jump"])
                {
                    //TODO: make this interruptable, so doing another input will cancel the jump queue
                    if (!action.isPerformTimeQueued)
                    {
                        action.queuePerformTime = QueuePerformTime(action, _jumpQueueTime);
                        StartCoroutine(action.queuePerformTime);
                    }
                    else
                    {
                        StopCoroutine(action.queuePerformTime);
                        action.queuePerformTime = QueuePerformTime(action, _jumpQueueTime);
                        StartCoroutine(action.queuePerformTime);
                    }
                }

                if (action.isStopQueued)
                {
                    StopCoroutine(action.queueStop);
                }
            }


            //TODO: NEW FOR JOYSTICK
            if (context.canceled)
            {
                action.isBeingInput = false;
                action.stop?.Invoke(action);
            }


            if (context.action.WasReleasedThisFrame())
            {
                action.isBeingInput = false;
                
                if (action == Actions["Crouch"])
                {
                    // Debug.Log($"{action.name} was released this frame");
                    action.stop?.Invoke(action);
                }
                if (action.disabledCount == 0)
                {
                    //action.stop?.Invoke(action);
                    //TODO: either find a better solution to this or change based off gamepad or keyboard because now on keyboard if you input move right after letting go you'll stop
                    //if (action == Actions["Move"])
                    //{
                    //    //action.stop?.Invoke(action);
                    //    if (_playerInput.currentControlScheme == "Gamepad" && !_isAwaitingStop)
                    //    {
                    //        StartCoroutine(Stop(action));
                    //    }
                    //    else
                    //    {
                    //        action.stop?.Invoke(action);
                    //    }
                    //}
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
                    action.isPerformQueued = false;
                }
            }
        }
    }

    public void Disable(params Action[] actionsToDisable)
    {
        foreach (InputManager.Action action in actionsToDisable)
        {
            action.disabledCount = int.MaxValue;
        }
        //for (int i = 0; i < actionsToDisable.Length; i++)
        //{
        //    actionsToDisable[i].disabledCount = 1;
        //}
    }

    public IEnumerator Disable(float duration, params Action[] actionsToDisable)
    {
        foreach (InputManager.Action action in actionsToDisable)
        {
            action.disabledCount++;
            if (action.isBeingPerformed)
            {
                if (action == Actions["Move"]) //TODO: use different flag for joystick actions
                {
                    action.stop?.Invoke(action);
                    if (!action.isPerformQueued)
                    {
                        action.queuePerform = QueuePerform(action);
                        StartCoroutine(action.queuePerform);
                    }
                }
            }
        }
        yield return new WaitForSeconds(duration);

        foreach (InputManager.Action action in actionsToDisable)
        {
            if (action.disabledCount > 0) action.disabledCount--;
        }
        yield break;
        //TODO: MOVING DIRECTION NOT FACING DIRECTION
    }

    public IEnumerator Disable(Func<bool> enableCondition, params Action[] actionsToDisable)
    {
        //yield return new WaitForFixedUpdate();
        foreach (InputManager.Action action in actionsToDisable)
        {
            action.disabledCount++;
            if (action.isBeingPerformed)
            {
                if (action == Actions["Move"])
                {
                    action.stop?.Invoke(action);
                    if (!action.isPerformQueued)
                    {
                        action.queuePerform = QueuePerform(action);
                        StartCoroutine(action.queuePerform);
                    }
                }
            }
        }
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        yield return new WaitUntil(enableCondition);

        foreach (InputManager.Action action in actionsToDisable)
        {
            if (action.disabledCount > 0) action.disabledCount--;
        }
        yield break;
    }

    public void EnableOneShot(params Action[] actionsToEnable)
    {
        foreach (InputManager.Action action in actionsToEnable)
        {
            StartCoroutine(OneShot(action));
        }

        IEnumerator OneShot(Action actionToEnable)
        {
            actionToEnable.disabledCount = 0;
            yield return new WaitUntil(() => actionToEnable.isBeingPerformed);
            Disable(actionToEnable);
        }
    }

    public IEnumerator DisableAll(float duration)
    {
        foreach (KeyValuePair<string, Action> pair in Actions)
        {
            Action action = pair.Value;
            action.disabledCount++;
            if (action.isBeingPerformed)
            {
                if (action == Actions["Move"]) //TODO: use different flag for joystick actions
                {
                    action.stop?.Invoke(action);
                    if (!action.isPerformQueued)
                    {
                        action.queuePerform = QueuePerform(action);
                        StartCoroutine(action.queuePerform);
                    }
                }
            }
        }
        yield return new WaitForSeconds(duration);

        foreach (KeyValuePair<string, Action> pair in Actions)
        {
            Action action = pair.Value;
            if (action.disabledCount > 0) action.disabledCount--;
        }
        yield break;
        //TODO: MOVING DIRECTION NOT FACING DIRECTION
    }

    public IEnumerator DisableAll(Func<bool> enableCondition, params Action[] actionsToDisable)
    {
        //yield return new WaitForFixedUpdate();
        foreach (KeyValuePair<string, Action> pair in Actions)
        {
            Action action = pair.Value;
            action.disabledCount++;
            if (action.isBeingPerformed)
            {
                if (action == Actions["Move"])
                {
                    action.stop?.Invoke(action);
                    if (!action.isPerformQueued)
                    {
                        action.queuePerform = QueuePerform(action);
                        StartCoroutine(action.queuePerform);
                    }
                }
            }
        }
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        yield return new WaitUntil(enableCondition);

        foreach (KeyValuePair<string, Action> pair in Actions)
        {
            Action action = pair.Value;
            if (action.disabledCount > 0) action.disabledCount--;
        }
        yield break;
    }

    private void EnableAll()
    {
        foreach (KeyValuePair<string, Action> pair in Actions)
        {
            pair.Value.disabledCount = 0;
        }
    }

    private IEnumerator QueuePerformTime(Action action, float duration)
    {
        action.isPerformTimeQueued = true;
        float timer = 0f;
        while (timer <= duration)
        {
            if (action.enableConditions.Count == 0 || action.enableConditions.TrueForAll(x => x()))
            {
                action.perform?.Invoke(action);
                action.isPerformTimeQueued = false;
                yield break;
            }
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        action.isPerformTimeQueued = false;
        yield break;
    }

    private IEnumerator QueuePerform(Action action)
    {
        if (action == Actions["Dash"]) yield break;
        if (Actions.ContainsKey("Dash Left"))
        {
            if (action == Actions["Dash Left"]) yield break;
        }
        if (Actions.ContainsKey("Dash Right"))
        {
            if (action == Actions["Dash Right"]) yield break;
        }
        
        action.isPerformQueued = true;
        // Debug.Log(action.disabledCount);
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        //yield return new WaitForFixedUpdate();
        yield return new WaitUntil(() => action.disabledCount == 0);
        // Debug.Log("Invoking queued");
        if (action.isBeingInput)
        {
            action.perform?.Invoke(action);
        }
        //action.perform?.Invoke(action);
        action.isPerformQueued = false;
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
        _isAwaitingStop = true;
        StartCoroutine(Disable(() => !_isAwaitingStop, Actions[action.name]));
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        if (action.isPerformQueued)
        {
            StopCoroutine(action.queuePerform);
            action.isPerformQueued = false;
        }
        action.stop?.Invoke(action);
        yield return new WaitForFixedUpdate();
        _isAwaitingStop = false;
        yield break;
    }
}
