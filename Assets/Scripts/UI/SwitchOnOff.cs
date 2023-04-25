using UnityEngine;

namespace UI
{
    public class SwitchOnOff: MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        private bool _on;
        private static readonly int On = Animator.StringToHash("On");

        public void Switch(bool on)
        {
            _on = on;
            SetSwitch();
        }

        public void Toggle()
        {
            // Debug.Log($"{name} toggling");
            _on = !_on;
            SetSwitch();
        }

        private void SetSwitch()
        {
            _animator.SetBool(On, _on);
        }
    }
}