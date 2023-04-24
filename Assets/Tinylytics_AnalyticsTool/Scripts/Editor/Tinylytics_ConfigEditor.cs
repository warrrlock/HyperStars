using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Tinylytics {

	[InitializeOnLoad]
	public class Tinylytics_InitialConfigChecker {
		static Tinylytics_InitialConfigChecker() {
			// Defer calling Runonce until the first editor update is called, we do this so that Application.isPlaying gets the correct value
			//EditorApplication.update += RunOnce;
		}
		static void RunOnce() {
			// Only show upgrade popup when project is opened, not when the app is playing. This will also show popup everytime the project recompiles.
			if (!Application.isPlaying)
				EditorWindow.GetWindowWithRect<Tinylytics_ConfigEditor>(new Rect(300, 300, 380, 130), true, "Unity Analytics SDK");
			EditorApplication.update -= RunOnce;
		}
	}


	public class Tinylytics_ConfigEditor : EditorWindow {

		public Tinylytics_Config analyticsConfig;

		[MenuItem("Window/Tinylytics/Configure")]
		static void Init() {
			EditorWindow.GetWindow(typeof(Tinylytics_ConfigEditor));
		}

		void OnEnable() {
			if (EditorPrefs.HasKey("AnalyticsConfigObjectPath")) {
				string objectPath = EditorPrefs.GetString("AnalyticsConfigObjectPath");
				analyticsConfig = AssetDatabase.LoadAssetAtPath(objectPath, typeof(Tinylytics_Config)) as Tinylytics_Config;
			}
		}

		private GUIContent close = new GUIContent("Close", "Close this window.");
		void OnGUI() {
			GUILayout.BeginHorizontal();
			GUILayout.Label("Analytics Configuration Settings", EditorStyles.whiteLargeLabel);
			GUILayout.EndHorizontal();


			if (analyticsConfig == null) {
				GUILayout.BeginHorizontal();
				GUILayout.Space(10);
				GUILayout.Label("No Config File Detected", EditorStyles.boldLabel);
				GUILayout.Space(20);
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Create New Config File", GUILayout.ExpandWidth(false))) {
					CreateNewConfigFile();
				}
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Set to Existing Config File", GUILayout.ExpandWidth(false))) {
					OpenConfigFile();
				}
				GUILayout.EndHorizontal();
			} else if (analyticsConfig != null) {
				GUILayout.BeginHorizontal();
				GUILayout.Space(10);
				if (GUILayout.Button("Show Config File")) {
					EditorUtility.FocusProjectWindow();
					Selection.activeObject = analyticsConfig;
					return;
				}
				GUILayout.Space(20);
				GUILayout.EndHorizontal();


				GUILayout.BeginHorizontal();
				GUILayout.Label("Deployment ID:", EditorStyles.boldLabel);
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();

				analyticsConfig.deploymentID = EditorGUILayout.TextField("Deployment ID", analyticsConfig.deploymentID as string);


				GUILayout.Space(60);

				//if (GUILayout.Button("Add Item", GUILayout.ExpandWidth(false))) {
				//	//AddItem();
				//}
				//if (GUILayout.Button("Delete Item", GUILayout.ExpandWidth(false))) {
				//	//DeleteItem(viewIndex - 1);
				//}

				GUILayout.EndHorizontal();

				//Show what's in config here =>
				//if (1 > 0) {
				//	GUILayout.BeginHorizontal();
				//	viewIndex = Mathf.Clamp(EditorGUILayout.IntField("Current Item", viewIndex, GUILayout.ExpandWidth(false)), 1, inventoryItemList.itemList.Count);
				//	//Mathf.Clamp (viewIndex, 1, inventoryItemList.itemList.Count);
				//	EditorGUILayout.LabelField("of   " + inventoryItemList.itemList.Count.ToString() + "  items", "", GUILayout.ExpandWidth(false));
				//	GUILayout.EndHorizontal();

				//	inventoryItemList.itemList[viewIndex - 1].itemName = EditorGUILayout.TextField("Item Name", inventoryItemList.itemList[viewIndex - 1].itemName as string);
				//	inventoryItemList.itemList[viewIndex - 1].itemIcon = EditorGUILayout.ObjectField("Item Icon", inventoryItemList.itemList[viewIndex - 1].itemIcon, typeof(Texture2D), false) as Texture2D;
				//	inventoryItemList.itemList[viewIndex - 1].itemObject = EditorGUILayout.ObjectField("Item Object", inventoryItemList.itemList[viewIndex - 1].itemObject, typeof(Rigidbody), false) as Rigidbody;

				//	GUILayout.Space(10);

				//	GUILayout.BeginHorizontal();
				//	inventoryItemList.itemList[viewIndex - 1].isUnique = (bool)EditorGUILayout.Toggle("Unique", inventoryItemList.itemList[viewIndex - 1].isUnique, GUILayout.ExpandWidth(false));
				//	inventoryItemList.itemList[viewIndex - 1].isIndestructible = (bool)EditorGUILayout.Toggle("Indestructable", inventoryItemList.itemList[viewIndex - 1].isIndestructible, GUILayout.ExpandWidth(false));
				//	inventoryItemList.itemList[viewIndex - 1].isQuestItem = (bool)EditorGUILayout.Toggle("QuestItem", inventoryItemList.itemList[viewIndex - 1].isQuestItem, GUILayout.ExpandWidth(false));
				//	GUILayout.EndHorizontal();

				//	GUILayout.Space(10);

				//	GUILayout.BeginHorizontal();
				//	inventoryItemList.itemList[viewIndex - 1].isStackable = (bool)EditorGUILayout.Toggle("Stackable ", inventoryItemList.itemList[viewIndex - 1].isStackable, GUILayout.ExpandWidth(false));
				//	inventoryItemList.itemList[viewIndex - 1].destroyOnUse = (bool)EditorGUILayout.Toggle("Destroy On Use", inventoryItemList.itemList[viewIndex - 1].destroyOnUse, GUILayout.ExpandWidth(false));
				//	inventoryItemList.itemList[viewIndex - 1].encumbranceValue = EditorGUILayout.FloatField("Encumberance", inventoryItemList.itemList[viewIndex - 1].encumbranceValue, GUILayout.ExpandWidth(false));
				//	GUILayout.EndHorizontal();

				//	GUILayout.Space(10);

				//} else {
				//	GUILayout.Label("This Inventory List is Empty.");
				//}
			}



			if (GUILayout.Button(close, GUILayout.MaxWidth(120))) {
				Close();
			}


			if (GUI.changed) {
				EditorUtility.SetDirty(analyticsConfig);
			}
		}

		void CreateNewConfigFile() {
			// There is no overwrite protection here!
			// There is No "Are you sure you want to overwrite your existing object?" if it exists.
			// This should probably get a string from the user to create a new name and pass it ...

			analyticsConfig = CreateAnalyticsConfigFile();
			if (analyticsConfig) {
				//initialize it:
				//inventoryItemList.itemList = new List<InventoryItem>();
				string relPath = AssetDatabase.GetAssetPath(analyticsConfig);
				EditorPrefs.SetString("AnalyticsConfigObjectPath", relPath);
			}
		}

		Tinylytics_Config CreateAnalyticsConfigFile() {
			Tinylytics_Config asset = ScriptableObject.CreateInstance<Tinylytics_Config>();
			AssetDatabase.CreateAsset(asset, "Assets/Tinylytics_AnalyticsTool/Resources/Tinylytics_URLConfig.asset");
			AssetDatabase.SaveAssets();
			return asset;
		}


		void OpenConfigFile() {
			string absPath = EditorUtility.OpenFilePanel("Select Config File", Application.dataPath, "asset");
			if (absPath.StartsWith(Application.dataPath)) {
				string relPath = absPath.Substring(Application.dataPath.Length - "Assets".Length);
				analyticsConfig = AssetDatabase.LoadAssetAtPath(relPath, typeof(Tinylytics_Config)) as Tinylytics_Config;
				//if (analyticsConfig.itemList == null)
				//	inventoryItemList.itemList = new List<InventoryItem>();
				if (analyticsConfig) {
					EditorPrefs.SetString("AnalyticsConfigObjectPath", relPath);
				}
			}
		}



}


	[CustomEditor(typeof(Tinylytics_Config))]
	public class Tinylytics_ConfigInspectorEditor : Editor {

		GUIStyle LinkStyle { get { return m_LinkStyle; } }
		[SerializeField] GUIStyle m_LinkStyle;

		GUIStyle TitleStyle { get { return m_TitleStyle; } }
		[SerializeField] GUIStyle m_TitleStyle;

		GUIStyle HeadingStyle { get { return m_HeadingStyle; } }
		[SerializeField] GUIStyle m_HeadingStyle;

		GUIStyle BodyStyle { get { return m_BodyStyle; } }
		[SerializeField] GUIStyle m_BodyStyle;


		public override void OnInspectorGUI() {
			var config = (Tinylytics_Config)target;

			m_BodyStyle = new GUIStyle(EditorStyles.label);
			m_BodyStyle.wordWrap = true;
			m_BodyStyle.fontSize = 14;

			m_TitleStyle = new GUIStyle(m_BodyStyle);
			m_TitleStyle.fontSize = 26;

			m_HeadingStyle = new GUIStyle(m_BodyStyle);
			m_HeadingStyle.fontSize = 18;

			GUILayout.Label("Deployment ID:", HeadingStyle);
			GUILayout.Label(config.deploymentID, BodyStyle);

		}

	}

}