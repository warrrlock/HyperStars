using UnityEngine;

namespace SFX
{
    public class PostWwiseUIEvent: MonoBehaviour
    {
        public AK.Wwise.Event hover;
        public AK.Wwise.Event submit;
        public AK.Wwise.Event lockIn;
        public AK.Wwise.Event exit;
        [HideInInspector] public AK.Wwise.Switch characterSwitch;

        public void PostHover()
        {
            hover?.Post(gameObject);
        }
        
        public void PostSubmit()
        {
            submit?.Post(gameObject);
        }

        public void PostLockIn()
        {
            characterSwitch.SetValue(gameObject);
            lockIn?.Post(gameObject);
        }
        
        public void PostExit()
        {
            exit?.Post(gameObject);
        }
    }
}