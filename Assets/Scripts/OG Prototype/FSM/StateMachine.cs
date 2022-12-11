using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public string customName;
    public bool isCombat;

    private State mainStateType;
    public State CurrentState { get; private set; }
    private State nextState;

    // Update is called once per frame
    void Update()
    {
        if (nextState != null)
        {
            SetState(nextState);
        }

        if (CurrentState != null)
            CurrentState.OnUpdate();
    }

    private void SetState(State _newState)
    {
        nextState = null;
        if (CurrentState != null)
        {
            CurrentState.OnExit();
        }
        CurrentState = _newState;
        CurrentState.OnEnter(this);
    }

    public void SetNextState(State _newState)
    {
        if (_newState != null)
        {
            nextState = _newState;
        }
    }

    private void LateUpdate()
    {
        if (CurrentState != null)
            CurrentState.OnLateUpdate();
    }

    private void FixedUpdate()
    {
        if (CurrentState != null)
            CurrentState.OnFixedUpdate();
    }

    public void SetNextStateToMain()
    {
        nextState = new IdleState();
    }   

    private void Awake()
    {
        SetNextStateToMain();
        mainStateType = new IdleState();
    }

    private void OnValidate()
    {
        if (mainStateType == null)
        {
            if (isCombat)
            {
                mainStateType = new IdleState();
            }
        }
    }
}