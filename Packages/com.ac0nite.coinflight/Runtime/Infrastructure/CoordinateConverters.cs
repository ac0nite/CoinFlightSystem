using Unity.Mathematics;
using UnityEngine;

namespace CoinFlight
{
    /// <summary>
    /// Конвертирует endpoint-ы <see cref="CoinSource"/> / <see cref="CoinTarget"/>
    /// в local space flight-канваса (в пикселях). Предназначен для вызова один раз
    /// на endpoint во время <c>Play()</c> (snapshot) и никогда в hot path.
    /// Стратегия конвертации (план §4):
    ///   - <see cref="CoinEndpointKind.RectTransform"/>: world corners rect-а
    ///     -> screen point -> ScreenPointToLocalPointInRectangle на flight-канвасе;
    ///   - <see cref="CoinEndpointKind.WorldTransform"/> / <see cref="CoinEndpointKind.WorldPosition"/>:
    ///     world position -> <c>Camera.WorldToScreenPoint</c> -> ScreenPointToLocalPointInRectangle.
    /// </summary>
    public static class CoordinateConverters
    {
        public static float2 ToCanvasLocal(in CoinSource source, CoordinateContext context)
        {
            return Resolve(source.Kind, source.RectTransform, source.WorldTransform, source.WorldPosition, source.WorldCamera, context);
        }

        public static float2 ToCanvasLocal(in CoinTarget target, CoordinateContext context)
        {
            return Resolve(target.Kind, target.RectTransform, target.WorldTransform, target.WorldPosition, target.WorldCamera, context);
        }

        private static float2 Resolve(
            CoinEndpointKind kind,
            RectTransform rectTransform,
            Transform worldTransform,
            Vector3 worldPosition,
            Camera worldCamera,
            CoordinateContext context)
        {
            switch (kind)
            {
                case CoinEndpointKind.RectTransform:
                    return FromRectTransform(rectTransform, context);
                case CoinEndpointKind.WorldTransform:
                    if (worldTransform == null) return float2.zero;
                    return FromWorldPosition(worldTransform.position, worldCamera, context);
                case CoinEndpointKind.WorldPosition:
                    return FromWorldPosition(worldPosition, worldCamera, context);
                default:
                    return float2.zero;
            }
        }

        private static float2 FromRectTransform(RectTransform rect, CoordinateContext context)
        {
            if (rect == null) return float2.zero;

            // World-space center of the source rect.
            Vector3 worldCenter = rect.TransformPoint(rect.rect.center);

            // Project to screen using the source rect's own canvas camera.
            Camera sourceUICamera = FindUICameraFor(rect);
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(sourceUICamera, worldCenter);

            return ScreenToCanvasLocal(screenPoint, context);
        }

        private static float2 FromWorldPosition(Vector3 worldPosition, Camera worldCamera, CoordinateContext context)
        {
            if (worldCamera == null) return float2.zero;

            Vector3 screenPoint3 = worldCamera.WorldToScreenPoint(worldPosition);
            // screenPoint3.z < 0 means behind camera; callers can still place it but it's up to them.
            return ScreenToCanvasLocal(new Vector2(screenPoint3.x, screenPoint3.y), context);
        }

        private static float2 ScreenToCanvasLocal(Vector2 screenPoint, CoordinateContext context)
        {
            Camera canvasCamera = context.GetRaycastCamera();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                context.CanvasRect,
                screenPoint,
                canvasCamera,
                out Vector2 local);
            return new float2(local.x, local.y);
        }

        private static Camera FindUICameraFor(RectTransform rect)
        {
            var canvas = rect.GetComponentInParent<Canvas>();
            if (canvas == null) return null;
            var root = canvas.rootCanvas != null ? canvas.rootCanvas : canvas;
            return root.renderMode == RenderMode.ScreenSpaceOverlay ? null : root.worldCamera;
        }
    }
}
