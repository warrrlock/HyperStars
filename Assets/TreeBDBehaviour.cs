using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeBDBehaviour : MonoBehaviour
{
    public GameObject rightEyeObj;
    public GameObject leftEyeObj;
    public GameObject mouthObj;
    public Material matEyeNormal;
    public Material matEyeLight;
    public Material matMouthNormal;
    public Material matMouthHappy;
    private bool unnrmMatSet; 
    private bool unhappyMatSet;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Services.MusicManager.Impressed)
        {
            unhappyMatSet = false;
            if (!unnrmMatSet)
            {
                leftEyeObj.gameObject.GetComponent<MeshRenderer>().material = matEyeLight;
                rightEyeObj.gameObject.GetComponent<MeshRenderer>().material = matEyeLight;
                mouthObj.gameObject.GetComponent<MeshRenderer>().material = matMouthHappy;
                unnrmMatSet = true;
            }
        }
        else
        {
            unnrmMatSet = false;
            if (!unhappyMatSet)
            {
                leftEyeObj.gameObject.GetComponent<MeshRenderer>().material = matEyeNormal;
                rightEyeObj.gameObject.GetComponent<MeshRenderer>().material = matEyeNormal;
                mouthObj.gameObject.GetComponent<MeshRenderer>().material = matMouthNormal;
                unhappyMatSet = true;
            }
        }
    }
}
