using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : State
{
    private StateMachine meleeStateMachine;
    public override void OnEnter(StateMachine _stateMachine)
    {
        base.OnEnter(_stateMachine);
        
        meleeStateMachine = GetComponent<StateMachine>();
    }

    // Update is called once per frame
    public override void OnUpdate()
    {
        base.OnUpdate();

        if (Input.GetKeyDown(KeyCode.J) && meleeStateMachine.CurrentState.GetType() == typeof(IdleState))
        {
            meleeStateMachine.SetNextState(new NeutralJab());
        }

        if (Input.GetKeyDown(KeyCode.K) && meleeStateMachine.CurrentState.GetType() == typeof(IdleState))
        {
            meleeStateMachine.SetNextState(new NeutralFollow());
        }

        if (Input.GetKeyDown(KeyCode.L) && meleeStateMachine.CurrentState.GetType() == typeof(IdleState))
        {
            meleeStateMachine.SetNextState(new NeutralFinish());
        }

        if (Input.GetKeyDown(KeyCode.M) && meleeStateMachine.CurrentState.GetType() == typeof(IdleState))
        {
            meleeStateMachine.SetNextState(new SpecialNeutralState());
        }

/*        if (player.inputY < 0 && meleeStateMachine.CurrentState.GetType() == typeof(IdleState))
        {
            meleeStateMachine.SetNextState(new CrouchState());
        }*/
       
        if (Input.GetKeyDown(KeyCode.Space) && meleeStateMachine.CurrentState.GetType() == typeof(IdleState))
        {
            meleeStateMachine.SetNextState(new JumpState());
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && meleeStateMachine.CurrentState.GetType() == typeof(IdleState))
        {
            meleeStateMachine.SetNextState(new DashState());
        }

    }
}
