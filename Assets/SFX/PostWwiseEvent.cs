using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostWwiseEvent : MonoBehaviour
{
    public AK.Wwise.Event[] comboEvents;

    public void PlayComboSound(int index)
    {
        comboEvents[index].Post(gameObject);
    }

    public void PlayComboSound2()
    {
        comboEvents[1].Post(gameObject);
    }

    public void PlayComboSound3()
    {
        comboEvents[2].Post(gameObject);
    }

    public void PlayComboSound4()
    {
        comboEvents[3].Post(gameObject);
    }

    public void PlayComboSound5()
    {
        comboEvents[4].Post(gameObject);
    }
    
    // Update is called once per frame.
    void Update()
    {
    
    }
}
