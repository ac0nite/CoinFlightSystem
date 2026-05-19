# Coin Flight System — Iteration 1 Plan (v2)

Первая итерация: reusable Unity 6 + URP система полёта монет в режимах `UI → UI` и `World → UI`, построенная поверх PrimeTween как production backend, с разделёнными слоями Simulation / Rendering / Infrastructure и подготовкой к optional Jobs-backend во 2-й фазе.

Базовый документ: `/Users/andrey.sachuk/Documents/coin_flight_architecture_summary_ru.md`.

## 1. Scope iteration 1

**В скоупе:**
- Режимы: `UI → UI` и `World → UI`.
- Simulation backend: PrimeTween (единственный в этой итерации).
- Renderer: `RectTransformRenderer` (единственный в этой итерации).
- Motion patterns: Linear, Arc, Bezier (quadratic), Scatter → Collect.
- Easing: 6 функций (Linear, InQuad, OutQuad, InOutQuad, OutCubic, OutBack).
- Pooling: views + вспомогательные буферы.
- Lifecycle: destroyed target / scene unload / window close.
- Cancellation: handle-based + global Stop All.
- Backend switching: configure-time only через `CoinFlightConfigSO`.
- Target abstraction сразу (Vector3 / Transform / RectTransform).
- Демо-сцена + ручной profiler-прогон.

**Вне скоупа (Phase 2+):**
- Jobs/Burst simulation backend.
- `World → World`, `WorldTransformRenderer`, `CustomGraphicRenderer`, `InstancedRenderer`.
- Moving targets с коррекцией траектории.
- Homing / magnetic pattern.
- Automated performance tests.
- World-Space Canvas / multi-canvas edge cases.
- Runtime backend switching с draining.

## 2. Architecture

```
CoinFlightService  (orchestration, API, lifecycle, backend selection)
  ├── ICoinSimulationBackend
  │     └── PrimeTweenSimulation           <- Phase 1
  ├── ICoinRenderer
  │     └── RectTransformRenderer          <- Phase 1
  ├── Shared Math (Burst-ready, pure)
  │     ├── MotionPatternEvaluator
  │     ├── EasingEvaluator
  │     └── CoordinateConverters
  └── Infrastructure
        ├── UICoinPool
        ├── CoinFlightHandle / Cancellation
        ├── TargetBinding (Vector3 / Transform / RectTransform)
        └── Lifecycle (scene/window/destroy hooks)
```

- `MotionPatternEvaluator` и `EasingEvaluator` — pure math, Burst-compatible сигнатуры, без Unity-ссылок. Один код для PrimeTween- и будущего Jobs-backend.
- Один `Tween.Custom` на монету, evaluator считает весь transform-state; renderer применяет.

## 3. Public API

```csharp
readonly struct CoinFlightRequest {
    public CoinSource Source;
    public CoinTarget Target;
    public int Count;
    public float Duration;
    public float StartDelay;
    public float StaggerPerCoin;
    public MotionPattern Pattern;
    public EasingType Easing;
    public float PatternStrength;
    public int PrefabId;
    public uint RandomSeed;
    public TimeMode TimeMode;   // Scaled | Unscaled
}

CoinFlightHandle CoinFlightService.Play(in CoinFlightRequest request);
void CoinFlightService.Stop(CoinFlightHandle handle, CancelPolicy policy);
void CoinFlightService.StopAll(CancelPolicy policy);

enum CancelPolicy { InstantHide, SnapToTarget }
```

## 4. Coordinate handling

- Все монеты — children одного dedicated Canvas (sorting order выше HUD, без LayoutGroup / ContentSizeFitter).
- Source/Target конвертируются в canvas local space один раз на старте:
  - RectTransform -> world corners + ScreenPointToLocalPointInRectangle;
  - Transform/Vector3 -> world camera WorldToScreenPoint + ScreenPointToLocalPointInRectangle.
- Target — snapshot на момент старта в Phase 1.

## 5. Motion patterns

- Linear: lerp(p0, p1, eased(t)).
- Arc: linear + up * sin(PI * t) * height.
- Bezier (quadratic): control = midpoint + перпендикуляр * strength, с seed.
- Scatter -> Collect: двухфазный, scatterEnd ≈ 0.35.

Random — `Unity.Mathematics.Random` с seed на монету.

## 6. Pooling

- `UICoinPool`, prewarm 256, max configurable.
- Overflow: `OverflowPolicy { DropNew, RecycleOldest, Expand }`, default `DropNew`. `Expand` — opt-in, нарушает zero-alloc guarantee.
- Пулятся: views, массивы активных слотов, служебные структуры cancellation.
- Без per-coin material instancing.

## 7. Zero-allocation rules (PrimeTween-specific)

