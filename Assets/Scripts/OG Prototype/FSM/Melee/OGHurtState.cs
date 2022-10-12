using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class OGHurtState : MeleeBaseState
{
    public override void OnEnter(StateMachine _stateMachine)
    {
        base.OnEnter(_stateMachine);

        //Attack
        //attackIndex = 1;
        duration = 0.3f;
        animator.SetBool("Hurt", true);
        Debug.Log("Hurt");

    }

    public override void OnUpdate()
    {
        base.OnUpdate();


        if (fixedtime >= duration)
        {
            {
                animator.SetBool("Hurt", false);
                stateMachine.SetNextStateToMain();
            }
        }
    }
}
