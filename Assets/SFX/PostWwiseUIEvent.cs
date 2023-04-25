using UnityEngine;

namespace SFX
{
    public class PostWwiseUIEvent: MonoBehaviour
    {
        public AK.Wwise.Event hover;
        public AK.Wwise.Event submit;

        public void PostHover()
        {
            hover?.Post(gameObject);
        }
        
        public void PostSubmit()
        {
            submit?.Post(gameObject);
        }
    }
}