using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WesleyDavies;

public class PassDrone : MonoBehaviour
{
    private Animator _passAnimator;
    [SerializeField] private string[] _dronePasses;
    public bool IsPlaying
    {
        get => _passAnimator.GetCurrentAnimatorStateInfo(0).IsName(_dronePasses[0]) || _passAnimator.GetCurrentAnimatorStateInfo(0).IsName(_dronePasses[1]) || _passAnimator.GetCurrentAnimatorStateInfo(0).IsName(_dronePasses[2]);
    }
    private int _spawnChance = 20;
    private int _chanceIncreaseDelta = 7;

    [SerializeField] private DolphinAnimation _dolphinAnimation;

    private void Awake()
    {
        _passAnimator = GetComponent<Animator>();
    }

    private void Start()
    {
        SubscribeEvents();
    }

    public void StartCheck()
    {
        StartCoroutine(CheckPass());
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

    private void OnPassExited()
    {
        _spawnChance = Mathf.Clamp(_spawnChance - 40, 5, 100);
        StartCoroutine(CheckPass());
    }

    private IEnumerator CheckPass()
    {
        while (true)
        {
            yield return new WaitForSeconds(6f);
            if (_dolphinAnimation.IsPlaying)
            {
                continue;
            }
            if (IsPlaying)
            {
                continue;
            }
            if (Wrandom.RollPercentChance(_spawnChance))
            {
                string passAnimation = _dronePasses.PickRandom();
                _passAnimator.Play(passAnimation,0);
                yield break;
            }
            else
            {
                int chanceIncrease = Services.MusicManager.Impressed ? _chanceIncreaseDelta + 5 : _chanceIncreaseDelta;
                _spawnChance = Mathf.Clamp(_spawnChance + chanceIncrease, 0, 100);
            }
        }
    }
}
