using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shadow : MonoBehaviour
{
    public Transform shadowSprite;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 shadowSpritePosition = shadowSprite.transform.position;
        shadowSpritePosition.y = -3.75f;
        shadowSprite.transform.position = shadowSpritePosition;

    }
}
