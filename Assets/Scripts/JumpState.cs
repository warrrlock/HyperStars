using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpState : State
{
    protected Animator animator;

    public float jumpSpeed = 8;

    protected CharacterMovement player;

    public override void OnEnter(StateMachine _stateMachine)
    {
        base.OnEnter(_stateMachine);
        player = GetComponent<CharacterMovement>();
        animator = GetComponent<ComboCharacter>().animator;

        //player.jump = true;

        animator.SetTrigger("Jump");
        Debug.Log("in Jump State");
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
/*        if (player.onBase)
        {
            stateMachine.SetNextStateToMain();

            //player.jump = false;
        }*/

/*        if (Input.GetButtonDown("LightAtk"))
        {
            stateMachine.SetNextState(new AirLightState());
        }

        if (Input.GetButtonDown("MediumAtk"))
        {
            stateMachine.SetNextState(new AirMedState());
        }

        if (Input.GetButtonDown("HeavyAtk"))
        {
            stateMachine.SetNextState(new AirHeavyState());
        }*/

        if (Input.GetKeyDown(KeyCode.LeftShift)) 
        {
            stateMachine.SetNextState(new DashState());
        }
        stateMachine.SetNextStateToMain();

    }
}
