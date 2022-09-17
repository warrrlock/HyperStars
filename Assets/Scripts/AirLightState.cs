using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirLightState : MeleeBaseState
{

    public override void OnEnter(StateMachine _stateMachine)
    {
        base.OnEnter(_stateMachine);

        //Attack
        attackIndex = 1;
        duration = 0.2f;
        animator.SetTrigger("AirAttack" + attackIndex);
        //player.controller.velocity = new Vector2(0, 0);
        //player.controller.gravityScale = 0;
        Debug.Log("Player Air Attack " + attackIndex + " Fired!");
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (fixedtime >= duration)
        {
            if (shouldCombo)
            {
                stateMachine.SetNextState(new AirLightState());
            }
            else
            {
                //stateMachine.SetNextStateToMain();
                //playerRB.gravityScale = 15;
            }
        }

        if (player.onBase)
        {
            stateMachine.SetNextStateToMain();
            //player.jumping = false;
        }
    }
}
