using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeutralFinish : MeleeBaseState
{
    public override void OnEnter(StateMachine _stateMachine)
    {
        base.OnEnter(_stateMachine);

        //Attack
        attackIndex = 3;
        duration = NeutralDuration3;
        animator.SetTrigger("Attack" + attackIndex);
        Debug.Log("Player Attack " + attackIndex + " Fired!");
/*        knockBackX = NeutralKnockBack3.x;
        knockBackY = NeutralKnockBack3.y;*/
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (fixedtime >= duration)
        {
             stateMachine.SetNextStateToMain();
        }
    }
}
