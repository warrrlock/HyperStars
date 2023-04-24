using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Text;
using System;
using System.Reflection;

namespace Tinylytics {

	public class DataPacket {

		public Dictionary<string, string> data = new Dictionary<string, string>();

		
		public DataPacket(string metricname, string newdata) {

			//All the standard fields
			if (Application.isEditor) {
				data.Add("TestingStatus", "InEditor");
			} else {
				data.Add("TestingStatus", "LiveBuild");
			}
			data.Add("Player_UniqueID", SystemInfo.deviceUniqueIdentifier.ToString());
			data.Add("Player_deviceModel", SystemInfo.deviceModel.ToString());
			data.Add("Player_OS", SystemInfo.operatingSystem.ToString());
			data.Add("Player_OSFamily", SystemInfo.operatingSystemFamily.ToString());
			//data.Add("Player_processorType", SystemInfo.processorType.ToString());
			data.Add("Player_SystemMemory", SystemInfo.systemMemorySize.ToString());
			data.Add("Build_UniqueID", Application.buildGUID.ToString());
			data.Add("Build_DateTime", BuildInfo.BUILD_TIME);

			//new data
			data.Add("MetricName", metricname);
			data.Add("MetricData", newdata);
		}

	}

[AddComponentMenu("")] 
	public class BackendManager : MonoBehaviour {
		public bool Analytics_Enabled = true;
		private static BackendManager _instance;

		public static BackendManager Instance { get { return _instance; } }

		private void Awake() {
			if (_instance != null && _instance != this) {
				Destroy(this.gameObject);
			} else {
				_instance = this;
				DontDestroyOnLoad(this);
				this.gameObject.name = "Tinylytics Backend Manager";
			}
		}




		public static string UniqueURLCode;

		public static void SetUniqueURL(string code) {
			UniqueURLCode = code;
		}


		public static void SendData(string metricname, string data) {
			if(_instance.Analytics_Enabled){
			DataPacket tosend = new DataPacket(metricname, data);

			Instance.StartCoroutine(Instance.PostData(tosend));
			//Debug.Log("Attempted to send data");

			}
		}


		IEnumerator PostData(DataPacket datatosend) {
			UnityWebRequest www = UnityWebRequest.Post("https://script.google.com/macros/s/" + UniqueURLCode + "/exec?", datatosend.data);

			//yield return www.Send();
			yield return www.SendWebRequest();

			if (www.isNetworkError) {
				Debug.Log(www.error);
			} else {
				//Debug.Log("Form upload complete!");
			}
		}
		

		//IEnumerator Post(string url, string bodyJsonString) {
		//	var request = new UnityWebRequest(url, "POST");
		//	byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
		//	request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
		//	request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
		//	request.SetRequestHeader("Content-Type", "application/json");

		//	yield return request.Send();

		//	//Debug.Log("Status Code: " + request.responseCode);
		//}

	}
}