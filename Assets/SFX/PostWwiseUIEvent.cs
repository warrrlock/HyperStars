using UnityEngine;

namespace SFX
{
    public class PostWwiseUIEvent: MonoBehaviour
    {
        public AK.Wwise.Event hover;
        public AK.Wwise.Event submit;
        public AK.Wwise.Event lockIn;
        
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
            lockIn?.Post(gameObject);
        }
    }
}