using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Fighter))]
public class SpecialMeterManager : MonoBehaviour
{
    [SerializeField] private Image _fillMeter;
    [SerializeField] private float _numberOfBars;
    private float _barAmount;
    private Fighter _fighter;
    
    public Image FillMeter {set => _fillMeter = value; }

    private void Awake()
    {
        _barAmount = 1 / _numberOfBars;
        _fighter = GetComponent<Fighter>();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    public void Initiate()
    {
        SubscribeToEvents();
    }

    public void IncrementBar(float bars)
    {
        float amount = _barAmount * bars;
        _fillMeter.fillAmount += amount;
    }

    public void DecrementBar(int bars)
    {
        float amount = _barAmount * bars;
        _fillMeter.fillAmount -= amount;
    }

    public bool CheckBar(int bars)
    {
        float amount = _barAmount * bars;
        // Debug.Log($"wanted {amount} and have filled {_fillMeter.fillAmount}");
        return _fillMeter.fillAmount >= amount;
    }

    private void SetupVisuals()
    {
        //create visuals to show how many bars meter has
    }

    private void HandleIncrement(Dictionary<string, object> message)
    {
        try
        {
            Fighter attacker = message["attacker"] as Fighter;
            Fighter attacked = message["attacked"] as Fighter;
            if (!attacker || !attacked) return;
            IncrementBar(attacker == _fighter
                ? attacker.BaseStateMachine.AttackInfo.incrementBarAmount
                : attacked.BaseStateMachine.AttackInfo.incrementBarAmount);
        }
        catch (KeyNotFoundException)
        {
            Debug.Log("keys attacker or attacked not found");
        }
    }

    private void SubscribeToEvents()
    {
        _fighter.Events.onAttackHit += HandleIncrement;
        _fighter.OpposingFighter.Events.onBlockHit += HandleIncrement;
    }

    private void UnsubscribeFromEvents()
    {
        if (!_fighter) return;
        _fighter.Events.onAttackHit -= HandleIncrement;
        _fighter.OpposingFighter.Events.onBlockHit -= HandleIncrement;
    }
}
