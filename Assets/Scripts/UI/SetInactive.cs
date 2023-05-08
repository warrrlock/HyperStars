using UnityEngine;

namespace UI
{
    public class SetInactive: MonoBehaviour
    {
        public void MakeInactive()
        {
            gameObject.SetActive(false);
        }
    }
}