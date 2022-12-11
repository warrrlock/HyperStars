using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashState : MeleeBaseState
{
    public override void OnEnter(StateMachine _stateMachine)
    {
        base.OnEnter(_stateMachine);

        duration = 0.3f;
        charCtrl.dash = true;
        //player.attacking = false;

        

        animator.SetBool("Dash", true);
        //player.AE_DashDust();

        
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (fixedtime >= duration)
        {
            //player.dash = false;
            animator.SetBool("Dash", false);
/*          player.pushBox.enabled = true;
            player.floorBox.enabled = true;*/
            stateMachine.SetNextStateToMain();
        }
    }
}
