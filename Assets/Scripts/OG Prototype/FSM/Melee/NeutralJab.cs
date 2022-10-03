using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NeutralJab : MeleeBaseState
{
    public override void OnEnter(StateMachine _stateMachine)
    {
        base.OnEnter(_stateMachine);

        //Attack
        attackIndex = 1;
        duration = NeutralDuration1;
        animator.SetTrigger("Attack" + attackIndex);
        Debug.Log("Player Attack " + attackIndex + " Fired!");

    }

    public override void OnUpdate()
    {
        base.OnUpdate();


        if (fixedtime >= duration)
        {
            if (shouldCombo)
            { 
                stateMachine.SetNextState(new NeutralFollow());
            }
            else
            {
                stateMachine.SetNextStateToMain();
            }
        }
    }
}
