using UnityEngine;

public abstract class ProjectileAction: ScriptableObject
{
    [Tooltip("what this action should do when executed.")]
    public abstract void Execute(Projectile projectile);
    [Tooltip("what the action does when the projectile is stopped/destroyed. Not implemented yet.")]
    public abstract void Stop(Projectile projectile);
}