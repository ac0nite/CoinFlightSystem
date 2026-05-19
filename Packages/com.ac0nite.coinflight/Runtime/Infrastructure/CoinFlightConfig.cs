using UnityEngine;

// ReSharper disable InconsistentNaming

namespace CoinFlight
{
    /// <summary>
    ///     Настройки уровня конфигурации для <c>CoinFlightService</c>.
    ///     Выбор бекенда разрешается один раз при построении сервиса.
    /// </summary>
    [CreateAssetMenu(fileName = "CoinFlightConfig", menuName = "Coin Flight/Config", order = 0)]
    public sealed class CoinFlightConfig : ScriptableObject
    {
        [Header("Пул")]
        [Min(0)] public int PoolPrewarm = 256;
        [Min(1)] public int PoolMax = 1024;
        public OverflowPolicy OverflowPolicy = OverflowPolicy.DropNew;

        [Header("Канвас")]
        [Tooltip("Порядок сортировки выделенного Canvas для полёта монет. Должен быть выше HUD.")]
        public int CanvasSortingOrder = 100;

        [Header("Симуляция")]
        [Min(1)] public int MaxConcurrentTweens = 1024;

        [Header("По умолчанию")]
        [Min(0f)] public float DefaultDuration = 0.6f;
        public EasingType DefaultEasing = EasingType.OutCubic;
        public MotionPattern DefaultPattern = MotionPattern.Arc;

        [Header("Дуга / Безье")]
        [Tooltip("Высота дуги по умолчанию в пикселях (canvas local space).")]
        [Min(0f)] public float DefaultArcHeight = 120f;

        [Tooltip("Перпендикулярное смещение контрольной точки квадратичного Безье по умолчанию, в пикселях.")]
        public float DefaultBezierStrength = 80f;

        [Header("Разлёт -> Сборка")] 
        [Range(0f, 1f)] public float ScatterEndT = 0.35f;
        
        [Tooltip("Радиус разлёта для паттерна Scatter в пикселях (canvas local space).")]
        [Min(0f)]
        public float ScatterRadius = 120f;
    }
}