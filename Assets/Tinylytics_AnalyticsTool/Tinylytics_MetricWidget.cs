using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Tinylytics {
[AddComponentMenu("Tinylytics/Tinylytics Metric Widget")]
	public class Tinylytics_MetricWidget : MonoBehaviour {
		//https://docs.unity3d.com/Manual/JSONSerialization.html

		
		[SerializeField] public string metric_name;
		[SerializeField] public ValueProperty datatosend;

		[SerializeField] public AnalyticsTrigger trigger;


		//TimeSinceGameStarted,
		//TotalTimePlayed




		void Awake() {
			if (trigger.triggerEvent == TriggerEvent.Awake) {
				SendEvent();
			}
		}

		void Start() {
			if (trigger.triggerEvent == TriggerEvent.Start) {
				SendEvent();
			}
		}

		void OnEnable() {
			if (trigger.triggerEvent == TriggerEvent.OnEnable) {
				SendEvent();
			} else if (trigger.triggerEvent == TriggerEvent.OnSceneUnloaded) {
				SceneManager.sceneUnloaded += OnSceneUnloaded;
			}
		}

		void OnDisable() {
			if (trigger.triggerEvent == TriggerEvent.OnDisable) {
				SendEvent();
			}
		}

		void OnDestroy() {
			if (trigger.triggerEvent == TriggerEvent.OnDestroy) {
				SendEvent();
			}
		}

		void OnSceneUnloaded(Scene scene) {
			if (trigger.triggerEvent == TriggerEvent.OnSceneUnloaded) {
				SendEvent();
			}
		}

		void OnApplicationQuit() {
			if (trigger.triggerEvent == TriggerEvent.OnApplicationQuit) {
				SendEvent();
			}
		}


		void SendEvent() {
			//payload.Send();
			if(BackendManager.Instance != null){
						BackendManager.SendData(metric_name, datatosend.propertyValue);
			}
		}


	}
}