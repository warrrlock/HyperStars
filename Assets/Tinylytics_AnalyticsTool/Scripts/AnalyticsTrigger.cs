using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Tinylytics {
	[Serializable]
	public class AnalyticsTrigger {

		// Properties for Lifecycle
		[SerializeField]
		public TriggerEvent triggerEvent;

		//// Delegate for triggering when using properties
		//internal delegate void OnTrigger();
		//OnTrigger triggerFunction;

		//[SerializeField]
		//TriggerMethod m_Method;

	}


	[Serializable]
	public enum TriggerEvent {
		None = 0,
		Awake,
		Start,
		OnEnable,
		OnDisable,
		OnDestroy,
		OnSceneUnloaded,
		OnApplicationQuit
	}
}