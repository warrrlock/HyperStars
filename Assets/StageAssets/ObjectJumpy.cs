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
    private void Awake()
    {
        
    }
    void Start()
    {
        crowdManager = GameObject.Find("CrowdManager").GetComponent<CrowdManager>();
        musicMan = crowdManager.musicManager;
        RandomizeColor();

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
        if (musicMan.Impressed)
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
    void RandomizeColor()
    {
        SpriteRenderer sprRend = gameObject.GetComponent<SpriteRenderer>();
        int sprIndex = Random.Range(0, crowdManager.sprListLength - 1);
        sprRend.sprite = crowdManager.sprList[sprIndex];
        float H, S, V;
        Color col = sprRend.color;
        Color.RGBToHSV(col, out H, out S, out V);
        float tempcol;
        if (Random.Range(0.0f, 1.0f) <= 0.3f)
            tempcol = Random.Range(0f, 0.14f);
        else tempcol = Random.Range(0.0f, 1.0f);
        //else
        //{
        //    tempcol = Random.Range(0.0f, 1.0f);
        //    if (tempcol > 0.18f && tempcol < 0.45f)
        //        tempcol += Random.Range(-0.1f, 0.35f); // extra should go to red?
        //}
        sprRend.color = Color.HSVToRGB(tempcol,S,V);
    }
}
