using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MainMenuUIManager: MonoBehaviour
    {
        [SerializeField] private Image _graphics;
        public void UpdateVisual(Sprite sprite)
        {
            if (_graphics) _graphics.sprite = sprite;
        }
    }
}