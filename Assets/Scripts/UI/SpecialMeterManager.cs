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

    private void HandleIncrement(Fighter attacker, Fighter attacked, Vector3 hitPosition)
    {
        IncrementBar(attacker == _fighter
            ? attacker.BaseStateMachine.AttackInfo.incrementBarAmount
            : attacked.BaseStateMachine.AttackInfo.incrementBarAmount);
    }

    private void SubscribeToEvents()
    {
        _fighter.Events.onAttackHit += HandleIncrement;
        _fighter.OpposingFighter.Events.onBlockHit += HandleIncrement;
    }

    private void UnsubscribeFromEvents()
    {
        _fighter.Events.onAttackHit -= HandleIncrement;
        _fighter.OpposingFighter.Events.onBlockHit -= HandleIncrement;
    }
}
