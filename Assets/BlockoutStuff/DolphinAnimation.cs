using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DolphinAnimation : MonoBehaviour
{
    public Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(checkDolphin());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator checkDolphin()
    {
        yield return new WaitForSeconds(5f);
        if (Random.Range(0, 5) > 3)
        {
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Move"))
                anim.Play("Move", 0);
        }
        StartCoroutine(checkDolphin());
    }
}
