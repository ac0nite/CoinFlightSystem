# Coin Flight Demo

Пример сборки демо-сцены для `Coin Flight System`. Включает скрипты:

- `CoinFlightDemoBootstrap` — создаёт `CoinFlightService`, публикует методы для кнопок.
- `CoinFlightDemoSpriteButton` — запускает UI → UI полёт со спрайтом из конкретной кнопки.
- `CoinFlightDemoWorldClick` — запускает World → UI полёт при клике по 3D-объекту.
- `CoinFlightDemoHud` — overlay с FPS / активными группами / активными монетами / паттерном.

## Как собрать сцену в Unity

### 1. Ассеты

1. **`CoinFlightConfig`**: `Create -> Coin Flight -> Config`. Настрой `PoolPrewarm` (256), `PoolMax` (1024), `MaxConcurrentTweens` (1024), дефолты по вкусу.
2. **Prefab монеты**: создай `UI -> Image` в сцене, добавь `CanvasGroup` и компонент `UICoinView`. Выставь нужный спрайт / размер (обычно 64×64). Перетащи в Project → получится prefab. Удали временный экземпляр из сцены.

### 2. Canvas'ы

- **HUD Canvas** (Screen Space Overlay, sortingOrder=0): Slider `Count` (0–500), три кнопки со спрайтами монет, Dropdown `Pattern`, кнопка `Stop`, Text overlay. Плюс `Image` `Coin Source` (квадратик в левом-верхнем углу) и `Image` `Coin Target` (иконка в правом-верхнем углу).
- **Flight Canvas** (Screen Space Overlay, sortingOrder=100): пустой; будет заполняться монетами. Важно: **нет** LayoutGroup / ContentSizeFitter на нём и его детях.

### 3. World-сцена (для World → UI)

- Камера `Main Camera` (Perspective).
- Любой GameObject в мире как `WorldSource` (например, куб на позиции (0, 0, 0)).

### 4. Bootstrap

Создай пустой GameObject `CoinFlightDemo`, добавь `CoinFlightDemoBootstrap`. В инспекторе назначь:
- `Config` → твой `CoinFlightConfig` asset.
- `Flight Canvas` → твой Flight Canvas.
- `Coin Prefab` → prefab монеты.
- `Ui Source` / `Ui Target` → RectTransform-ы Image-ов на HUD.
- `World Source` → Transform мирового объекта.
- `Gameplay Camera` → Main Camera.
- `Pattern Dropdown` → TMP_Dropdown выбора паттерна.
- `Count Slider` → Slider количества монет.

Дефолты (`Duration`, `Pattern`, `Easing`, `PatternStrength`, `StaggerPerCoin`) подкрути в инспекторе.

### 5. Slider, кнопки и 3D-click

Slider `Count`:
- Min Value = `0`.
- Max Value = `500`.
- Whole Numbers = `true`.
- В `CoinFlightDemoBootstrap.CountSlider` назначь этот Slider. Ручной `OnValueChanged` в инспекторе не нужен.

Для каждой из трёх sprite-кнопок:
- Добавь на кнопку компонент `CoinFlightDemoSpriteButton`.
- `Bootstrap` → объект `CoinFlightDemo`.
- `Sprite Source` → Image этой кнопки.
- В `Button.OnClick()` вызови `CoinFlightDemoSpriteButton.Play()`.

Для клика по 3D-объекту:
- На любой объект сцены добавь `CoinFlightDemoWorldClick`.
- `Bootstrap` → объект `CoinFlightDemo`.
- `Raycast Camera` → Main Camera.
- `Click Mask` → слой интерактивных 3D-объектов.
- `Random Sprites` → массив из трёх или более спрайтов.
- На 3D-объекте должен быть Collider.

Кнопка "Stop" → `CoinFlightDemoBootstrap.StopAll()`.

Dropdown "Pattern":
- В `CoinFlightDemoBootstrap.PatternDropdown` назначь TMP_Dropdown.
- Ручной `OnValueChanged` в инспекторе не нужен.
Значения: `0=Linear, 1=Arc, 2=Bezier, 3=ScatterCollect`.

### 6. HUD

На HUD Canvas добавь Text, на него повесь `CoinFlightDemoHud`. Назначь:
- `Bootstrap` → `CoinFlightDemo`.
- `Overlay Text` → сам Text.

### 7. Запуск

Play Mode → выбери количество на Slider → нажми одну из трёх sprite-кнопок. Монеты полетят из UI source в UI target со спрайтом этой кнопки. Клик по 3D-объекту запустит World → UI полёт с рандомными спрайтами.

## Профилирование

- Window → Analysis → Profiler.
- Записывай кадр, смотри `CPU Usage -> GC.Alloc` (цель 0 после prewarm), `Canvas.SendWillRenderCanvases`, `Canvas.BuildBatch`.
- Для 500 монет и 4 паттернов пройди все комбинации, зафиксируй цифры в README проекта.
