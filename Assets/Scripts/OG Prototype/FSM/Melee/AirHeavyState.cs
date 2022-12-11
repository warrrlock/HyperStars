using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirHeavyState : MeleeBaseState
{
    public override void OnEnter(StateMachine _stateMachine)
    {
        base.OnEnter(_stateMachine);

        //Attack
        attackIndex = 3;
        duration = 0.2f;
        animator.SetTrigger("AirAttack" + attackIndex);
        playerRB.velocity = new Vector2(0, 0);
        //playerRB.gravityScale = 0;
        Debug.Log("Player Air Attack " + attackIndex + " Fired!");
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (fixedtime >= duration)
        {
            if (shouldCombo)
            {
                stateMachine.SetNextState(new AirHeavyState());
            }
            else
            {
                stateMachine.SetNextStateToMain();
                //playerRB.gravityScale = 15;
            }
        }
    }
}
