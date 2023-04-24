using UnityEngine;

namespace Tinylytics {
	class AnalyticsInitializer {


		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		static void OnBeforeSceneLoadRuntimeMethod() {
			//Debug.Log("Before first scene loaded");
			Tinylytics_Config storage = Resources.Load<Tinylytics_Config>("Tinylytics_URLConfig");

			//GameObject instance = GameObject.Instantiate(Resources.Load("AnalyticsManager")) as GameObject;
			GameObject.Instantiate(Resources.Load("Tinylytics_BackendManager"));
			BackendManager.SetUniqueURL(storage.deploymentID);


		}


		//[RuntimeInitializeOnLoadMethod]
		//static void OnRuntimeMethodLoad() {
		//	Debug.Log("RuntimeMethodLoad: After first scene loaded");
		//}



	}
}