using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AnimatedValues;
#endif
using System.Collections.Generic;
using System;
//using System.Reflection;
//using System.Xml.Linq;
//using System.Linq;
//using System.IO;

namespace Tinylytics {
	public class Tinylytics_About : EditorWindow {
		private static readonly Vector2 _WinSize = new Vector2(400f, 600f);
		private const string TITLE = "Tinylytics";
		private const string MENU_ITEM = "Window/Tinylytics/About";

	

		public void Awake() {
			base.titleContent.text = TITLE;

			this.minSize = new Vector2(400, 800);
		}

		public void OnEnable() {


		}

		protected void OnGUI() {
			GUI.skin.label.richText = true;

			EditorGUILayout.BeginHorizontal();
			// About us
			GUILayout.Label("About Info Here!");
			EditorGUILayout.EndHorizontal();
			// Special thanks
			GUILayout.Label(" ");

			GUILayout.Space(16f);
			EditorGUILayout.BeginHorizontal();

			EditorGUILayout.EndHorizontal();


		}


		public static void Open() {
			EditorWindow window = EditorWindow.GetWindow<Tinylytics_About>(true, "Tinylytics About", true);
			((EditorWindow)window).minSize = Tinylytics_About._WinSize;
			((EditorWindow)window).maxSize = Tinylytics_About._WinSize;
			((EditorWindow)window).ShowUtility();
		}

		[MenuItem(MENU_ITEM, false, 100)]
		private static void ShowWindow() {
			//Tinylytics_About welcomeWindow = (Tinylytics_About)EditorWindow.GetWindow(typeof(Tinylytics_About));
			Tinylytics_About.Open();
		}
	}


}