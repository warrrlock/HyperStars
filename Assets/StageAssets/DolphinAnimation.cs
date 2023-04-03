using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DolphinAnimation : MonoBehaviour
{
    public Animator anim;
    public int ranNumb;

    void Start()
    {
        StartCoroutine(checkDolphin());
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    IEnumerator checkDolphin()
    {
        yield return new WaitForSeconds(30f);
        ranNumb = Random.Range(0, 4);
        //Debug.Log(ranNumb);
        if (ranNumb > 2)
        {
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Move"))
                anim.Play("Move", 0);
        }
        StartCoroutine(checkDolphin());
    }
}
