using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Fighter))]
public class SpecialMeterManager : MonoBehaviour
{
    public GameObject FillMeter {set => _fillMeter = value; }
    
    [SerializeField] private GameObject _meterBarPrefab;
    [SerializeField] private int _numberOfBars;
    [SerializeField] private float _passiveIncrementAmount;
    
    private GameObject _fillMeter;
    private Image[] _bars;
    private float _barAmount;
    private int _barIndex;
    private Fighter _fighter;

    private RoundManager _roundManager;
    private Coroutine _incrementRoutine;

    private void Awake()
    {
        _barAmount = 1.0f / _numberOfBars;
        _fighter = GetComponent<Fighter>();
        _roundManager = FindObjectOfType<RoundManager>();
    }

    public void Initialize()
    {
        SubscribeToEvents();
    }

    public void ResetValues()
    {
        StopAllCoroutines();
        SetupVisuals();
        StartPassiveIncrement();
    }

    public void IncrementBar(float bars)
    {
        float amount = bars;
        
        if (_barIndex < 0) _barIndex = 0;
        while (amount > 0)
        {
            if (_barIndex >= _bars.Length) return;
            float left = 1 - _bars[_barIndex].fillAmount;
            float toAdd = Math.Min(left, amount);
            
            amount -= toAdd;
            _bars[_barIndex].fillAmount += toAdd;
            if (_bars[_barIndex].fillAmount >= 1) _barIndex++;
        }
    }

    public void DecrementBar(int bars)
    {
        float amount = bars;
        if (_barIndex >= _bars.Length) _barIndex = _bars.Length-1;
        while (amount > 0)
        {
            if (_barIndex < 0) return;
            float left = _bars[_barIndex].fillAmount;
            float toSub = Math.Min(left, amount);
            
            amount -= toSub;
            _bars[_barIndex].fillAmount -= toSub;
            if (_bars[_barIndex].fillAmount <= 0) _barIndex--;
        }
    }

    public bool CheckBar(int bars)
    {
        float amount = _barAmount * bars;
        Debug.Log($"wanted {amount} and have filled {_barIndex} bars");
        
        return _barIndex >= bars;
    }

    private void SetupVisuals()
    {
        _barIndex = 0;
        _bars = new Image[_numberOfBars];
        for (int i = 0; i < _numberOfBars; i++)
        {
            _bars[i] = Instantiate(_meterBarPrefab, _fillMeter.transform).GetComponent<Image>();
            _bars[i].GetComponent<RectTransform>().localRotation = _fillMeter.transform.localRotation;
        }
    }

    private void HandleIncrement(Dictionary<string, object> message)
    {
        try
        {
            AttackInfo attackInfo = message["attack info"] as AttackInfo;

            if (attackInfo == null)
            {
                Debug.Log("needed information to [handle incrementing special bar] not found.");
                return;
            }
            IncrementBar(attackInfo.incrementBarAmount);
        }
        catch (KeyNotFoundException)
        {
            Debug.LogError($"{name}: attack info not found");
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
        if (_fighter.OpposingFighter != null) _fighter.OpposingFighter.Events.onBlockHit -= HandleIncrement;
    }

    private void StartPassiveIncrement()
    {
        if (_incrementRoutine != null) StopCoroutine(_incrementRoutine);
        _incrementRoutine = StartCoroutine(PassivelyIncrementBy(_passiveIncrementAmount));
    }
    
    private IEnumerator PassivelyIncrementBy(float amount)
    {
        while ((_roundManager && _roundManager.InGame) || amount > 0)
        {
            if(_barIndex < _bars.Length) IncrementBar(amount * Time.deltaTime);
            yield return null;
        }
    }
}
