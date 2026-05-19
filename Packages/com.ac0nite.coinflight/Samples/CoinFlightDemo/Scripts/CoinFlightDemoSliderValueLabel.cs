using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable InconsistentNaming

namespace CoinFlight.Samples
{
    /// <summary>
    /// Обновляет TMP-текст текущим целым значением Slider, чтобы в демо-сцене
    /// было видно выбранное количество монет.
    /// </summary>
    public sealed class CoinFlightDemoSliderValueLabel : MonoBehaviour
    {
        [Tooltip("Slider, значение которого нужно отображать.")]
        public Slider Slider;

        [Tooltip("TMP-текст для отображения текущего значения.")]
        public TMP_Text Label;

        [Tooltip("Префикс перед числом, например: Coins: ")]
        public string Prefix = "Coins: ";

        private void OnEnable()
        {
            if (Slider != null)
                Slider.onValueChanged.AddListener(UpdateLabel);

            if (Slider != null)
                UpdateLabel(Slider.value);
        }

        private void OnDisable()
        {
            if (Slider != null)
                Slider.onValueChanged.RemoveListener(UpdateLabel);
        }

        private void UpdateLabel(float value)
        {
            if (Label == null) return;
            Label.text = $"{Prefix}{Mathf.RoundToInt(value)}";
        }
    }
}
