using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class FavourMeter: MonoBehaviour
    {
        [Tooltip("Used for fill meter.")]
        [SerializeField] private Image _meterImage;

        [SerializeField] private TextMeshProUGUI _multiplierText;
        [Tooltip("Do not rotate indicator.")]
        [SerializeField] private Image _indicator; //TODO: remove for UI via peter
        private RectTransform _indicatorTransform;
        
        private RectTransform _p0IndicatorTransform;
        private RectTransform _p1IndicatorTransform;

        private float _length;

        public void Initialize()
        {
            _indicatorTransform = _indicator.rectTransform;
            Rect rect = _meterImage.rectTransform.rect;
            _length = Math.Max(rect.width, rect.height);
            _meterImage.fillAmount = 0.5f;
            UpdateFavorMeter(true, 0, 1);
        }

        public void UpdateFavorMeter(bool player0, float amount, float multiplier)
        {
            Debug.Log(amount * 0.5f);
            _multiplierText.text = $"x {multiplier}";
            IncrementFavour(player0, amount * 0.5f);
        }

        /// <summary>
        /// Increments the favour of a player ( 0 or 1).
        /// </summary>
        /// <param name="player0">true if meter is increasing in player0's favour</param>
        /// <param name="amount">percentage amount of total bar to increment, between 0 and 1.</param>
        private void IncrementFavour(bool player0, float amount)
        {
            int direction = player0 ? -1 : 1;
            _meterImage.fillAmount += (direction) * amount;
            Vector2 transformPosition = new Vector2(_length * _meterImage.fillAmount, 0);
            Debug.Log(transformPosition);
            _indicatorTransform.anchoredPosition = transformPosition;
            // _multiplierText.rectTransform.anchoredPosition += transformAmount;
        }
    }
}