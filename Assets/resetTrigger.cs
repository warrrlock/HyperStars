using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class resetTrigger : MonoBehaviour
{
    Animator m_Animator;
    
    
    void Awake()
    {
        m_Animator = gameObject.GetComponent<Animator>();
    }
    
    
    public void onReset(){
        m_Animator.ResetTrigger("Begin");
    }
}
