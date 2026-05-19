using System;
using UnityEngine;

namespace CoinFlight
{
    /// <summary>
    /// Пул фиксированной ёмкости для экземпляров <see cref="UICoinView"/>,
    /// запаренченных под выделенный flight-канвас. Модель аллокаций:
    ///   - массивы, открытые на конструкторе до размера <c>max</c>;
    ///   - free list (LIFO) и active list (FIFO через двусвязный список индексов);
    ///   - zero-alloc <c>TryAcquire</c> / <c>Release</c> после <c>Prewarm</c>.
    /// Overflow (план §14.2):
    ///   - <see cref="OverflowPolicy.DropNew"/>: новый acquire молча неуспешен;
    ///   - <see cref="OverflowPolicy.RecycleOldest"/>: вытесняется самый старый активный слот;
    ///   - <see cref="OverflowPolicy.Expand"/>: массивы ресайзятся (opt-in, аллоцирует).
    /// </summary>
    public sealed class UICoinPool
    {
        private readonly UICoinView _prefab;
        private readonly RectTransform _parent;
        private readonly OverflowPolicy _overflow;

        private UICoinView[] _views;
        private int[] _free;
        private int _freeCount;

        // Doubly-linked active list over slot indices. -1 = no link.
        private int[] _prev;
        private int[] _next;
        private int _head = -1; // oldest
        private int _tail = -1; // newest
        private int _activeCount;

        private int _capacity;
        private int _max;

        public int Capacity => _capacity;
        public int ActiveCount => _activeCount;
        public int Max => _max;
        public OverflowPolicy Overflow => _overflow;

        public UICoinPool(UICoinView prefab, RectTransform parent, int prewarm, int max, OverflowPolicy overflow)
        {
            if (prefab == null) throw new ArgumentNullException(nameof(prefab));
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (max < 1) throw new ArgumentOutOfRangeException(nameof(max));
            if (prewarm < 0) prewarm = 0;
            if (prewarm > max) prewarm = max;

            _prefab = prefab;
            _parent = parent;
            _overflow = overflow;
            _max = max;

            _views = new UICoinView[max];
            _free = new int[max];
            _prev = new int[max];
            _next = new int[max];
            for (int i = 0; i < max; i++)
            {
                _prev[i] = -1;
                _next[i] = -1;
            }

            for (int i = 0; i < prewarm; i++)
                AllocateNewSlot();
        }

        public bool TryAcquire(out int slot, out UICoinView view)
        {
            if (_freeCount > 0)
            {
                slot = _free[--_freeCount];
            }
            else if (_capacity < _max)
            {
                slot = AllocateNewSlot();
                _freeCount--; // AllocateNewSlot pushes to free; consume it
            }
            else
            {
                switch (_overflow)
                {
                    case OverflowPolicy.DropNew:
                        slot = -1;
                        view = null;
                        return false;

                    case OverflowPolicy.RecycleOldest:
                        if (_head < 0)
                        {
                            slot = -1;
                            view = null;
                            return false;
                        }
                        int oldest = _head;
                        Release(oldest);
                        slot = _free[--_freeCount];
                        break;

                    case OverflowPolicy.Expand:
                        Grow(_max * 2);
                        slot = AllocateNewSlot();
                        _freeCount--;
                        break;

                    default:
                        slot = -1;
                        view = null;
                        return false;
                }
            }

            view = _views[slot];
            SetVisible(view, true);
            view.PoolSlot = slot;
            AppendActive(slot);
            return true;
        }

        public void Release(int slot)
        {
            if (slot < 0 || slot >= _capacity) return;
            var view = _views[slot];
            if (view == null) return;
            if (view.PoolSlot != slot) return; // already released

            view.PoolSlot = -1;
            SetVisible(view, false);

            RemoveActive(slot);
            _free[_freeCount++] = slot;
        }

        public void ReleaseAll()
        {
            while (_head >= 0)
                Release(_head);
        }

        private int AllocateNewSlot()
        {
            int slot = _capacity;
            var go = UnityEngine.Object.Instantiate(_prefab, _parent, false);
            var gameObject = go.gameObject;
            gameObject.SetActive(true);
            SetVisible(go, false);
            go.PoolSlot = -1;
            _views[slot] = go;
            _free[_freeCount++] = slot;
            _capacity++;
            return slot;
        }

        private static void SetVisible(UICoinView view, bool visible)
        {
            var canvasGroup = view.CanvasGroup;
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        private void AppendActive(int slot)
        {
            _prev[slot] = _tail;
            _next[slot] = -1;
            if (_tail >= 0) _next[_tail] = slot;
            else _head = slot;
            _tail = slot;
            _activeCount++;
        }

        private void RemoveActive(int slot)
        {
            int p = _prev[slot];
            int n = _next[slot];
            if (p >= 0) _next[p] = n; else _head = n;
            if (n >= 0) _prev[n] = p; else _tail = p;
            _prev[slot] = -1;
            _next[slot] = -1;
            _activeCount--;
        }

        private void Grow(int newMax)
        {
            if (newMax <= _max) return;
            Array.Resize(ref _views, newMax);
            Array.Resize(ref _free, newMax);
            Array.Resize(ref _prev, newMax);
            Array.Resize(ref _next, newMax);
            for (int i = _max; i < newMax; i++)
            {
                _prev[i] = -1;
                _next[i] = -1;
            }
            _max = newMax;
        }
    }
}
