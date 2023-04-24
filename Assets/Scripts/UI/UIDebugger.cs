using UnityEngine;

namespace UI
{
    public class UIDebugger: MonoBehaviour
    {
        public void Debug(string s)
        {
            UnityEngine.Debug.Log(s);
        }
    }
}