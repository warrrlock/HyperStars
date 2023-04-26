using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WesleyDavies;

public class DolphinAnimation : MonoBehaviour
{
    public Animator anim;
    //public int ranNumb;
    private int _spawnChance;
    private int _chanceIncreaseDelta = 12;

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
        StartCoroutine(CheckDolphin());
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    //IEnumerator checkDolphin()
    //{
    //    yield return new WaitForSeconds(30f);
    //    ranNumb = Random.Range(0, 4);
    //    //Debug.Log(ranNumb);
    //    if (ranNumb > 2)
    //    {
    //        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Move"))
    //            anim.Play("Move", 0);
    //    }
    //    StartCoroutine(checkDolphin());
    //}

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
            if (Wrandom.RollPercentChance(_spawnChance))
            {
                if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Move"))
                {
                    anim.Play("Move", 0);
                    yield break;
                }
            }
            else
            {
                int chanceIncrease = Services.MusicManager.Impressed ? _chanceIncreaseDelta + 5 : _chanceIncreaseDelta;
                _spawnChance = Mathf.Clamp(_spawnChance + chanceIncrease, 0, 100);
            }
        }
    }
}
