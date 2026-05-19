namespace CoinFlight
{
    /// <summary>
    /// Применяет <see cref="CoinVisualState"/> к пулованному
    /// <see cref="UICoinView"/>. Stateless-контракт: рендерер не владеет
    /// симуляционным состоянием и должен быть безопасен при многократном
    /// вызове за кадр из бекенда симуляции.
    /// </summary>
    public interface ICoinRenderer
    {
        void Apply(UICoinView view, in CoinVisualState state);
    }
}
