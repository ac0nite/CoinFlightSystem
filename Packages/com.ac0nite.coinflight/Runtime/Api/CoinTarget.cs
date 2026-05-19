using UnityEngine;

namespace CoinFlight
{
    /// <summary>
    /// Конечная точка полёта монеты. Размеченное объединение поверх
    /// Vector3 / Transform / RectTransform. Конвертируется в canvas local space
    /// один раз во время <c>Play()</c> (snapshot в Phase 1).
    /// </summary>
    public readonly struct CoinTarget
    {
        public readonly CoinEndpointKind Kind;
        public readonly Vector3 WorldPosition;
        public readonly Transform WorldTransform;
        public readonly RectTransform RectTransform;
        public readonly Camera WorldCamera;

        private CoinTarget(
            CoinEndpointKind kind,
            Vector3 worldPosition,
            Transform worldTransform,
            RectTransform rectTransform,
            Camera worldCamera)
        {
            Kind = kind;
            WorldPosition = worldPosition;
            WorldTransform = worldTransform;
            RectTransform = rectTransform;
            WorldCamera = worldCamera;
        }

        public static CoinTarget FromWorldPosition(Vector3 position, Camera worldCamera)
        {
            return new CoinTarget(CoinEndpointKind.WorldPosition, position, null, null, worldCamera);
        }

        public static CoinTarget FromTransform(Transform transform, Camera worldCamera)
        {
            return new CoinTarget(CoinEndpointKind.WorldTransform, default, transform, null, worldCamera);
        }

        public static CoinTarget FromRectTransform(RectTransform rectTransform)
        {
            return new CoinTarget(CoinEndpointKind.RectTransform, default, null, rectTransform, null);
        }
    }
}
