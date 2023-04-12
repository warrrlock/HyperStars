using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (speed == 0)
            speed = 5;
        transform.Rotate(0,speed * Time.deltaTime,0);//Y
    }
}
