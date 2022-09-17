using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundMoveState : State
{
    protected Animator animator;

    public float jumpSpeed = 8;

    private Rigidbody2D floorRB;

    public override void OnEnter(StateMachine _stateMachine)
    {
        base.OnEnter(_stateMachine);
        animator = GetComponent<Animator>();
        floorRB = GetComponent<Rigidbody2D>();


        animator.SetTrigger("Jump");
    }

    public override void OnUpdate()
    {


        base.OnUpdate();
/*        if (player.m_Grounded)
        {
            stateMachine.SetNextStateToMain();
            //player.jumping = false;
        }

        if (Input.GetButtonDown("LightAtk"))
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
        }
*/
    }
}
