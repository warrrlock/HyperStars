using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeutralFollow : MeleeBaseState
{
    public override void OnEnter(StateMachine _stateMachine)
    {
        base.OnEnter(_stateMachine);

        //Attack
        attackIndex = 2;
        duration = NeutralDuration2;
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
                stateMachine.SetNextState(new NeutralFinish());
            }
            else
            {
                stateMachine.SetNextStateToMain();
            }
        }
    }
}
