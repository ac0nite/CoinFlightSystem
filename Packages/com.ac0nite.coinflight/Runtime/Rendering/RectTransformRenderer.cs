using UnityEngine;

namespace CoinFlight
{
    /// <summary>
    /// Рендерер Phase 1: пишет <see cref="CoinVisualState"/> в
    /// <see cref="RectTransform"/> и <see cref="CanvasGroup"/>. Профиль
    /// производительности (план §14.6):
    ///   - sweet spot: до ~500 одновременных монет;
    ///   - 500–1000: работает, но заметна стоимость Canvas.BuildBatch / overdraw;
    ///   - 1000+: требуется instanced-рендерер (Phase 3).
    /// </summary>
    public sealed class RectTransformRenderer : ICoinRenderer
    {
        public void Apply(UICoinView view, in CoinVisualState state)
        {
            var rt = view.RectTransform;
            rt.anchoredPosition = new Vector2(state.Position.x, state.Position.y);

            if (state.RotationDeg != 0f)
                rt.localRotation = Quaternion.Euler(0f, 0f, state.RotationDeg);

            float s = state.Scale;
            rt.localScale = new Vector3(s, s, 1f);

            view.CanvasGroup.alpha = state.Alpha;
        }
    }
}
