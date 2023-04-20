using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeRect : MonoBehaviour
{

    [SerializeField] float inspectorPerlinMult = 50f;
    [SerializeField] float inspectorMagnitude = 100f;
    [SerializeField] float inspectorShakeTime = 1f;


    bool shaking;
    Vector3 originalPosition;
    float myTimeTracker = 0;
    RectTransform myTransform;
    float myMagnitude;
    float myTime;
    float myPerlin;

    void Awake() {
        myTransform = GetComponent<RectTransform>();
        originalPosition = myTransform.anchoredPosition3D;
    }
    

    void StartShaking() {
        originalPosition = new Vector3(myTransform.anchoredPosition3D.x, myTransform.anchoredPosition3D.y, myTransform.anchoredPosition3D.z);
        myTimeTracker = 0;
        shaking = true;
    }

    void StopShaking() {
        //return transform to original position
        myTransform.anchoredPosition3D = originalPosition;
        shaking = false;
    }

    public void ShakeIt(float magnitude, float time, float perlinMultiplier) {
        StopShaking();
        myMagnitude = magnitude;
        myTime = time;
        myPerlin = perlinMultiplier;
        StartShaking();
    }

    public void ShakeIt(float magnitude, float time) {
        StopShaking();
        myMagnitude = magnitude;
        myTime = time;
        myPerlin = inspectorPerlinMult;
        StartShaking();
    }

    public void ShakeIt() {
        StopShaking();
        myMagnitude = inspectorMagnitude;
        myTime = inspectorShakeTime;
        myPerlin = inspectorPerlinMult;
        StartShaking();
    }

    void Update() {
        //using Update instead of a coroutine so it only happens once, and so it doesn't change position.
        //have more control
        //and can do things "on stop coroutine" 
        if(shaking) {
            if(myTimeTracker >= myTime) {
                StopShaking();
            } else {
                float thisMag = myMagnitude * (1 - (myTimeTracker / myTime)); //decreases mag over time 
                float x = Mathf.PerlinNoise(0, Time.time * myPerlin) * thisMag - (thisMag / 2);
                float y = Mathf.PerlinNoise(Time.time * myPerlin, 0) * thisMag - (thisMag / 2);
                Vector3 pos = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);
                myTransform.anchoredPosition3D = pos;
                myTimeTracker += Time.deltaTime;
            }
        }
    }

    //shake anything with this thing anywhere
    //but this is NOT SAFE for being interrupted 
    public static IEnumerator ShakeTransform(RectTransform t, float magnitude, float time, float perlinMultiplier, Vector3 originalPos) {

        Vector3 origPos = new Vector3(t.anchoredPosition3D.x, t.anchoredPosition3D.y, t.anchoredPosition3D.z);

        for(float tracker = 0; tracker < time; tracker += Time.deltaTime) {
            float thisMag = magnitude * (1 - (tracker / time)); //decreases mag over time 
            float x = Mathf.PerlinNoise(0, Time.time * perlinMultiplier) * thisMag - (thisMag / 2);
            float y = Mathf.PerlinNoise(Time.time * perlinMultiplier, 0) * thisMag - (thisMag / 2);
            Vector3 pos = new Vector3(origPos.x + x, origPos.y + y, origPos.z);
            t.anchoredPosition3D = pos;
            yield return null;
        }

        t.anchoredPosition3D = origPos;

    }
}
