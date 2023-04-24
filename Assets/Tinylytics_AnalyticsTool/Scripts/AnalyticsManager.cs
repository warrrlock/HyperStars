using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tinylytics{
[AddComponentMenu("")] 
public class AnalyticsManager : MonoBehaviour {
		private static AnalyticsManager _instance;
		public static AnalyticsManager Instance { get { return _instance; } }

		private void Awake() {
			if (_instance != null && _instance != this) {
				Destroy(this.gameObject);
			} else {
				_instance = this;
				DontDestroyOnLoad(this);
			}
		}


	void Start () {
		sessionStartTime = Time.time;
	}

	float sessionStartTime = 0;

	public static void LogSessionPlaytime() {
		BackendManager.SendData("Session Playtime", (Time.time - Instance.sessionStartTime).ToString());
	}

	public static void LogCustomMetric(string metricName, string dataToSend){
		BackendManager.SendData(metricName, dataToSend);
	}



}
}