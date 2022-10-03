using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WesleyDavies;
using WesleyDavies.UnityFunctions;

public class TestClass : MonoBehaviour
{
    public GameObject square;
    public GameObject square2;
    public GameObject square3;
    public GameObject square4;

    private void Start()
    {
        //Juice.DilateTime(this, 2f, 5f);

        //this.DilateTime(2f, 5f);

        //value.Tlerp(5f, 5f, this);

        //StartCoroutine(Mathw.Tlerp(result => square.transform.rotation = result, square.transform.rotation, square2.transform.rotation, 2f, Easing.Funcs.Linear));

        //StartCoroutine(Mathw.Tslerp(result => square3.transform.rotation = result, square3.transform.rotation, square2.transform.rotation, 2f));

        StartCoroutine(Mathw.Tslerp(result => square3.transform.position = result, square3.transform.position, square.transform.position, 2f));
        StartCoroutine(Mathw.Tlerp(result => square4.transform.position = result, square4.transform.position, square.transform.position, 2f));
    }

    private void Update()
    {
        //Debug.Log(value);
    }
}
