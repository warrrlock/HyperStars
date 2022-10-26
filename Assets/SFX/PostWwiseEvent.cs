using System.Collections;
using System.Collections.Generic;
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
    public AK.Wwise.Event[] comboEvents;
    public AK.Wwise.Event comboVoicelineEvent;
    public AK.Wwise.Event[] movementEvents;

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
}
