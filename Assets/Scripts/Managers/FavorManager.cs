using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FiniteStateMachine;
using TMPro;
using UI;

public class FavorManager : MonoBehaviour
{
    [Tooltip("The maximum amount of favor a fighter can have.")]
    [SerializeField] private float _maxFavor;
    [Tooltip("How much the favor multiplier increases when favor reverses.")]
    [SerializeField] private float _favorMultiplierDelta;
    //[Tooltip("The percentage of total favor that a fighter needs to win in order to increase the favor multiplier.")]
    //[SerializeField][Range(0f, 1f)] private float _favorSwitchPercentage;

    /// <summary>
    /// If favor is < 0, player 1 is favored. If favor is > 0, player 0 is favored.
    /// </summary>
    private float _favor;
    private float _favorMultiplier = 1f;
    private int _favoredPlayer = -1;
    //private float[] _peakFavors;

    [SerializeField] private RectTransform _favorMeter;
    [SerializeField] private RectTransform _favorMeterIndicator;
    [SerializeField] private TextMeshProUGUI _multiplierText;
    [SerializeField] private Canvas _multiplierTextCanvas;
    [SerializeField] private GameEvent _winConditionEvent;
    //[SerializeField] private FavourMeter _favourMeter;



    [SerializeField] private RectTransform _p1Bar;
    [SerializeField] private RectTransform _p2Bar;

    private float _barMinimum = 0f;
    private float _barMaximum;

    private void Awake()
    {
        Services.FavorManager = this;
    }

    private void Start()
    {
        _favor = 0f;
        //_peakFavors = new float[2];

        _barMaximum = 625f;
        //_favourMeter.Initialize();
        UpdateFavorMeter();
    }

    //Attacks should have a cooldown time where they don't increase favor as much when used in succession.

    public void IncreaseFavor(int playerId, float value)
    {
        value = playerId == 0 ? value : -value;
        value *= _favorMultiplier;
        if (Mathf.Abs(_favor) == _maxFavor)
        {
            if (Mathf.Abs(_favor + value) > _maxFavor)
            {
                //player wins
                Debug.Log("Player " + playerId + " wins.");
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
                if (_favor + value < _favor)
                {
                    //reverse favor
                    _favorMultiplier += _favorMultiplierDelta;
                    _favoredPlayer = playerId;
                }
                break;
            case 1:
                if (_favor + value > _favor)
                {
                    //reverse favor
                    _favorMultiplier += _favorMultiplierDelta;
                    _favoredPlayer = playerId;
                }
                break;
        }
        _favor += value;
        _favor = Mathf.Clamp(_favor, -_maxFavor, _maxFavor);
        if (Mathf.Abs(_favor) == _maxFavor)
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

    private void UpdateFavorMeter()
    {
        _p1Bar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Lerp(_barMinimum, _barMaximum, Mathf.Abs(_favor - _maxFavor) / (_maxFavor * 2f)));
        _p2Bar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Lerp(_barMinimum, _barMaximum, Mathf.Abs(_favor + _maxFavor) / (_maxFavor * 2f)));

        //float indicatorX = Mathf.Lerp(_favorMeter.transform.position.x - _favorMeter.transform.localScale.x / 2f,
        //    _favorMeter.transform.position.x + _favorMeter.transform.localScale.x / 2f, (_favor + _maxFavor) / (_maxFavor * 2f));
        //_favorMeterIndicator.transform.position = new Vector3(indicatorX, _favorMeterIndicator.transform.position.y, _favorMeterIndicator.transform.position.z);

        float indicatorX = Mathf.Lerp(-_favorMeter.rect.width / 2f,
    _favorMeter.rect.width / 2f, (_favor + _maxFavor) / (_maxFavor * 2f));
        _favorMeterIndicator.anchoredPosition = new Vector3(indicatorX, 0f, 0f);

        if (_multiplierText)
        {
            _multiplierText.text = "x" + _favorMultiplier;
            _multiplierText.rectTransform.position = new Vector3(WorldToUISpace(_multiplierTextCanvas, new Vector3(indicatorX, 0f, 0f)).x, _multiplierText.transform.position.y, 0f);
        }
    }

    public Vector3 WorldToUISpace(Canvas parentCanvas, Vector3 worldPos)
    {
        //Convert the world for screen point so that it can be used with ScreenPointToLocalPointInRectangle function
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        Vector2 movePos;

        //Convert the screenpoint to ui rectangle local point
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvas.transform as RectTransform, screenPos, parentCanvas.worldCamera, out movePos);
        //Convert the local point to world point
        return parentCanvas.transform.TransformPoint(movePos);
    }
}
