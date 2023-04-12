using UnityEngine;

namespace Attacks
{
    /// <summary>
    /// Created in editor, and set in a projectile prefab (on spawn, on trigger enter, on invisible)
    /// </summary>
    [CreateAssetMenu(menuName = "Projectile/SFX")]
    public class PlaySFXAction: ProjectileAction
    {
        [SerializeField] private AK.Wwise.Event sfxEvent;
        
        public override void Execute(Projectile projectile)
        {
            //Fighter: projectile.Owner
            //play sound here
            projectile.Owner.GetComponent<PostWwiseEvent>().Wwise_PlaySingle(sfxEvent);
        }

        public override void Stop(Projectile projectile)
        {
            //(what happens if the projectile is destroyed, but this isn't implemented rn) does nothing
        }
    }
}