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

    public float MaxFavor { get; private set; }

    /// <summary> If favor is < 0, player 1 is favored. If favor is > 0, player 0 is favored. </summary>
    private float _favor;
    private float _favorMultiplier = 1f;
    private int _favoredPlayer = -1;

    [SerializeField] private RectTransform _favorMeter;
    [SerializeField] private Image _favorMeterIndicator;
    //[SerializeField] private Image _favorMeterIndicatorGlow;
    [SerializeField] private Material _favorMeterIndicatorOutlineMaterial; // TODO: outline material
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
    [SerializeField] public Color[] _glowColors = new Color[2];

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
    [SerializeField] private float _flipMaxSizeY;
    private Vector2 _indicatorFlipSpeed = new();
    private Vector2 _indicatorScaleDefault;
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

    [Tooltip("x is min. y is max.")]
    [SerializeField] private Vector2 _portraitScaleBounds;
    [SerializeField] private float _portraitEnlargeDuration;
    [SerializeField] private float _portraitShrinkDuration;
    [SerializeField] private Image[] _portraitOutlines;
    private float[] _portraitScales = new float[2];
    private bool _isPortraitScaling = false;
    private IEnumerator _portraitResize;

    public delegate void GoldenGoal(int player);
    public GoldenGoal onGoldenGoalEnabled;
    public delegate void GoldenGoalDisable(int player);
    public GoldenGoalDisable onGoldenGoalDisabled;

    private void Awake()
    {
        Services.FavorManager = this;
    }

    private void Start()
    {
        AssignComponents();
        SubscribeEvents();
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
        _indicatorWidthOffsetLeft = _favorMeterIndicator.rectTransform.rect.width / 2f - 2f;
        _indicatorWidthOffsetRight = _favorMeterIndicator.rectTransform.rect.width / 2f - 7f;
        _indicatorScaleDefault = _favorMeterIndicator.rectTransform.localScale;
        _indicatorFlipSpeed.x = _indicatorScaleDefault.x * 2f / _indicatorFlipDuration;
        _indicatorFlipSpeed.y = (_flipMaxSizeY - _indicatorScaleDefault.y) * 2f / _indicatorFlipDuration;

        for (int i = 0; i < 2; i++)
        {
            _portraitScales[i] = Mathf.Lerp(_portraitScaleBounds.x, _portraitScaleBounds.y, 0.5f);
            _portraitOutlines[i].rectTransform.localScale = new Vector3(_portraitScales[i], _portraitScales[i], _portraitScales[i]);
            _portraitOutlines[i].color = _glowColors[i];
        }
        UpdateFavorMeter();
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    private void AssignComponents()
    {
        _canvas = GetComponentInChildren<Canvas>();
    }

    public void ResetFavorMeter()
    {
        Debug.Log("resetting favor meter");
        _favor = 0;
        //also reset the chip effect
        if (_p1ChipEffect != null)
        {
            StopCoroutine(_p1ChipEffect);
            _p1ChipRect.fillAmount = 0;
            _p1ChipFill = 0;
        }
        if (_p2ChipEffect != null)
        {
            StopCoroutine(_p2ChipEffect);
            _p2ChipRect.fillAmount = 0;
            _p2ChipFill = 0;
        }
        UpdateFavorMeter();
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
                if (Services.Fighters[_favoredPlayer].HasGoldenGoal)
                {
                    //TODO: dont let a player win who isn't golden because the favor meter shrunk on the other person being favored
                    //player wins
                    //Debug.Log("Player " + playerId + " wins.");
                    Dictionary<string, object> result = new Dictionary<string, object>()
                {
                    {"winnerId", playerId}
                };
                    if (_winConditionEvent) _winConditionEvent.Raise(result);
                }
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

        if (_isPortraitScaling)
        {
            StopCoroutine(_portraitResize);
            _isPortraitScaling = false;
        }
        _portraitResize = ResizeCharacterPortraits(_favoredPlayer, _favor, _favor + value);
        StartCoroutine(_portraitResize);

        _portraitResize = ResizeCharacterPortraits(_favoredPlayer, _favor, _favor + value);
        StartCoroutine(_portraitResize);
        _favor += value;
        _favor = Mathf.Clamp(_favor, -MaxFavor, MaxFavor);

        UpdateFavorMeter(true);

        //golden goal
        switch (_favoredPlayer)
        {
            case 0:
                if (_favor <= -MaxFavor)
                {
                    if (!Services.Fighters[0].HasGoldenGoal)
                    {
                        onGoldenGoalEnabled?.Invoke(0);
                    }
                }
                if (Services.Fighters[1].HasGoldenGoal)
                {
                    onGoldenGoalDisabled?.Invoke(1);
                }
                break;
            case 1:
                if (_favor >= MaxFavor)
                {
                    if (!Services.Fighters[1].HasGoldenGoal)
                    {
                        onGoldenGoalEnabled?.Invoke(1);
                    }
                }
                if (Services.Fighters[0].HasGoldenGoal)
                {
                    onGoldenGoalDisabled?.Invoke(0);
                }
                break;
        }
        //if (_favor > _peakFavors[playerId])
        //{
        //    _peakFavors[playerId] = _favor;
        //}
    }

    public void ResizeFavorMeter(float factor)
    {
        MaxFavor += factor;
        _favor = Mathf.Clamp(_favor, -MaxFavor, MaxFavor);
        switch (_favoredPlayer)
        {
            case 0:
                if (_favor <= -MaxFavor)
                {
                    if (!Services.Fighters[_favoredPlayer].HasGoldenGoal)
                    {
                        onGoldenGoalEnabled?.Invoke(_favoredPlayer);
                    }
                }
                break;
            case 1:
                if (_favor >= MaxFavor)
                {
                    if (!Services.Fighters[_favoredPlayer].HasGoldenGoal)
                    {
                        onGoldenGoalEnabled?.Invoke(_favoredPlayer);
                    }
                }
                break;
        }
        UpdateFavorMeter();
    }

    private void UpdateFavorMeter(bool shouldChip = false)
    {
        _p1Mask.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (MaxFavor / _maxFavorInitial) * _barMaximum);
        _p2Mask.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (MaxFavor / _maxFavorInitial) * _barMaximum);
        _outline.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (MaxFavor / _maxFavorInitial) * _outlineMaximum);
        _p1ChipShrinkMask.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (MaxFavor / _maxFavorInitial) * _outlineMaximum);
        _p2ChipShrinkMask.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (MaxFavor / _maxFavorInitial) * _outlineMaximum);

        _p1Bar.fillAmount = Mathf.Lerp(0f, 1f, Mathf.Abs(_favor - _maxFavorInitial) / (_maxFavorInitial * 2f));
        _p2Bar.fillAmount = Mathf.Lerp(0f, 1f, Mathf.Abs(_favor + _maxFavorInitial) / (_maxFavorInitial * 2f));

        float previousIndicatorX = 0f;
        if (shouldChip)
        {
            previousIndicatorX = _favorMeterIndicator.rectTransform.anchoredPosition.x;
        }

        float indicatorX = Mathf.Lerp(_minIndicatorX, _maxIndicatorX, Mathf.Abs(_favor - _maxFavorInitial) / (_maxFavorInitial * 2f));
        indicatorX = Mathf.Clamp(indicatorX, _minIndicatorX * (MaxFavor / _maxFavorInitial), _maxIndicatorX * (MaxFavor / _maxFavorInitial));
        _favorMeterIndicator.rectTransform.anchoredPosition = new Vector3(indicatorX, _favorMeterIndicator.rectTransform.anchoredPosition.y, 0f);

        if (_favoredPlayer < 0)
        {
            _favorMeterIndicator.sprite = Services.Characters[0].IndicatorSprite;
            //_favorMeterIndicatorGlow.sprite = Services.Characters[0].IndicatorGlowSprite;
            //_favorMeterIndicatorGlow.color = _glowColors[0];
            _favorMeterIndicatorOutlineMaterial.SetColor("_OutlineColor", _glowColors[0]);
        }

        if (shouldChip)
        {
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

    //private float ConvertFromWorldRectToLocal(float worldX)
    //{
    //    float xPercent = Mathf.Lerp(0f, 1f, (worldX - _minIndicatorX) / (_maxIndicatorX - _minIndicatorX)); //TODO: maybe mathf.abs(min) looks better than -min?
    //    return Mathf.Lerp(_minChipX, _maxChipX, xPercent);
    //}

    private IEnumerator FlipIndicator(int newPlayerId)
    {
        _isIndicatorFlipping = true;
        int playerIdMultiplier = newPlayerId == 0 ? 1 : -1;
        Vector2 indicatorScaleCurrent = _favorMeterIndicator.rectTransform.localScale;
        bool hasIconChanged = false;
        while (indicatorScaleCurrent.x * playerIdMultiplier < _indicatorScaleDefault.x)
        {
            yield return new WaitForFixedUpdate();
            Vector3 newScale = _favorMeterIndicator.rectTransform.localScale;
            indicatorScaleCurrent.x += _indicatorFlipSpeed.x * playerIdMultiplier * Time.fixedDeltaTime;
            if (!hasIconChanged)
            {
                indicatorScaleCurrent.y += _indicatorFlipSpeed.y * Time.fixedDeltaTime;
            }
            else
            {
                indicatorScaleCurrent.y -= _indicatorFlipSpeed.y * Time.fixedDeltaTime;
            }
            newScale = indicatorScaleCurrent;
            _favorMeterIndicator.rectTransform.localScale = newScale;
            if (hasIconChanged)
            {
                continue;
            }
            if (indicatorScaleCurrent.x * playerIdMultiplier > 0f)
            {
                _favorMeterIndicator.sprite = Services.Characters[newPlayerId].IndicatorSprite;
                //_favorMeterIndicatorGlow.sprite = Services.Characters[newPlayerId].IndicatorGlowSprite;
                //_favorMeterIndicatorGlow.color = _glowColors[newPlayerId];
                if (Services.Fighters[newPlayerId].HasGoldenGoal)
                {
                    _favorMeterIndicatorOutlineMaterial.SetColor("_OutlineColor", _glowColors[2]);
                }
                else
                {
                    _favorMeterIndicatorOutlineMaterial.SetColor("_OutlineColor", _glowColors[newPlayerId]);
                }
                hasIconChanged = true;
            }
        }
        Vector3 endScale = _favorMeterIndicator.rectTransform.localScale;
        endScale.x = _indicatorScaleDefault.x * playerIdMultiplier;
        endScale.y = _indicatorScaleDefault.y;
        _favorMeterIndicator.rectTransform.localScale = endScale;
        _isIndicatorFlipping = false;
        yield break;
    }

    private IEnumerator ResizeCharacterPortraits(int enlargingPlayer, float startFavor, float endFavor)
    {
        _isPortraitScaling = true;
        int shrinkingPlayer = enlargingPlayer == 0 ? 1 : 0;
        int playerIdMultiplier = enlargingPlayer == 0 ? 1 : -1;

        float enlargingStartScale;
        float enlargingEndScale;
        float shrinkingStartScale;
        float shrinkingEndScale;
        if (enlargingPlayer == 0)
        {
            enlargingStartScale = Mathf.Lerp(_portraitScaleBounds.x, _portraitScaleBounds.y, Mathf.Abs(startFavor - MaxFavor) / (MaxFavor * 2f));
            enlargingEndScale = Mathf.Lerp(_portraitScaleBounds.x, _portraitScaleBounds.y, Mathf.Abs(endFavor - MaxFavor) / (MaxFavor * 2f));
            shrinkingStartScale = Mathf.Lerp(_portraitScaleBounds.x, _portraitScaleBounds.y, Mathf.Abs(startFavor + MaxFavor) / (MaxFavor * 2f));
            shrinkingEndScale = Mathf.Lerp(_portraitScaleBounds.x, _portraitScaleBounds.y, Mathf.Abs(endFavor + MaxFavor) / (MaxFavor * 2f));
        }
        else
        {
            enlargingStartScale = Mathf.Lerp(_portraitScaleBounds.x, _portraitScaleBounds.y, Mathf.Abs(startFavor + MaxFavor) / (MaxFavor * 2f));
            enlargingEndScale = Mathf.Lerp(_portraitScaleBounds.x, _portraitScaleBounds.y, Mathf.Abs(endFavor + MaxFavor) / (MaxFavor * 2f));
            shrinkingStartScale = Mathf.Lerp(_portraitScaleBounds.x, _portraitScaleBounds.y, Mathf.Abs(startFavor - MaxFavor) / (MaxFavor * 2f));
            shrinkingEndScale = Mathf.Lerp(_portraitScaleBounds.x, _portraitScaleBounds.y, Mathf.Abs(endFavor - MaxFavor) / (MaxFavor * 2f));
        }
        enlargingStartScale = _portraitScales[enlargingPlayer] > enlargingStartScale ? _portraitScales[enlargingPlayer] : enlargingStartScale;

        float timer = 0f;
        float duration = _portraitEnlargeDuration > _portraitShrinkDuration ? _portraitEnlargeDuration : _portraitShrinkDuration;
        Easing enlargeFunction = Easing.CreateEasingFunc(Easing.Funcs.OutBack);
        Easing shrinkFunction = Easing.CreateEasingFunc(Easing.Funcs.CubicOut);
        while (timer <= duration)
        {
            yield return new WaitForFixedUpdate();
            timer += Time.fixedDeltaTime;
            if (timer <= _portraitEnlargeDuration)
            {
                _portraitScales[enlargingPlayer] = enlargeFunction.Ease(enlargingStartScale, enlargingEndScale, timer / _portraitEnlargeDuration);
            }
            if (timer <= _portraitShrinkDuration)
            {
                _portraitScales[shrinkingPlayer] = shrinkFunction.Ease(shrinkingStartScale, shrinkingEndScale, timer / _portraitShrinkDuration);
            }
            for (int i = 0; i < 2; i++)
            {
                _portraitOutlines[i].rectTransform.localScale = new Vector3(_portraitScales[i], _portraitScales[i], _portraitScales[i]);
            }
        }
        _portraitOutlines[enlargingPlayer].rectTransform.localScale = new Vector3(enlargingEndScale, enlargingEndScale, enlargingEndScale);
        _portraitOutlines[shrinkingPlayer].rectTransform.localScale = new Vector3(shrinkingEndScale, shrinkingEndScale, shrinkingEndScale);
        _isPortraitScaling = false;
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

    private void GoldenGoalGet(int player)
    {
        _favorMeterIndicatorOutlineMaterial.SetColor("_OutlineColor", _glowColors[2]);
        _portraitOutlines[player].color = _glowColors[2];
    }

    private void GoldenGoalLose(int player)
    {
        _favorMeterIndicatorOutlineMaterial.SetColor("_OutlineColor", _glowColors[_favoredPlayer]);
        for (int i = 0; i < 2; i++)
        {
            _portraitOutlines[i].color = _glowColors[i];
        }
    }

    private void SubscribeEvents()
    {
        onGoldenGoalEnabled += GoldenGoalGet;
        onGoldenGoalDisabled += GoldenGoalLose;
    }

    private void UnsubscribeEvents()
    {
        onGoldenGoalEnabled -= GoldenGoalGet;
        onGoldenGoalDisabled -= GoldenGoalLose;
    }
}
