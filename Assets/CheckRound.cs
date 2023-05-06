using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckRound : MonoBehaviour
{

    //public GameObject roundInfoObject;
    //private RoundInformation _roundInfoScript;
    // Start is called before the first frame update
    void Awake()
    {
        if(RoundInformation.round != 1){
            Destroy(gameObject);
        }

        if(SceneInfo.IsTraining == true){
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if(RoundInformation.round != 1){
            Destroy(gameObject);
        }

        if(SceneInfo.IsTraining == true){
            Destroy(gameObject);
        }
    }

}
