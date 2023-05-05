using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class triggerAnimation : MonoBehaviour
{

    Animator m_Animator;
    public int begin = 0;
    
    
    void Awake()
    {
        m_Animator = gameObject.GetComponent<Animator>();
    }
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(begin == 1){
            m_Animator.SetTrigger("Begin");
        }
    }
}
