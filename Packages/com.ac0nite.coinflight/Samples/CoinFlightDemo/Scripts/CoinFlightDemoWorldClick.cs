using UnityEngine;

// ReSharper disable InconsistentNaming

namespace CoinFlight.Samples
{
    /// <summary>
    /// Обрабатывает клик / тап по 3D-объектам через Physics.Raycast и запускает
    /// World → UI полёт с рандомным выбором спрайтов из заданного массива.
    /// </summary>
    public sealed class CoinFlightDemoWorldClick : MonoBehaviour
    {
        [Tooltip("Ссылка на bootstrap, управляющий сервисом.")]
        public CoinFlightDemoBootstrap Bootstrap;

        [Tooltip("Камера, из которой выполняется raycast. Если не задана, используется Bootstrap.GameplayCamera.")]
        public Camera RaycastCamera;

        [Tooltip("Слой 3D-объектов, по которым разрешён клик.")]
        public LayerMask ClickMask = ~0;

        [Tooltip("Спрайты, которые будут случайно назначаться монетам из world-click.")]
        public Sprite[] RandomSprites;

        [Min(0f)] public float MaxDistance = 1000f;

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0)) return;
            if (Bootstrap == null) return;

            var camera = RaycastCamera != null ? RaycastCamera : Bootstrap.GameplayCamera;
            if (camera == null) return;

            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, MaxDistance, ClickMask, QueryTriggerInteraction.Ignore))
                Bootstrap.PlayWorldFromPosition(hit.point, RandomSprites);
        }
    }
}
