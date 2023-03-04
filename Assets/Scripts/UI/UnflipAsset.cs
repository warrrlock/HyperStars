using System;
using UnityEngine;

namespace UI
{
    public class UnflipAsset: MonoBehaviour
    {
        private void Awake()
        {
            if (transform.parent.rotation.y == 0) return;
            RectTransform rect = GetComponent<RectTransform>();
            Vector3 position = rect.localPosition;
            rect.localPosition = new Vector3(position.x + rect.sizeDelta.x, position.y, position.z);

            transform.localRotation = transform.parent.rotation;
        }
    }
}