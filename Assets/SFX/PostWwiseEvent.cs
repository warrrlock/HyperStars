using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Rendering.LookDev;
using UnityEngine;

public enum Wwise_MovementEvents
{
    FOOTSTEPS, JUMP, DASH, LAND, CROUCH
}

public enum Wwise_SpecialSwings
{
    
}

public class PostWwiseEvent : MonoBehaviour
{
    private Fighter _fighter;
    [Header("Set to character")]
    public AK.Wwise.Switch CharacterSwitch;

    [Header("Specials")]
    public AK.Wwise.Event[] specialSwings;
    [Header("Movements")]
    public AK.Wwise.Event[] movements;

    private void Start()
    {
        // set character switch
        CharacterSwitch.SetValue(gameObject);
        // get fighter
        _fighter = GetComponent<Fighter>();
        // hits
        _fighter.Events.onAttackHit += Wwise_PlayHitSound;
        // blocks
        _fighter.Events.onBlockHit += ctx => Wwise_PlaySingle(SoundManagerCombat.Instance.blockHit);
        // environment hits
        _fighter.Events.wallBounce += () => Wwise_PlaySingle(SoundManagerCombat.Instance.environmentHits[(int)Wwise_CombatEnvironmentSounds.WallBounce]);
        _fighter.Events.onLandedHurt += () => Wwise_PlaySingle(SoundManagerCombat.Instance.environmentHits[(int)Wwise_CombatEnvironmentSounds.GroundSplat]);
        // land
        _fighter.Events.onLandedNeutral += () => Wwise_PlaySingle(movements[(int)Wwise_MovementEvents.LAND]);
    }
    
    private void Wwise_PlaySingle(AK.Wwise.Event e)
    {
        e.Post(gameObject);
    }
    
    /// <summary>
    /// Play swing sound in animation
    /// </summary>
    /// <param name="swings"></param>
    public void Wwise_PlayAttackSwing(Wwise_CombatSwings swings)
    {
        SoundManagerCombat.Instance.swingEvents[(int)swings].Post(gameObject);
    }

    /// <summary>
    /// Play special swing in animation
    /// </summary>
    /// <param name="swings"></param>
    public void Wwise_PlaySpecialSwing(Wwise_SpecialSwings swings)
    {
        specialSwings[(int)swings].Post(gameObject);
    }

    /// <summary>
    /// Play movement in animation
    /// </summary>
    /// <param name="mEvent"></param>
    public void Wwise_PlayMovementSound(Wwise_MovementEvents mEvent)
    {
        movements[(int)mEvent].Post(gameObject);
    }
    
    private void Wwise_PlayHitSound(Dictionary<string, object> message)
    {
        
        //Play Character Hit Sound
        Fighter receiver = (Fighter)message["attacked"];
        AttackInfo.AttackType type = (AttackInfo.AttackType)message["attack type"];
        switch (type)
        {
            case AttackInfo.AttackType.Light:
                SoundManagerCombat.Instance.hitEvents[(int)Wwise_CombatHits.Light].Post(receiver.gameObject);
                break;
            case AttackInfo.AttackType.Medium:
                SoundManagerCombat.Instance.hitEvents[(int)Wwise_CombatHits.Medium].Post(receiver.gameObject);
                break;
            case AttackInfo.AttackType.Heavy:
                SoundManagerCombat.Instance.hitEvents[(int)Wwise_CombatHits.Heavy].Post(receiver.gameObject);
                break;
        }
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
