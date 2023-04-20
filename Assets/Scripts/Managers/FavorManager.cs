using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FiniteStateMachine;
using TMPro;
using UI;
using WesleyDavies;

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
    public float FavorDecayValue
    {
        get => _favorDecayValue;
    }
    [Tooltip("What percentage of an attack gets decayed after use.")]
    [SerializeField][Range(0f, 1f)] private float _favorDecayValue;
    public float HitstunDecayValue
    {
        get => _hitstunDecayValue;
    }
    [Tooltip("What percentage of an attack's hitstun gets decayed after use.")]
    [SerializeField][Range(0f, 1f)] private float _hitstunDecayValue;
    public float KnockbackDecayValue
    {
        get => _knockbackDecayValue;
    }
    [Tooltip("What percentage of an attack's knockback gets decayed after use.")]
    [SerializeField][Range(1f, 2f)] private float _knockbackDecayValue;
    //hitstun, favor, knockback
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
    [Tooltip("Index 0 is p1, index 1 is p2.")]
    [SerializeField] private Color[] _glowColors = new Color[2];

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

    [Header("Chip Effect")]
    [SerializeField] private float _chipDuration;
    [SerializeField] private RectTransform _p1ChipShrinkMask;
    [SerializeField] private RectTransform _p2ChipShrinkMask;
    [SerializeField] private Image _p1ChipMask;
    [SerializeField] private Image _p1ChipRect;
    [SerializeField] private Image _p2ChipMask;
    [SerializeField] private Image _p2ChipRect;
    private IEnumerator _p1ChipEffect;
    private IEnumerator _p2ChipEffect;
    private bool _isP1Chipping = false;
    private bool _isP2Chipping = false;

    [SerializeField] private float _indicatorFlipDuration;
    private float _indicatorFlipSpeed;
    private float _indicatorScaleDefault;
    private bool _isIndicatorFlipping = false;
    private IEnumerator _indicatorFlip;

    private float _maxIndicatorX;
    private float _minIndicatorX;
    private float _maxChipX;
    private float _minChipX;
    private float _p1ChipFill = 0f;
    private float _p2ChipFill = 0f;
    private float _indicatorWidthOffsetLeft;
    private float _indicatorWidthOffsetRight;
    private float _p1ChipFillDestination;
    private float _p2ChipFillDestination;

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

        _minIndicatorX = -_initialWidth * _multiplier;
        _maxIndicatorX = _initialWidth * _multiplier;
        _minChipX = -_initialWidth / 2f;
        _maxChipX = _initialWidth / 2f;
        _indicatorWidthOffsetLeft = _favorMeterIndicatorGlow.rectTransform.rect.width / 2f - 2f;
        _indicatorWidthOffsetRight = _favorMeterIndicatorGlow.rectTransform.rect.width / 2f - 7f;
        _indicatorScaleDefault = _favorMeterIndicatorGlow.rectTransform.localScale.x;
        _indicatorFlipSpeed = _indicatorScaleDefault * 2f / _indicatorFlipDuration;
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
            }
        }
        switch (_favoredPlayer)
        {
            case < 0:
                _favoredPlayer = playerId;
                _indicatorFlip = FlipIndicator(_favoredPlayer);
                StartCoroutine(_indicatorFlip);
                break;
            case 0:
                if (_favor + value > _favor)
                {
                    //reverse favor
                    _favorMultiplier += _favorMultiplierDelta;
                    _favoredPlayer = playerId;
                    if (_isIndicatorFlipping)
                    {
                        StopCoroutine(_indicatorFlip);
                        _isIndicatorFlipping = false;
                    }
                    _indicatorFlip = FlipIndicator(_favoredPlayer);
                    StartCoroutine(_indicatorFlip);
                }
                break;
            case 1:
                if (_favor + value < _favor)
                {
                    //reverse favor
                    _favorMultiplier += _favorMultiplierDelta;
                    _favoredPlayer = playerId;
                    if (_isIndicatorFlipping)
                    {
                        StopCoroutine(_indicatorFlip);
                        _isIndicatorFlipping = false;
                    }
                    _indicatorFlip = FlipIndicator(_favoredPlayer);
                    StartCoroutine(_indicatorFlip);
                }
                break;
        }
        _favor += value;
        _favor = Mathf.Clamp(_favor, -MaxFavor, MaxFavor);
        //if (_favor > _peakFavors[playerId])
        //{
        //    _peakFavors[playerId] = _favor;
        //}
        UpdateFavorMeter(true);
    }

    public void ResizeFavorMeter(float factor)
    {
        MaxFavor += factor;
        _favor = Mathf.Clamp(_favor, -MaxFavor, MaxFavor);
        UpdateFavorMeter();
    }

    private void UpdateFavorMeter(bool shouldChip = false)
    {
        _p1Mask.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (MaxFavor / _maxFavorInitial) * _barMaximum);
        _p2Mask.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (MaxFavor / _maxFavorInitial) * _barMaximum);
        _outline.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (MaxFavor / _maxFavorInitial) * _outlineMaximum);
        _p1ChipShrinkMask.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (MaxFavor / _maxFavorInitial) * _outlineMaximum);
        _p2ChipShrinkMask.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (MaxFavor / _maxFavorInitial) * _outlineMaximum);

        //_p1Bar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Lerp(_barMinimum, _barMaximum, Mathf.Abs(_favor - _maxFavorInitial) / (_maxFavorInitial * 2f)));
        //_p2Bar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Lerp(_barMinimum, _barMaximum, Mathf.Abs(_favor + _maxFavorInitial) / (_maxFavorInitial * 2f)));
        _p1Bar.fillAmount = Mathf.Lerp(0f, 1f, Mathf.Abs(_favor - _maxFavorInitial) / (_maxFavorInitial * 2f));
        _p2Bar.fillAmount = Mathf.Lerp(0f, 1f, Mathf.Abs(_favor + _maxFavorInitial) / (_maxFavorInitial * 2f));

        float previousIndicatorX = 0f;
        if (shouldChip)
        {
            previousIndicatorX = _favorMeterIndicatorGlow.rectTransform.anchoredPosition.x;
        }

        float indicatorX = Mathf.Lerp(_minIndicatorX, _maxIndicatorX, Mathf.Abs(_favor - _maxFavorInitial) / (_maxFavorInitial * 2f));
        indicatorX = Mathf.Clamp(indicatorX, _minIndicatorX * (MaxFavor / _maxFavorInitial), _maxIndicatorX * (MaxFavor / _maxFavorInitial));
        _favorMeterIndicatorGlow.rectTransform.anchoredPosition = new Vector3(indicatorX, _favorMeterIndicatorGlow.rectTransform.anchoredPosition.y, 0f);
        //if (_favoredPlayer > -1)
        //{
        //    _favorMeterIndicator.sprite = Services.Characters[_favoredPlayer].IndicatorSprite;
        //    _favorMeterIndicatorGlow.sprite = Services.Characters[_favoredPlayer].IndicatorGlowSprite;
        //    _favorMeterIndicatorGlow.color = _glowColors[_favoredPlayer];
        //}
        //else
        //{
        //    _favorMeterIndicator.sprite = Services.Characters[0].IndicatorSprite;
        //    _favorMeterIndicatorGlow.sprite = Services.Characters[0].IndicatorGlowSprite;
        //    _favorMeterIndicatorGlow.color = _glowColors[0];
        //}

        if (_favoredPlayer < 0)
        {
            _favorMeterIndicator.sprite = Services.Characters[0].IndicatorSprite;
            _favorMeterIndicatorGlow.sprite = Services.Characters[0].IndicatorGlowSprite;
            _favorMeterIndicatorGlow.color = _glowColors[0];
        }

        if (shouldChip)
        {
            //float chipX = Mathw.Average(previousIndicatorX, indicatorX);
            //chipX -= _favorMeterIndicatorGlow.rectTransform.rect.width / 1.5f;
            //chipX = ConvertFromWorldRectToLocal(chipX);
            //Debug.Log(chipX);
            //Image chipRect = _favoredPlayer == 0 ? _p1ChipRect : _p2ChipRect;
            //Image chipMask = _favoredPlayer == 0 ? _p1ChipMask : _p2ChipMask;
            //Vector2 chipPosition = chipRect.rectTransform.InverseTransformPoint(_favorMeterIndicatorGlow.rectTransform.TransformPoint(new Vector2(chipX, 0f)));
            //float chipWidth = Mathf.Abs(indicatorX - previousIndicatorX); //TODO: /2 is most accurate, but looks too small
            //_chipEffect = ChipEffect(chipX, chipWidth);
            Side originSide;
            float previousFill;
            float previousIndicatorXOffset;
            float indicatorXOffset;
            switch (_favoredPlayer)
            {
                case 0:
                    originSide = Side.Right;
                    previousFill = _p1ChipFill;
                    previousIndicatorXOffset = previousIndicatorX - _indicatorWidthOffsetLeft;
                    indicatorXOffset = indicatorX - _indicatorWidthOffsetLeft;
                    _p1ChipMask.fillAmount = GetXPercent(indicatorX, Side.Left);
                    break;
                case 1:
                    originSide = Side.Left;
                    previousFill = _p2ChipFill;
                    previousIndicatorXOffset = previousIndicatorX + _indicatorWidthOffsetRight;
                    indicatorXOffset = indicatorX + _indicatorWidthOffsetRight;
                    _p2ChipMask.fillAmount = GetXPercent(indicatorX, Side.Right);
                    break;
                default:
                    throw new Exception("Favored player is neither 0 nor 1.");
            }
            float previousXPercent = GetXPercent(previousIndicatorXOffset, originSide);
            float startXPercent = previousXPercent > previousFill ? previousXPercent : previousFill;
            float newXPercent = GetXPercent(indicatorXOffset, originSide);
            switch (_favoredPlayer)
            {
                case 0:
                    if (_isP1Chipping)
                    {
                        StopCoroutine(_p1ChipEffect);
                    }
                    _p2ChipMask.fillAmount = GetXPercent(indicatorX + _indicatorWidthOffsetRight, Side.Right);
                    if (_isP2Chipping)
                    {
                        StopCoroutine(_p2ChipEffect);
                        FastForwardChip(1, _p2ChipFillDestination);
                    }
                    _p1ChipFillDestination = newXPercent;
                    _p1ChipEffect = ChipEffect(startXPercent, _p1ChipFillDestination);
                    StartCoroutine(_p1ChipEffect);
                    break;
                case 1:
                    if (_isP2Chipping)
                    {
                        StopCoroutine(_p2ChipEffect);
                    }
                    _p1ChipMask.fillAmount = GetXPercent(indicatorX - _indicatorWidthOffsetLeft, Side.Left);
                    if (_isP1Chipping)
                    {
                        StopCoroutine(_p1ChipEffect);
                        FastForwardChip(0, _p1ChipFillDestination);
                    }
                    _p2ChipFillDestination = newXPercent;
                    _p2ChipEffect = ChipEffect(startXPercent, _p2ChipFillDestination);
                    StartCoroutine(_p2ChipEffect);
                    break;
            }
        }

        //if (_multiplierText)
        //{
        //    _multiplierText.text = "x" + Math.Round(_favorMultiplier, 1);
        //}
    }

    private float GetXPercent(float worldX, Side originSide)
    {
        float startPercent = originSide == Side.Left ? 0f : 1f;
        float destinationPercent = originSide == Side.Left ? 1f : 0f;
        float xPercent = Mathf.Lerp(startPercent, destinationPercent, (worldX - _minIndicatorX) / (_maxIndicatorX - _minIndicatorX)); //TODO: maybe mathf.abs(min) looks better than -min?
        return xPercent;
    }

    private float ConvertFromWorldRectToLocal(float worldX)
    {
        float xPercent = Mathf.Lerp(0f, 1f, (worldX - _minIndicatorX) / (_maxIndicatorX - _minIndicatorX)); //TODO: maybe mathf.abs(min) looks better than -min?
        return Mathf.Lerp(_minChipX, _maxChipX, xPercent);
    }

    private IEnumerator FlipIndicator(int newPlayerId)
    {
        _isIndicatorFlipping = true;
        int playerIdMultiplier = newPlayerId == 0 ? 1 : -1;
        float indicatorScaleCurrent = _favorMeterIndicatorGlow.rectTransform.localScale.x;
        bool hasIconChanged = false;
        while (indicatorScaleCurrent * playerIdMultiplier < _indicatorScaleDefault)
        {
            yield return new WaitForFixedUpdate();
            Vector3 newScale = _favorMeterIndicatorGlow.rectTransform.localScale;
            indicatorScaleCurrent += _indicatorFlipSpeed * playerIdMultiplier * Time.fixedDeltaTime;
            newScale.x = indicatorScaleCurrent;
            _favorMeterIndicatorGlow.rectTransform.localScale = newScale;
            if (hasIconChanged)
            {
                continue;
            }
            if (indicatorScaleCurrent * playerIdMultiplier > 0f)
            {
                _favorMeterIndicator.sprite = Services.Characters[newPlayerId].IndicatorSprite;
                _favorMeterIndicatorGlow.sprite = Services.Characters[newPlayerId].IndicatorGlowSprite;
                _favorMeterIndicatorGlow.color = _glowColors[newPlayerId];
                hasIconChanged = true;
            }
        }
        Vector3 endScale = _favorMeterIndicatorGlow.rectTransform.localScale;
        endScale.x = _indicatorScaleDefault * playerIdMultiplier;
        _favorMeterIndicatorGlow.rectTransform.localScale = endScale;
        _isIndicatorFlipping = false;
        yield break;
    }

    private IEnumerator ChipEffect(float startFill, float destinationFill)
    {
        switch (_favoredPlayer)
        {
            case 0:
                _isP1Chipping = true;
                break;
            case 1:
                _isP2Chipping = true;
                break;
        }
        Easing function = Easing.CreateEasingFunc(Easing.Funcs.CubicOut);
        Image chipRect = _favoredPlayer == 0 ? _p1ChipRect : _p2ChipRect;
        float timer = 0f;
        while (timer <= _chipDuration)
        {
            yield return new WaitForFixedUpdate();
            timer += Time.fixedDeltaTime;
            chipRect.fillAmount = function.Ease(startFill, destinationFill, timer / _chipDuration);
            switch (_favoredPlayer)
            {
                case 0:
                    _p1ChipFill = chipRect.fillAmount;
                    break;
                case 1:
                    _p2ChipFill = chipRect.fillAmount;
                    break;
                default:
                    throw new Exception("Favored player is neither 0 nor 1.");
            }
        }
        chipRect.fillAmount = destinationFill;
        switch (_favoredPlayer)
        {
            case 0:
                _p1ChipFill = chipRect.fillAmount;
                break;
            case 1:
                _p2ChipFill = chipRect.fillAmount;
                break;
            default:
                throw new Exception("Favored player is neither 0 nor 1.");
        }
        switch (_favoredPlayer)
        {
            case 0:
                _isP1Chipping = false;
                break;
            case 1:
                _isP2Chipping = false;
                break;
        }
        yield break;
    }

    private void FastForwardChip(int chipPlayer, float destinationFill)
    {
        switch (_favoredPlayer)
        {
            case 0:
                _p1ChipFill = destinationFill;
                break;
            case 1:
                _p2ChipFill = destinationFill;
                break;
        }
    }

    private IEnumerator FlipIndicator()
    {
        yield break;
    }
}
