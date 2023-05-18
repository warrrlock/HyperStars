using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectJumpy : MonoBehaviour
{
    // Start is called before the first frame update
    public bool randomize;
    public bool startWithHeight;
    public bool onPlat;
    public CrowdManager crowdManager;

    public float minSpeed;
    public float maxSpeed;
    public float maxHeight;
    float speed;
    float height;
    private float initHeight;
    private float jumpHeight;
    public Transform platTrans;
    private float platHeight;
    private float platInitHeight;

    private MusicManager musicMan;
    public bool menuMode;

    private void Awake()
    {
        
    }
    void Start()
    {
        if (!menuMode)
        {
            crowdManager = GameObject.Find("CrowdManager").GetComponent<CrowdManager>();
            musicMan = crowdManager.musicManager;
        }

        if (onPlat)
        {
            platInitHeight = platTrans.position.y;
        }
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
        if (!menuMode&&musicMan.Impressed)
            jumpHeight = Mathf.Abs(height * 1.8f * Mathf.Sin(speed * 1.8f * Time.realtimeSinceStartup));
        else
            jumpHeight = Mathf.Abs(height * Mathf.Sin(speed * Time.realtimeSinceStartup));

        if (onPlat)
        {
            platHeight = platTrans.position.y;
            transform.position = new Vector3(transform.position.x, initHeight+jumpHeight + (platHeight - platInitHeight), transform.position.z);
        }
        else
            transform.position = new Vector3(transform.position.x, initHeight+jumpHeight, transform.position.z);
    }
}
