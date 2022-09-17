using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    public Transform leftChar;
    public Transform rightChar;

    public float minDistance;
    public float maxDistance;
    public float minDistY;
    public float minDistZ;
    public float maxDistZ;

    private float zPos;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float centerPos = (leftChar.position.x + rightChar.position.x) / 2;
        float vertPos = Mathf.Abs(leftChar.position.y - rightChar.position.y) * 0.5f;
        zPos = (Mathf.Abs(leftChar.position.x) + Mathf.Abs(rightChar.position.x)) * -2;

        zPos = Mathf.Clamp(zPos, minDistZ, maxDistZ); //this was forced lol, this creates an effect that once min/max dist is reached it just moves to it instantly.
                                                      //as opposed to how vertpos in the Y axis is constantly moving.

        transform.position = new Vector3(centerPos, transform.position.y, transform.position.z);

        //vertPos < minDistY ? minDistY : minDistY
    }
}
