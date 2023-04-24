using System;
using UnityEngine;

namespace Tinylytics {
    [Serializable]
    public abstract class TrackableProperty
    {
        [SerializeField]
        protected UnityEngine.Object m_Target;
        [SerializeField]
        protected string m_Path;
    }
}
