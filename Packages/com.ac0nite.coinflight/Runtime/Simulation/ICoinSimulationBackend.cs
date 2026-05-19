namespace CoinFlight
{
    /// <summary>
    /// Подключаемый бекенд симуляции. Бекенд управляет нормализованным
    /// временем <c>t</c> для каждой монеты, вызывает <see cref="MotionPatternEvaluator"/>
    /// для получения <see cref="CoinVisualState"/> и применяет его через переданный
    /// <see cref="ICoinRenderer"/>.
    /// Phase 1 поставляется с <c>PrimeTweenSimulation</c>; Phase 2+ может добавить
    /// Jobs/Burst backend.
    /// </summary>
    public interface ICoinSimulationBackend
    {
        /// <summary>
        /// Прогревает внутренние структуры данных (например,
        /// <c>PrimeTweenConfig.SetTweensCapacity</c>), чтобы runtime-вызовы
        /// <c>StartGroup</c> не аллоцировали память.
        /// </summary>
        void Prewarm(int tweenCapacity);

        /// <summary>
        /// Запускает группу монет по уже предвычисленным per-coin данным
        /// движения и уже взятым из пула views / слотам. Возвращает
        /// <see cref="ICoinFlightGroup"/>, который сервис может остановить через
        /// <see cref="HandleRegistry"/>.
        /// </summary>
        ICoinFlightGroup StartGroup(
            CoinFlightHandle handle,
            in CoinFlightRequest request,
            CoinMotionData[] motionData,
            UICoinView[] views,
            int[] poolSlots,
            int coinCount,
            ICoinRenderer renderer,
            UICoinPool pool,
            HandleRegistry registry);
    }
}
