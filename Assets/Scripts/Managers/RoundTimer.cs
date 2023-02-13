using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RoundTimer : MonoBehaviour
{
    private float _roundTime;
    [Tooltip("The amount of time until the favor meter starts shrinking.")]
    [SerializeField] private float _startShrinkDelay;
    [Tooltip("How quickly the favor meter shrinks in units/sec.")]
    [SerializeField] private float _shrinkSpeed;
    [Tooltip("The minimum size of the favor meter in favor units.")]
    [SerializeField] private float _minFavorMeter;

    private void Start()
    {
        StartCoroutine(Timer());
    }

    public IEnumerator Timer()
    {
        yield return new WaitForSeconds(_startShrinkDelay);
        while (Services.FavorManager.MaxFavor > _minFavorMeter)
        {
            Services.FavorManager.ResizeFavorMeter(-_shrinkSpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
        yield break;
    }
}
