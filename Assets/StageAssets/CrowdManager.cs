using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowdManager : MonoBehaviour
{
    //public List<Sprite> sprList = new List<Sprite>();
    //public int sprListLength;
    public MusicManager musicManager;

    // Start is called before the first frame update
    void Awake()
    {
        musicManager = GameObject.Find("Music Manager").GetComponent<MusicManager>();
        //sprListLength = sprList.Count;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }
}
