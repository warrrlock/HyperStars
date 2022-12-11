using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shadow : MonoBehaviour
{
    public Transform shadowSprite;
    public Vector3 shadowSpritePosition;
    public float yPos = -3.75f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        shadowSpritePosition = shadowSprite.transform.position;
        shadowSpritePosition.y = yPos;
        shadowSprite.transform.position = shadowSpritePosition;

    }
}
