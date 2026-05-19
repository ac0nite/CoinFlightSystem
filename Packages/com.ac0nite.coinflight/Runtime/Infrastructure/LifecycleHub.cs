using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoinFlight
{
    /// <summary>
    /// Подписывается на lifecycle-события Unity, которые должны приводить
    /// к глобальной остановке активных групп монет (план §8):
    ///   - <see cref="SceneManager.sceneUnloaded"/> — предотвращает повисшие
    ///     ссылки после выгрузки сцены;
    ///   - <see cref="Application.quitting"/> — чистое завершение при выходе.
    /// Для отписки вызвать Dispose.
    /// </summary>
    public sealed class LifecycleHub : IDisposable
    {
        private readonly Action<CancelPolicy> _stopAll;
        private bool _disposed;

        public LifecycleHub(Action<CancelPolicy> stopAll)
        {
            _stopAll = stopAll ?? throw new ArgumentNullException(nameof(stopAll));
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            Application.quitting += OnApplicationQuitting;
        }

        private void OnSceneUnloaded(Scene scene)
        {
            _stopAll?.Invoke(CancelPolicy.InstantHide);
        }

        private void OnApplicationQuitting()
        {
            _stopAll?.Invoke(CancelPolicy.InstantHide);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            Application.quitting -= OnApplicationQuitting;
        }
    }
}
