using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    // public AK.Wwise.Event musicStart;
    void Start()
    {
        AkSoundEngine.PostEvent("Play_LisaTheme", gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
