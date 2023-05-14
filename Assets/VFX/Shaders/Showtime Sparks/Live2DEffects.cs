using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Live2DEffects : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void TriggerSilhouette()
    {
        Material[] mats =
        {
            Services.Fighters[0].GetComponent<SpriteRenderer>().material, 
            Services.Fighters[1].GetComponent<SpriteRenderer>().material
        };
        Services.CameraManager.SilhouetteToggle(true, mats, new Color(0, 0, 0), .5f, .35f);
    }
}
