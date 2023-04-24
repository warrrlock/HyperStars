using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class example_senddataviacode : MonoBehaviour {


	void Start () {
		Tinylytics.AnalyticsManager.LogCustomMetric("Current Month", System.DateTime.Now.Month.ToString());
	}

}
