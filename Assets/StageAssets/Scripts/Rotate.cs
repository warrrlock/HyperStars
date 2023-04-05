using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public bool rotateX;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(rotateX)
            transform.Rotate(5f * Time.deltaTime,0,0);
        else
            transform.Rotate(0,5f * Time.deltaTime,0);//Y
    }
}
