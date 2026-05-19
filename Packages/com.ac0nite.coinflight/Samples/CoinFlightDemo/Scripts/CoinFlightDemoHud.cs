using UnityEngine;
using UnityEngine.UI;

// ReSharper disable InconsistentNaming

namespace CoinFlight.Samples
{
    /// <summary>
    /// Минимальный HUD: FPS, количество активных групп, количество живых монет в пуле,
    /// активный паттерн / бекенд. Пишет в переданный UI Text (UGUI) — без TMP-зависимости.
    /// </summary>
    public sealed class CoinFlightDemoHud : MonoBehaviour
    {
        [Tooltip("Ссылка на bootstrap, управляющий сервисом.")]
        public CoinFlightDemoBootstrap Bootstrap;

        [Tooltip("UI Text, в который выводится overlay.")]
        public Text OverlayText;

        [Min(0.1f)] public float UpdateInterval = 0.25f;

        private float _timer;
        private float _smoothedFps;

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            if (dt > 0f)
            {
                float instantaneousFps = 1f / dt;
                _smoothedFps = _smoothedFps <= 0f
                    ? instantaneousFps
                    : Mathf.Lerp(_smoothedFps, instantaneousFps, 0.1f);
            }

            _timer += dt;
            if (_timer < UpdateInterval) return;
            _timer = 0f;

            if (OverlayText == null || Bootstrap == null) return;
            var service = Bootstrap.Service;
            int active = service != null ? service.ActiveGroupCount : 0;
            int poolActive = service != null ? service.Pool.ActiveCount : 0;
            int poolCap = service != null ? service.Pool.Capacity : 0;

            OverlayText.text =
                $"FPS: {_smoothedFps:F0}\n" +
                $"Groups: {active}\n" +
                $"Coins: {poolActive} / {poolCap}\n" +
                $"Pattern: {Bootstrap.Pattern}\n" +
                $"Backend: PrimeTween";
        }
    }
}
