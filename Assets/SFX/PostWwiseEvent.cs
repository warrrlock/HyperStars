using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

public enum Wwise_ComboEvents
{
    COMBO1, COMBO2, COMBO3, COMBO4, COMBO5, SPECIAL_NEUTRAL, SPECIAL_SIDE, AIR_LIGHT, AIR_MEDIUM, AIR_SPECIAL,
    LOW_LIGHT, LOW_MEDIUM
}

public enum Wwise_MovementEvents
{
    FOOTSTEPS, JUMP, DASH, LAND, CROUCH
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
    public AK.Wwise.Event blockEvent;
    
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
        // block
        _fighter.Events.onBlockHit += Wwise_PlayBlock;
    }

    private void Update()
    {
        // Debug.Log(IsEventPlayingOnGameObject("Play_LISA_PARRY"));
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

    private void Wwise_PlayHit(Dictionary<string, object> message)
    {
        hitEvent.Post(gameObject);
    }
    
    private void Wwise_PlayBlock(Dictionary<string, object> message)
    {
        blockEvent.Post(gameObject);
    }
    
        
    static uint[] playingIds = new uint[50];
    public bool IsEventPlayingOnGameObject(string eventName)
    {
        uint testEventId = AkSoundEngine.GetIDFromString(eventName);

        uint count = (uint)playingIds.Length;
        AKRESULT result = AkSoundEngine.GetPlayingIDsFromGameObject(this.gameObject, ref count, playingIds);

        for (int i = 0; i < count; i++)
        {
            uint playingId = playingIds[i];
            uint eventId = AkSoundEngine.GetEventIDFromPlayingID(playingId);

            if (eventId == testEventId)
                return true;
        }

        return false;
    }
}