- Только non-capturing `Tween.Custom(target, start, end, duration, static (target, value) => ...)`.
- `OnComplete(target, static lambda)`.
- Prewarm checklist:
  - PrimeTweenConfig.SetTweensCapacity(N);
  - UICoinPool.Prewarm(capacity);
  - preallocated массивы состояний.
- TMP-счётчик: SetCharArray / SetText(StringBuilder).
- В hot path запрещено: new, LINQ, closures, coroutines, Camera.main, GetComponent, FindObjectOfType.
- Все data-структуры — readonly struct, передаются in.
- Time.timeScale == 0 -> pause; задокументировать.

Цель: no GC allocations during active animation playback after prewarm.

## 8. Lifecycle & Cancellation

- SceneManager.sceneUnloaded -> StopAll(InstantHide).
- Destroyed target -> fallback на последний snapshot + policy.
- CoinFlightHandle = id группы + generation counter.
- Cancellation общий интерфейс для PrimeTween и будущего Jobs-backend.

## 9. Demo scene

`Samples/CoinFlightDemo.unity`:
- HUD-счётчик (TMP, no-alloc).
- UI button + world object с 3D-камерой.
- Кнопки: Burst 10 / 100 / 500, Pattern switch, Mode switch.
- Overlay: FPS, active count, backend.

## 10. Profiling sanity check

- 50 / 100 / 500 × 4 pattern × 2 mode.
- Метрики: CPU main, GC Alloc per frame (цель 0), Canvas.SendWillRenderCanvases, Canvas.BuildBatch.
- Результаты — таблица в README.

## 11. Folder layout

```
Assets/CoinFlight/
  Runtime/
    Api/
    Simulation/
    Rendering/
    Math/
    Pool/
    Infrastructure/
    CoinFlight.Runtime.asmdef
  Samples/
    CoinFlightDemo/
    CoinFlight.Samples.asmdef
  Tests/
    Editor/
      CoinFlight.Tests.Editor.asmdef
```

Зависимости CoinFlight.Runtime: Unity.Mathematics, Unity.TextMeshPro, PrimeTween.

## 12. Deliverables

1. Unity 6 + URP проект с Assets/CoinFlight.
2. UI→UI и World→UI на PrimeTween + RectTransformRenderer, 4 patterns.
3. Handle-based cancellation + lifecycle hooks.
4. Демо-сцена с переключателями и HUD.
5. Подтверждённый zero-GC (Profiler screenshot + заметка).
6. README: API, пример, правила zero-alloc PrimeTween.

## 13. Execution order

1. Unity-проект + PrimeTween + Mathematics + asmdef.
2. Data layer: CoinFlightRequest, CoinMotionData, CoinSource/Target, enums, CoinFlightConfigSO.
3. EasingEvaluator + MotionPatternEvaluator (pure) + editor unit tests.
4. CoordinateConverters с кэшем Canvas/Camera.
5. UICoinPool + UICoinView + RectTransformRenderer.
6. HandleRegistry + LifecycleHub.
7. PrimeTweenSimulation через non-capturing Tween.Custom + OnComplete; prewarm.
8. CoinFlightService: Play/Stop/StopAll, выбор backend из config.
9. Демо-сцена + overlay.
10. Profiler-прогон.
11. Edge cases: count=0, duration=0, destroyed target, scene unload, timeScale=0.

## 14. Explicit contracts & constraints

Фиксируем недостающие контракты между слоями, чтобы сделать архитектуру implementation-ready.

### 14.1 CoinVisualState

Единственный контракт между Simulation и Rendering. Evaluator возвращает, renderer применяет.

```csharp
readonly struct CoinVisualState {
    public float2 Position;      // canvas local space (pixels)
    public float  RotationDeg;   // degrees, CCW
    public float  Scale;         // uniform
    public float  Alpha;         // 0..1
    public CoinVisualFlags Flags; // Visible | Landed | Cancelled
}
```

`MotionPatternEvaluator.Evaluate(in CoinMotionData data, float t) -> CoinVisualState`.

### 14.2 OverflowPolicy

```csharp
enum OverflowPolicy { DropNew, RecycleOldest, Expand }
```

- `DropNew` — default, zero-alloc guaranteed.
- `RecycleOldest` — мгновенно прячет старейшую активную монету, zero-alloc.
- `Expand` — аллоцирует, **opt-in**, ломает zero-alloc guarantee.

### 14.3 TimeMode

```csharp
enum TimeMode { Scaled, Unscaled }
```

- `Scaled` — default, уважает `Time.timeScale` (gameplay pickup на паузе замирает).
- `Unscaled` — для UI reward на pause-меню.
- Маппинг: PrimeTween `useUnscaledTime`; будущий Jobs — `Time.deltaTime` vs `Time.unscaledDeltaTime`.

### 14.4 Deterministic random contract

