using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectJumpy : MonoBehaviour
{
    // Start is called before the first frame update
    public bool randomize;
    public bool startWithHeight;
    public float minSpeed;
    public float maxSpeed;
    public float maxHeight;
    float speed;
    float height;
    private float initHeight;

    private MusicManager musicMan;
    private void Awake()
    {
        musicMan = GameObject.Find("Music Manager").GetComponent<MusicManager>();
    }
    void Start()
    {
        if (startWithHeight)
            initHeight = transform.position.y;
        if (randomize)
        {
            speed = 3 + Random.Range(minSpeed, maxSpeed);
            height = Random.Range(0, maxHeight);
        }
        else
            speed = maxSpeed;
            height = maxHeight;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (musicMan.Impressed)
            transform.position = new Vector3(transform.position.x, initHeight + Mathf.Abs(height*1.8f * Mathf.Sin(speed * 1.8f * Time.realtimeSinceStartup)), transform.position.z);
        else
            transform.position = new Vector3(transform.position.x, initHeight + Mathf.Abs(height * Mathf.Sin(speed * Time.realtimeSinceStartup)), transform.position.z);
    }
}
