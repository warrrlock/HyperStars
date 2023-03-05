using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FiniteStateMachine;
using TMPro;
using UI;

public class FavorManager : MonoBehaviour
{
    //public float MaxFavor
    //{
    //    get => _maxFavor;
    //}
    [Tooltip("The maximum amount of favor a fighter can have.")]
    [SerializeField] private float _maxFavorInitial;
    [Tooltip("How much the favor multiplier increases when favor reverses.")]
    [SerializeField] private float _favorMultiplierDelta;
    //[Tooltip("The percentage of total favor that a fighter needs to win in order to increase the favor multiplier.")]
    //[SerializeField][Range(0f, 1f)] private float _favorSwitchPercentage;
    public float DecayValue
    {
        get => _decayValue;
    }
    [Tooltip("What percentage of an attack gets decayed after use.")]
    [SerializeField][Range(0f, 1f)] private float _decayValue;
    public float DecayResetDuration
    {
        get => _decayResetDuration;
    }
    [Tooltip("How long it takes for an attack to fully recover from decay.")]
    [SerializeField] private float _decayResetDuration;

    public float MaxFavor { get; private set; }

    /// <summary>
    /// If favor is < 0, player 1 is favored. If favor is > 0, player 0 is favored.
    /// </summary>
    private float _favor;
    private float _favorMultiplier = 1f;
    private int _favoredPlayer = -1;

    [SerializeField] private RectTransform _favorMeter;
    [SerializeField] private Image _favorMeterIndicator;
    [SerializeField] private Image _favorMeterIndicatorGlow;
    [SerializeField] private TextMeshProUGUI _multiplierText;
    [SerializeField] private Canvas _multiplierTextCanvas;
    [SerializeField] private GameEvent _winConditionEvent;


    private Canvas _canvas;
    [SerializeField] private Image _p1Bar;
    [SerializeField] private Image _p2Bar;
    [SerializeField] private RectTransform _p1Mask;
    [SerializeField] private RectTransform _p2Mask;
    [SerializeField] private RectTransform _outline;
    private float _initialWidth;

    private float _barMinimum = 0f;

    public float BarMaximum
    {
        get => _barMaximum;
    }
    private float _barMaximum;
    private float _outlineMaximum;
    private float _multiplier; //TODO: rename this

    private float _timeMultiplier;

    private bool _isRoundOver = false;

    private void Awake()
    {
        Services.FavorManager = this;
    }

    private void Start()
    {
        AssignComponents();
        MaxFavor = _maxFavorInitial;

        _initialWidth = _favorMeter.rect.width;

        _favor = 0f;

        _multiplier = _favorMeter.localScale.x / 2f;

        _barMaximum = _p1Mask.rect.width;
        _outlineMaximum = _outline.rect.width;
        UpdateFavorMeter();
    }

    private void AssignComponents()
    {
        _canvas = GetComponentInChildren<Canvas>();
    }

    public void StopIncrementing(Dictionary<string, object> data)
    {
        _isRoundOver = true;
    }

    public void IncreaseFavor(int playerId, float value)
    {
        if (_isRoundOver)
        {
            return;
        }
        value = playerId == 0 ? -value : value;
        value *= _favorMultiplier;
        if (Mathf.Abs(_favor) >= MaxFavor)
        {
            if (Mathf.Abs(_favor + value) > MaxFavor)
            {
                //player wins
                //Debug.Log("Player " + playerId + " wins.");
                Dictionary<string, object> result = new Dictionary<string, object>()
                {
                    {"winnerId", playerId}
                };
                if (_winConditionEvent) _winConditionEvent.Raise(result);
                // SceneReloader.Instance.ReloadScene();
            }
        }
        switch (_favoredPlayer)
        {
            case < 0:
                _favoredPlayer = playerId;
                break;
            case 0:
                if (_favor + value > _favor)
                {
                    //reverse favor
                    _favorMultiplier += _favorMultiplierDelta;
                    _favoredPlayer = playerId;
                }
                break;
            case 1:
                if (_favor + value < _favor)
                {
                    //reverse favor
                    _favorMultiplier += _favorMultiplierDelta;
                    _favoredPlayer = playerId;
                }
                break;
        }
        _favor += value;
        _favor = Mathf.Clamp(_favor, -MaxFavor, MaxFavor);
        if (Mathf.Abs(_favor) == MaxFavor)
        {
            //give golden goal
        }
        //if (_favor > _peakFavors[playerId])
        //{
        //    _peakFavors[playerId] = _favor;
        //}
        UpdateFavorMeter();
        //_favourMeter.UpdateFavorMeter(playerId == 0, Math.Abs(value)/_maxFavor, _favorMultiplier);
    }

    public IEnumerator CooldownFavor(ComboState attack, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            yield return new WaitForFixedUpdate();

            timer += Time.fixedDeltaTime;
        }
    }

    public void ResizeFavorMeter(float factor)
    {
        MaxFavor += factor;
        _favor = Mathf.Clamp(_favor, -MaxFavor, MaxFavor);
        UpdateFavorMeter();
    }

    private void UpdateFavorMeter()
    {
        _p1Mask.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (MaxFavor / _maxFavorInitial) * _barMaximum);
        _p2Mask.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (MaxFavor / _maxFavorInitial) * _barMaximum);
        _outline.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (MaxFavor / _maxFavorInitial) * _outlineMaximum);

        //_p1Bar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Lerp(_barMinimum, _barMaximum, Mathf.Abs(_favor - _maxFavorInitial) / (_maxFavorInitial * 2f)));
        //_p2Bar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Lerp(_barMinimum, _barMaximum, Mathf.Abs(_favor + _maxFavorInitial) / (_maxFavorInitial * 2f)));
        _p1Bar.fillAmount = Mathf.Lerp(0f, 1f, Mathf.Abs(_favor - _maxFavorInitial) / (_maxFavorInitial * 2f));
        _p2Bar.fillAmount = Mathf.Lerp(0f, 1f, Mathf.Abs(_favor + _maxFavorInitial) / (_maxFavorInitial * 2f));
        float indicatorX = Mathf.Lerp(-_initialWidth * _multiplier,
            _initialWidth * _multiplier, Mathf.Abs(_favor - _maxFavorInitial) / (_maxFavorInitial * 2f));
        indicatorX = Mathf.Clamp(indicatorX, -_initialWidth * _multiplier * (MaxFavor / _maxFavorInitial), _initialWidth * _multiplier * (MaxFavor / _maxFavorInitial));
        _favorMeterIndicatorGlow.rectTransform.anchoredPosition = new Vector3(indicatorX, _favorMeterIndicatorGlow.rectTransform.anchoredPosition.y, 0f);
        if (_favoredPlayer > -1)
        {
            _favorMeterIndicator.sprite = Services.Characters[_favoredPlayer].IndicatorSprite;
            _favorMeterIndicatorGlow.sprite = Services.Characters[_favoredPlayer].IndicatorGlowSprite;
        }
        else
        {
            _favorMeterIndicator.sprite = Services.Characters[0].IndicatorSprite;
            _favorMeterIndicatorGlow.sprite = Services.Characters[0].IndicatorGlowSprite;
        }

        if (_multiplierText)
        {
            _multiplierText.text = "x" + Math.Round(_favorMultiplier, 1);
        }
    }
}
