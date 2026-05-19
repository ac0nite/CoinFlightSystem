namespace CoinFlight
{
    /// <summary>
    /// Backend-независимое представление работающей группы монет, выданной одним
    /// вызовом <c>CoinFlightService.Play()</c>. Владеет своими tween-ами / coin-слотами
    /// и отвечает за их освобождение при <see cref="Stop"/>.
    /// </summary>
    public interface ICoinFlightGroup
    {
        bool IsAlive { get; }
        int CoinCount { get; }
        void Stop(CancelPolicy policy);
    }
}
