using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FiniteStateMachine;
using TMPro;

public class FavorManager : MonoBehaviour
{
    [Tooltip("The maximum amount of favor a fighter can have.")]
    [SerializeField] private float _maxFavor;
    [Tooltip("How much the favor multiplier increases when favor reverses.")]
    [SerializeField] private float _favorMultiplierDelta;
    //[Tooltip("The percentage of total favor that a fighter needs to win in order to increase the favor multiplier.")]
    //[SerializeField][Range(0f, 1f)] private float _favorSwitchPercentage;

    /// <summary>
    /// If favor is < 0, player 1 is favored. If favor is > 0, player 2 is favored.
    /// </summary>
    private float _favor;
    private float _favorMultiplier = 1f;
    //private float[] _peakFavors;

    [SerializeField] private GameObject _favorMeter;
    [SerializeField] private GameObject _favorMeterIndicator;
    [SerializeField] private TextMeshProUGUI _multiplierText;

    private void Awake()
    {
        Services.FavorManager = this;
    }

    private void Start()
    {
        _favor = 0f;
        UpdateFavorMeter();
        //_peakFavors = new float[2];
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
                SceneReloader.Instance.ReloadScene();
            }
        }
        if (Mathf.Abs(_favor + value) < _favor)
        {
            //reverse favor
            _favorMultiplier += _favorMultiplierDelta;
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
        float indicatorX = Mathf.Lerp(_favorMeter.transform.position.x - _favorMeter.transform.localScale.x / 2f,
            _favorMeter.transform.position.x + _favorMeter.transform.localScale.x / 2f, (_favor + _maxFavor) / (_maxFavor * 2f));
        _favorMeterIndicator.transform.position = new Vector3(indicatorX, _favorMeterIndicator.transform.position.y, _favorMeterIndicator.transform.position.z);

        _multiplierText.text = "x" + _favorMultiplier;
        _multiplierText.rectTransform.position = new Vector3(indicatorX, _multiplierText.transform.position.y, _multiplierText.transform.position.z);
    }
}
