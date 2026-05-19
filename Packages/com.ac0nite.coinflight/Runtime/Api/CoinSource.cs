using UnityEngine;

namespace CoinFlight
{
    /// <summary>
    /// Исходная точка полёта монеты. Размеченное объединение поверх
    /// Vector3 / Transform / RectTransform. Конвертируется в canvas local space
    /// один раз во время <c>Play()</c>.
    /// </summary>
    public readonly struct CoinSource
    {
        public readonly CoinEndpointKind Kind;
        public readonly Vector3 WorldPosition;
        public readonly Transform WorldTransform;
        public readonly RectTransform RectTransform;
        public readonly Camera WorldCamera;

        private CoinSource(
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

        public static CoinSource FromWorldPosition(Vector3 position, Camera worldCamera)
        {
            return new CoinSource(CoinEndpointKind.WorldPosition, position, null, null, worldCamera);
        }

        public static CoinSource FromTransform(Transform transform, Camera worldCamera)
        {
            return new CoinSource(CoinEndpointKind.WorldTransform, default, transform, null, worldCamera);
        }

        public static CoinSource FromRectTransform(RectTransform rectTransform)
        {
            return new CoinSource(CoinEndpointKind.RectTransform, default, null, rectTransform, null);
        }
    }
}
