using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicEffector : MonoBehaviour
{
    private Fighter _fighter;
    private MusicManager _musicManager;

    void SFXAssignComponents() {
        _fighter = GetComponent<Fighter>();
        _musicManager = FindObjectOfType<MusicManager>();
    }
    
    void SFXSubscribeEvents() {
        _fighter.Events.onAttackHit += IncreaseHit;
    }
    
    void IncreaseHit(Dictionary<string, object> message)
    {
        try
        {
            switch (_fighter.PlayerId)
            {
                case 0:
                    _musicManager.hitCountP1++;
                    break;
                case 1:
                    _musicManager.hitCountP2++;
                    break;
            }
            _musicManager.thisHit = _fighter.PlayerId;
            _musicManager.CheckCombo();
            _musicManager.ResetHitEffectiveTimer();
        }
        catch (KeyNotFoundException)
        {
            Debug.Log("key was not found in dictionary.");
        }
    }

    void Start()
    {
        SFXAssignComponents();
        SFXSubscribeEvents();
    }
}