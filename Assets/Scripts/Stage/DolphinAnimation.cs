using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WesleyDavies;

public class DolphinAnimation : MonoBehaviour
{
    private Animator anim;
    //public int ranNumb;
    private int _spawnChance;
    private int _chanceIncreaseDelta = 12;
    public bool IsPlaying { get => anim.GetCurrentAnimatorStateInfo(0).IsName("Move"); }
    [SerializeField] private PassDrone _dronePass;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        if (RoundInformation.round < 3)
        {
            _spawnChance = 10;
        }
        else
        {
            _spawnChance = 25;
        }
        SubscribeEvents();
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
        StartCoroutine(CheckDolphin());
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        UnsubscribeEvents();
    }

    private void OnDolphinExited()
    {
        _spawnChance = Mathf.Clamp(_spawnChance - 40, 5, 100);
        StartCoroutine(CheckDolphin());
    }

    IEnumerator CheckDolphin()
    {
        while (true)
        {
            yield return new WaitForSeconds(11f);
            if (_dronePass.IsPlaying)
            {
                continue;
            }
            if (IsPlaying)
            {
                continue;
            }
            if (Wrandom.RollPercentChance(_spawnChance))
            {
                anim.Play("Move", 0);
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
