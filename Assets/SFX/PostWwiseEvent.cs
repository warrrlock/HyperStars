using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

public enum Wwise_ComboEvents
{
    COMBO1_INITIAL, COMBO1_TRANSITION, COMBO2, COMBO3, COMBO4, COMBO5
}

public enum Wwise_MovementEvents
{
    FOOTSTEPS, JUMP, DASH, LAND
}

public class PostWwiseEvent : MonoBehaviour
{
    private Fighter _fighter;
    public AK.Wwise.Event[] comboEvents;
    public AK.Wwise.Event comboVoicelineEvent;
    public AK.Wwise.Event[] movementEvents;
    public AK.Wwise.Event wallBounceEvent;

    private void Start()
    {
        _fighter = GetComponent<Fighter>();
        _fighter.Events.wallBounce += () => Wwise_PlaySingle(wallBounceEvent);
    }
    
    public void Wwise_PlayComboSound(Wwise_ComboEvents cEvent)
    {
        comboEvents[(int)cEvent].Post(gameObject);
    }

    public void Wwise_PlayComboVoiceline()
    {
        if (Random.value < .4f)
        {
            comboVoicelineEvent.Post(gameObject);
        }
    }

    public void Wwise_PlayMovementSound(Wwise_MovementEvents mEvent)
    {
        movementEvents[(int)mEvent].Post(gameObject);
    }

    private void Wwise_PlaySingle(AK.Wwise.Event e)
    {
        e.Post(gameObject);
    }
}
