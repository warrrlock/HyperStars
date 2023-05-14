using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorDeactivator : MonoBehaviour
{
    public void DeactivateAnimator()
    {
        GetComponent<Animator>().enabled = false;
    }
}
