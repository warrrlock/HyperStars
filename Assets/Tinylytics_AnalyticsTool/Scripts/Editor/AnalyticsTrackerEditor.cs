using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif



namespace Tinylytics {
	[CanEditMultipleObjects]
	[CustomEditor(typeof(Tinylytics_MetricWidget))]

	public class AnalyticsTrackerEditor : Editor {

		SerializedProperty metricname, triggertype;
		//SerializedProperty otherobj;
		//object dataholder;
		//BRAND_AnalyticsTracker myObjectRef;

		SerializedProperty datatosend;

	
		TriggerEvent chosentrigger;
		//int datatypeselected = 0;

		void OnEnable() {
			//myObjectRef = serializedObject.targetObject as BRAND_AnalyticsTracker;
			metricname = serializedObject.FindProperty("metric_name");
			triggertype = serializedObject.FindProperty("trigger").FindPropertyRelative("triggerEvent");
			chosentrigger = (TriggerEvent)triggertype.enumValueIndex;

			//otherobj = serializedObject.FindProperty("otherobjectref");

			datatosend = serializedObject.FindProperty("datatosend");
		}


		public override void OnInspectorGUI() {
			serializedObject.Update();
	


			EditorGUILayout.PropertyField(metricname, new GUIContent("Metric Name","This is a custom field to name the stat, like 'GameWon', 'Level4Loaded' or 'PlayerAccuracy'"));
			chosentrigger = (TriggerEvent)EditorGUILayout.EnumPopup(new GUIContent("Trigger","This is what will cause the tracker to fire, when this object is enabled, started, destroyed, etc."), chosentrigger);
			

			triggertype.enumValueIndex = (int)chosentrigger;


			//EditorGUILayout.HelpBox("This Component is designed to work with Unity Analytics, which is not currently enabled. To enable Analytics, go to Window/Services, select Analytics and click the 'Enable Analytics' button.", MessageType.Warning);

			

			//datatypeselected = EditorGUILayout.Popup("Data to Send", datatypeselected, datatosendchoicetext);
			//EditorGUILayout.BeginHorizontal();
			//EditorGUILayout.LabelField("", GUILayout.Width(EditorGUIUtility.labelWidth));

			//switch (datatypeselected) {
			//	case 0:
			//		stringholder = EditorGUILayout.TextField(stringholder);
			//		dataholder = (object)stringholder;
			//		break;
			//	case 1:
			//		floatholder = EditorGUILayout.FloatField(floatholder);
			//		dataholder = (object)floatholder;
			//		break;
			//	case 2:
			//		dataholder = (object)EditorGUILayout.IntField(0);
			//		break;
			//	case 3:
			//		dataholder = (object)EditorGUILayout.Toggle(false);
			//		break;
					
			//	case 4:
			//		EditorGUILayout.PropertyField(otherobj, new GUIContent("Other:"));
					
			//		break;

			
			
			//}
			//EditorGUILayout.EndHorizontal();



			//	EditorGUILayout.PropertyField(otherobj, new GUIContent("Other:"));
			//}



			
			EditorGUILayout.PropertyField(datatosend, new GUIContent("Data to Send", "This is the data which will be sent, you can send any string, int, float or bool, specified here or linked to a script"));
		
			


			//EditorGUILayout.IntSlider(testint, 0, 50, new GUIContent("Test"));

			//myObjectRef.SetDataToSend(dataholder);

			serializedObject.ApplyModifiedProperties();
		}
	}
}