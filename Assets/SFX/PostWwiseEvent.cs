using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

public enum Wwise_ComboEvents
{
    COMBO1, COMBO2, COMBO3, COMBO4, COMBO5, SPECIAL_NEUTRAL, SPECIAL_SIDE
}

public enum Wwise_MovementEvents
{
    FOOTSTEPS, JUMP, DASH, LAND
}

public class PostWwiseEvent : MonoBehaviour
{
    private Fighter _fighter;
    public AK.Wwise.Event[] attackEvents;
    public AK.Wwise.Event[] movementEvents;
    [Header("Hits")]
    public AK.Wwise.Event wallBounceEvent;
    public AK.Wwise.Event hurtLandEvent;
    public AK.Wwise.Event hitEvent;
    
    // public AK.Wwise.Event comboVoicelineEvent;

    private void Start()
    {
        _fighter = GetComponent<Fighter>();
        // environment hits
        _fighter.Events.wallBounce += () => Wwise_PlaySingle(wallBounceEvent);
        _fighter.Events.onLandedHurt += () => Wwise_PlaySingle(hurtLandEvent);
        // land
        _fighter.Events.onLandedNeutral += () => Wwise_PlaySingle(movementEvents[(int)Wwise_MovementEvents.LAND]);
        // hit
        _fighter.Events.onAttackHit += Wwise_PlayHit;
    }
    
    public void Wwise_PlayAttackSound(Wwise_ComboEvents cEvent)
    {
        attackEvents[(int)cEvent].Post(gameObject);
    }

    // public void Wwise_PlayComboVoiceline()
    // {
    //     if (Random.value < .4f)
    //     {
    //         comboVoicelineEvent.Post(gameObject);
    //     }
    // }

    public void Wwise_PlayMovementSound(Wwise_MovementEvents mEvent)
    {
        movementEvents[(int)mEvent].Post(gameObject);
    }

    private void Wwise_PlaySingle(AK.Wwise.Event e)
    {
        e.Post(gameObject);
    }

    private void Wwise_PlayHit(Fighter sender, Fighter receiver, Vector3 hitPos)
    {
        hitEvent.Post(gameObject);
    }
}
