using UnityEngine;
using UnityEngine.UI;

// ReSharper disable InconsistentNaming

namespace CoinFlight.Samples
{
    /// <summary>
    /// UI-кнопка демо-сцены, запускающая UI → UI полёт с количеством из slider
    /// и спрайтом, взятым из указанного Image.
    /// </summary>
    public sealed class CoinFlightDemoSpriteButton : MonoBehaviour
    {
        [Tooltip("Ссылка на bootstrap, управляющий сервисом.")]
        public CoinFlightDemoBootstrap Bootstrap;

        [Tooltip("Image, из которого берётся спрайт монеты для этого типа кнопки.")]
        public Image SpriteSource;

        [SerializeField] private RectTransform _rectTransform;

        public void Play()
        {
            if (Bootstrap == null) return;
            Bootstrap.PlayUiToUiWithCurrentCount(_rectTransform, SpriteSource != null ? SpriteSource.sprite : null);
        }

        private void Reset()
        {
            _rectTransform = (RectTransform)transform;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_rectTransform == null) _rectTransform = (RectTransform)transform;
        }
#endif
    }
}
