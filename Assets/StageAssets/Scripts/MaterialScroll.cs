using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialScroll : MonoBehaviour
{
    public float spdX;
    public float spdY;
    MeshRenderer meshRend;
    // Start is called before the first frame update
    void Start()
    {
        meshRend = gameObject.GetComponent<MeshRenderer>();    
    }

    // Update is called once per frame
    void Update()
    {
        meshRend.material.mainTextureOffset = new Vector2(Time.realtimeSinceStartup * spdX, Time.realtimeSinceStartup * spdY);
    }
}