- `seedPerCoin = WangHash(request.RandomSeed, coinIndex)`.
- `Unity.Mathematics.Random` derive-per-coin, stateless.
- Same request seed => same trajectory (внутри одной платформы/билда).
- Random используется только для control offsets / scatter — **не** для landing.
- Cross-platform determinism **не** гарантируется (float math).

### 14.5 Canvas isolation rule (hard rule)

- `CoinFlightService` владеет dedicated `Canvas` с собственным `CanvasRenderer`, sorting order выше HUD.
- Никакие user-HUD элементы не являются children этого Canvas.
- Container RectTransform: full stretch, pivot (0.5, 0.5).
- Запрещены на container и его children: `LayoutGroup`, `ContentSizeFitter`, `AspectRatioFitter`, `GridLayoutGroup`.
- Цель: изолировать Canvas rebuild от основного HUD; это единственный способ сделать UI-анимацию дешёвой.

### 14.6 RectTransformRenderer scaling limits

Честный дисклеймер производительности:

- Sweet spot: до ~500 одновременных монет.
- 500–1000: работоспособно, но заметен рост `Canvas.BuildBatch` и overdraw.
- 1000+: требуется `InstancedRenderer` / `CommandBuffer` / custom `CanvasRenderer` batching (Phase 3).
- Причины: `RectTransform` writes dirty canvas, Canvas rebuild не параллелится, прозрачные `Image` линейно давят fill rate.

### 14.7 CoordinateConverters cache invalidation

Контракт кэша `CoordinateContext`:

- Поля кэша: `Canvas.renderMode`, `Canvas.worldCamera`, `CanvasScaler.scaleFactor`, `Screen.width/height`, `Screen.safeArea`.
- Invalidate triggers:
  - resolution change;
  - scale factor change;
  - render mode change;
  - worldCamera reassigned;
  - safe area / orientation change.
- Стратегия: `int Version` инкрементируется при invalidate. Pre-computed snapshots хранят version; при mismatch — один re-snapshot.
- Проверка: 1 раз за service tick (не per-coin), сравнение hash из (`width, height, scaleFactor`).
- Per-frame инвалидация запрещена.
- Без этого контракта монеты улетают не туда при ротации устройства / resolution change.

### 14.8 Pattern evaluator discipline

Contract для `MotionPatternEvaluator` (и `EasingEvaluator`) — самый нагруженный слой системы.

**Обязательные свойства:**

- **Pure** — без side effects, без чтения Unity API (`Time.*`, `UnityEngine.Random`, `Transform.*`, `Screen.*`), без логирования.
- **Deterministic** — одинаковый `(data, t)` => одинаковый результат (within platform/build).
- **Stateless** — всё состояние во входных данных; seed-based RNG через `WangHash(seed, coinIndex)`.
- **Branch-light** — верхнеуровневый `switch` по `MotionPattern`, внутри паттерна — прямолинейный math. Branch-light, не branch-free.
- **Allocation-free** — никаких managed allocations; no `params`, no `IEnumerable`, no boxing.
- **Burst-compatible** — компилируется Burst-ом без warning-ов; no managed types / virtual calls / UnityEngine.* (кроме разрешённых).

**Signature discipline:**

```csharp
public static CoinVisualState Evaluate(in CoinMotionData data, float t);
```

- `static`, `in`-параметр, value return — no copy / no alloc.

**Type discipline:**

- На hot path использовать только `Unity.Mathematics` (`float2`, `float3`, `math.*`).
- `UnityEngine.Vector2/3` и `Mathf` — **только** в `RectTransformRenderer` при записи в `RectTransform`.

**Monotonic t contract:**

- `t ∈ [0, 1]`, clamping — ответственность driver (Simulation backend), не evaluator.
- `t = 0` => позиция строго в source.
- `t = 1` => позиция строго в target (snap, **без** random/scatter offset).
- Evaluator **не** clamp-ит `t` сам.

**Pre-computation на `Play()`:**

`CoinMotionData` должна быть полностью самодостаточной и pre-computed:

- `StartPos`, `EndPos` (float2, canvas local);
- `ControlPoint` для Bezier (уже с perpendicular offset и seed-random);
- `ArcHeight`, `ArcAxis`;
- `ScatterPoint`, `ScatterEnd` для двухфазного паттерна;
- `PatternType`, `EasingType`;
- `Seed` (derived per-coin).

Всё, что можно посчитать один раз при `Play()`, считается при `Play()`. В hot path остаются только math-операции. Это одновременно ускоряет evaluator и упрощает будущую Burst-компиляцию.

## 15. Known risks / deferred

- Moving target с коррекцией траектории.
- Screen Space - Camera / World-Space Canvas edge cases.
- Runtime backend switch с drain.
- Jobs simulation backend + benchmarks.
- World → World + WorldTransformRenderer.
- Instanced/custom renderer для 1000+ монет.
- Automated performance test suite.
