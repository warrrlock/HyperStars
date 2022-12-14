using UnityEngine;

public abstract class ProjectileAction: ScriptableObject
{
    public abstract void Execute(Projectile projectile);
    public abstract void Stop(Projectile projectile);
}