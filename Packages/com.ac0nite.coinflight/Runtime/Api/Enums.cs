using System;

namespace CoinFlight
{
    public enum MotionPattern : byte
    {
        Linear = 0,
        Arc = 1,
        Bezier = 2,
        ScatterCollect = 3
    }

    public enum EasingType : byte
    {
        Linear = 0,
        InQuad = 1,
        OutQuad = 2,
        InOutQuad = 3,
        OutCubic = 4,
        OutBack = 5
    }

    public enum TimeMode : byte
    {
        Scaled = 0,
        Unscaled = 1
    }

    public enum CancelPolicy : byte
    {
        InstantHide = 0,
        SnapToTarget = 1
    }

    public enum OverflowPolicy : byte
    {
        DropNew = 0,
        RecycleOldest = 1,
        Expand = 2
    }

    public enum CoinEndpointKind : byte
    {
        WorldPosition = 0,
        WorldTransform = 1,
        RectTransform = 2
    }

    [Flags]
    public enum CoinVisualFlags : byte
    {
        None = 0,
        Visible = 1 << 0,
        Landed = 1 << 1,
        Cancelled = 1 << 2
    }
}
