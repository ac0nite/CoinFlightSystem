# Coin Flight System

Переиспользуемая система полёта монет (reward feedback) для Unity, оформленная как UPM-пакет `com.ac0nite.coinflight`. Поддерживает полёты UI → UI и World → UI с настраиваемыми паттернами движения.

## Требования

- Unity 6000.0+
- Зависимости (подтягиваются автоматически): Unity.Mathematics, uGUI, PrimeTween

## Установка

Через Package Manager → Add package from git URL:

```
https://github.com/ac0nite/CoinFlightSystem.git?path=Packages/com.ac0nite.coinflight
```

## Архитектура

Пакет разбит на слои с направлением зависимостей внутрь:

- `Api`: публичный контракт (`CoinFlightService`, `CoinFlightRequest`, `CoinFlightHandle`, `CoinSource`/`CoinTarget`).
- `Simulation`: backend симуляции движения на PrimeTween (`ICoinSimulationBackend`, `MotionDataBuilder`).
- `Math`: easing и motion patterns, детерминированный рандом (Wang hash).
- `Pool`: пул UI-монет без аллокаций в рантайме.
- `Rendering`: отрисовка (`UICoinView`, `RectTransformRenderer`, `ICoinRenderer`).
- `Infrastructure`: конфиг, конвертация координат world/UI, реестр хэндлов, lifecycle.

## Демо

Сэмпл `Coin Flight Demo`: сцена с HUD, переключением паттернов, кейсы UI-UI и World-UI. Импортируется через Package Manager → Samples.

## Тесты

Editor-тесты: motion patterns, easing, Wang hash, реестр хэндлов.
