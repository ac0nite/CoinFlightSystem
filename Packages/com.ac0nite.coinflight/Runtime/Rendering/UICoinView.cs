using UnityEngine;
using UnityEngine.UI;

// ReSharper disable InconsistentNaming

namespace CoinFlight
{
    /// <summary>
    /// View-компонент на пулованном GameObject монеты. Кэширует ссылки,
    /// необходимые <see cref="RectTransformRenderer"/>, чтобы рендерер никогда
    /// не вызывал <c>GetComponent</c> в hot path.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class UICoinView : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _image;
        public RectTransform RectTransform => _rectTransform;
        public CanvasGroup CanvasGroup => _canvasGroup;

        public Sprite Sprite
        {
            get => _image.sprite;
            set => _image.sprite = value;
        }

        public int PrefabId { get; internal set; }
        public int PoolSlot { get; internal set; } = -1;

        private void Reset()
        {
            _rectTransform = (RectTransform) transform;
            _canvasGroup = GetComponent<CanvasGroup>();
            _image = GetComponent<Image>();
        }
        
        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (_rectTransform == null) _rectTransform = (RectTransform)transform;
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            if (_image == null) _image = GetComponent<Image>();
        }
        #endif
    }
}
