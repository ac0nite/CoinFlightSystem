using CoinFlight;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable InconsistentNaming

namespace CoinFlight.Samples
{
    /// <summary>
    /// Создаёт <see cref="CoinFlightService"/> в рантайме и публикует
    /// высокоуровневые демо-действия для UI-кнопок и других скриптов.
    /// Повесьте на любой GameObject демо-сцены.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class CoinFlightDemoBootstrap : MonoBehaviour
    {
        [Header("Обязательные зависимости")]
        [Tooltip("ScriptableObject с настройками пула / канваса / симуляции.")]
        public CoinFlightConfig Config;

        [Tooltip("Выделенный Canvas, на который будут лететь монеты.")]
        public Canvas FlightCanvas;

        [Tooltip("Prefab с UICoinView + Image + CanvasGroup.")]
        public UICoinView CoinPrefab;

        [Header("UI → UI")]
        public RectTransform UiTarget;

        [Header("World → UI")]
        public Transform WorldSource;
        public Camera GameplayCamera;

        [Header("Параметры по умолчанию")]
        [Range(0, 500)] public int CoinCount = 100;
        [Min(0f)] public float Duration = 0.8f;
        [Min(0f)] public float StaggerPerCoin = 0.02f;
        [Min(0f)] public float PatternStrength = 1f;
        public MotionPattern Pattern = MotionPattern.Arc;
        public EasingType Easing = EasingType.OutCubic;
        public TimeMode TimeMode = TimeMode.Scaled;
        
        [Tooltip("Dropdown для выбора паттерна (опционально, биндит SetPattern в runtime).")]
        public TMP_Dropdown PatternDropdown;

        [Tooltip("Slider для выбора количества монет от 0 до 500.")]
        public Slider CountSlider;

        public CoinFlightService Service { get; private set; }

        private uint _seedCounter = 1u;

        private void Awake()
        {
            if (Config == null || FlightCanvas == null || CoinPrefab == null)
            {
                Debug.LogError("[CoinFlightDemo] Не заданы обязательные зависимости.", this);
                enabled = false;
                return;
            }

            Service = new CoinFlightService(Config, FlightCanvas, CoinPrefab);
            
            Application.targetFrameRate = -1;
            QualitySettings.vSyncCount = 0;
        }

        private void Start()
        {
            if (PatternDropdown != null)
            {
                PatternDropdown.onValueChanged.AddListener(SetPattern);
                SetPattern(PatternDropdown.value);
            }

            if (CountSlider != null)
            {
                CountSlider.minValue = 0f;
                CountSlider.maxValue = 500f;
                CountSlider.wholeNumbers = true;
                CountSlider.onValueChanged.AddListener(SetCoinCount);
                SetCoinCount(CountSlider.value);
            }
        }

        private void OnDestroy()
        {
            if (PatternDropdown != null)
                PatternDropdown.onValueChanged.RemoveListener(SetPattern);

            if (CountSlider != null)
                CountSlider.onValueChanged.RemoveListener(SetCoinCount);

            Service?.Dispose();
            Service = null;
        }

        // ---- UI actions ----

        public void PlayUiToUiWithCurrentCount(RectTransform source, Sprite sprite)
        {
            PlayUiToUi(source, CoinCount, sprite);
        }

        public void PlayWorldFromPosition(Vector3 worldPosition, Sprite[] randomSprites)
        {
            if (Service == null) return;
            if (UiTarget == null || GameplayCamera == null)
            {
                Debug.LogWarning("[CoinFlightDemo] UI Target / Camera не заданы.", this);
                return;
            }

            var request = BuildRequest(
                CoinSource.FromWorldPosition(worldPosition, GameplayCamera),
                CoinTarget.FromRectTransform(UiTarget),
                CoinCount,
                null,
                randomSprites);
            Service.Play(in request);
        }

        private void PlayUiToUi(RectTransform source, int count, Sprite sprite)
        {
            if (Service == null) return;
            if (source == null || UiTarget == null)
            {
                Debug.LogWarning("[CoinFlightDemo] UI Source/Target не заданы.", this);
                return;
            }

            var request = BuildRequest(
                CoinSource.FromRectTransform(source),
                CoinTarget.FromRectTransform(UiTarget),
                count,
                sprite,
                null);
            Service.Play(in request);
        }

        public void PlayWorldToUi(int count)
        {
            if (Service == null) return;
            if (WorldSource == null || UiTarget == null || GameplayCamera == null)
            {
                Debug.LogWarning("[CoinFlightDemo] World Source / UI Target / Camera не заданы.", this);
                return;
            }

            var request = BuildRequest(
                CoinSource.FromTransform(WorldSource, GameplayCamera),
                CoinTarget.FromRectTransform(UiTarget),
                count,
                null,
                null);
            Service.Play(in request);
        }

        public void StopAll()
        {
            Service?.StopAll(CancelPolicy.InstantHide);
        }

        public void SetPattern(int patternIndex)
        {
            Pattern = (MotionPattern)Mathf.Clamp(patternIndex, 0, 3);
        }

        public void SetCoinCount(float count)
        {
            CoinCount = Mathf.Clamp(Mathf.RoundToInt(count), 0, 500);
        }

        private CoinFlightRequest BuildRequest(CoinSource src, CoinTarget tgt, int count, Sprite sprite, Sprite[] randomSprites)
        {
            return new CoinFlightRequest(
                source: src,
                target: tgt,
                count: Mathf.Clamp(count, 0, 500),
                duration: Duration,
                pattern: Pattern,
                easing: Easing,
                patternStrength: PatternStrength,
                staggerPerCoin: StaggerPerCoin,
                sprite: sprite,
                randomSprites: randomSprites,
                randomSeed: _seedCounter++,
                timeMode: TimeMode);
        }
    }
}
