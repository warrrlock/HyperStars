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

    void Update()
    {
    
    }
}
