using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialScrollOnlyX : MonoBehaviour
{
    public float spdX;
    MeshRenderer meshRend;
    // Start is called before the first frame update
    void Start()
    {
        meshRend = gameObject.GetComponent<MeshRenderer>();    
    }

    // Update is called once per frame
    void Update()
    {
        meshRend.material.mainTextureOffset = new Vector2(Time.realtimeSinceStartup * spdX, meshRend.material.mainTextureOffset.y);
    }
}
