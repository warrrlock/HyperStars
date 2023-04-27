using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class EventSystemSelector: MonoBehaviour
    {
        public void SelectGameObject(GameObject obj)
        {
            // Debug.Log($"set selected gameObject to {obj.name}, " +
            //           $"previous was {EventSystem.current.currentSelectedGameObject.name}");
            EventSystem.current.SetSelectedGameObject(obj);
        }
    }
}