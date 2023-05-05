using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraSwitch : MonoBehaviour
{
    public void onCamSwitch(){
        Services.CameraManager.onCameraSwitch?.Invoke();
    }
}
