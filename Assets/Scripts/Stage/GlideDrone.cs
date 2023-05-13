using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WesleyDavies;

public class GlideDrone : MonoBehaviour
{
    private Animator _glideAnimator;
    [SerializeField] private string[] _droneGlides;
    private int _lastGlideIndex = -1;

    private void Awake()
    {
        _glideAnimator = GetComponent<Animator>();
    }

    private void Start()
    {
        SubscribeEvents();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        UnsubscribeEvents();
    }

    private void SubscribeEvents()
    {
        Services.CameraManager.onCameraFinalized += StartCheck;
    }

    private void UnsubscribeEvents()
    {
        Services.CameraManager.onCameraFinalized -= StartCheck;
    }

    public void StartCheck()
    {
        StartCoroutine(CheckGlide());
    }

    private IEnumerator CheckGlide()
    {
        while (true)
        {
            int randomDuration = Random.Range(8, 16);
            yield return new WaitForSeconds(randomDuration);
            int playChance = 20;
            if (Wrandom.RollPercentChance(playChance))
            {
                if (_lastGlideIndex < 0)
                {
                    _lastGlideIndex = _droneGlides.PickRandomIndex();
                }
                else
                {
                    List<string> validGlides = new(_droneGlides);
                    validGlides.RemoveAt(_lastGlideIndex);
                    _lastGlideIndex = validGlides.PickRandomIndex();
                }
                string glideAnimation = _droneGlides[_lastGlideIndex];
                if (!_glideAnimator.GetCurrentAnimatorStateInfo(0).IsName(glideAnimation))
                {
                    _glideAnimator.Play(glideAnimation, 0);
                }
            }
        }
    }
}
