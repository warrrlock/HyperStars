using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Projectile/Destroy")]
public class DestroyProjectileAction : ProjectileAction
{
    public override void Execute(Projectile projectile)
    {
        projectile.enabled = false;
        projectile.GetComponent<Renderer>().enabled = false;
        
        // AttackInfo info = projectile.AttackInfo;
        // float time = Max(new float[]{info.hangTime, info.knockbackDuration, info.hitStopDuration, info.hitStunDuration});
        // projectile.DestroyIn(time+0.2f);
        projectile.DestroyIn(0.0f);
    }

    float Max(float[] arr)
    {
        float max = Single.NegativeInfinity;
        foreach (float f in arr)
            max = Math.Max(max, f);
        return max;
    }

    public override void Stop(Projectile projectile)
    {
        //do nothing
    }
}
