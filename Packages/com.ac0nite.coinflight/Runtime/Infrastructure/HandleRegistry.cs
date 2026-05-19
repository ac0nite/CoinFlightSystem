using System;

namespace CoinFlight
{
    /// <summary>
    /// Выдаёт и валидирует значения <see cref="CoinFlightHandle"/>. Использует
    /// пару (id, generation): id — индекс слота (начиная с 1) во внутренних
    /// массивах; generation инкрементируется на каждый <see cref="Unregister"/>,
    /// чтобы устаревшие хэндлы не могли случайно адресовать повторно используемые слоты.
    /// Модель аллокаций: fixed-capacity массивы, открытые на конструкторе;
    /// нулевые аллокации после <c>Register</c> / <c>Unregister</c>.
    /// Expand (opt-in) через <see cref="OverflowPolicy.Expand"/> удваивает массивы.
    /// </summary>
    public sealed class HandleRegistry
    {
        private ICoinFlightGroup[] _groups;
        private int[] _generations;
        private int[] _free;
        private int _freeCount;
        private int _capacity;
        private int _max;
        private readonly OverflowPolicy _overflow;

        public int Max => _max;
        public int ActiveCount => _capacity - _freeCount;

        public HandleRegistry(int max, OverflowPolicy overflow = OverflowPolicy.DropNew)
        {
            if (max < 1) throw new ArgumentOutOfRangeException(nameof(max));
            _max = max;
            _overflow = overflow;
            _groups = new ICoinFlightGroup[max];
            _generations = new int[max];
            _free = new int[max];
        }

        public CoinFlightHandle Register(ICoinFlightGroup group)
        {
            if (group == null) throw new ArgumentNullException(nameof(group));

            if (!TryAcquireSlot(out int slot))
                return CoinFlightHandle.Invalid;

            _groups[slot] = group;
            return new CoinFlightHandle(slot + 1, _generations[slot]);
        }

        /// <summary>
        /// Выделяет слот с валидным handle, но без привязки к группе.
        /// Используйте для разрешения chicken-and-egg между конструированием
        /// группы и её регистрацией: reserve → создать группу с известным handle →
        /// <see cref="AttachGroup"/>.
        /// </summary>
        public CoinFlightHandle ReserveSlot()
        {
            if (!TryAcquireSlot(out int slot))
                return CoinFlightHandle.Invalid;

            _groups[slot] = null;
            return new CoinFlightHandle(slot + 1, _generations[slot]);
        }

        public bool AttachGroup(CoinFlightHandle handle, ICoinFlightGroup group)
        {
            if (group == null) throw new ArgumentNullException(nameof(group));
            if (!handle.IsValid) return false;
            int slot = handle.Id - 1;
            if ((uint)slot >= (uint)_capacity) return false;
            if (_generations[slot] != handle.Generation) return false;
            _groups[slot] = group;
            return true;
        }

        public void ReleaseReservation(CoinFlightHandle handle)
        {
            Unregister(handle);
        }

        private bool TryAcquireSlot(out int slot)
        {
            if (_freeCount > 0)
            {
                slot = _free[--_freeCount];
                return true;
            }
            if (_capacity < _max)
            {
                slot = _capacity++;
                _generations[slot] = 1;
                return true;
            }
            if (_overflow == OverflowPolicy.Expand)
            {
                Grow(_max * 2);
                slot = _capacity++;
                _generations[slot] = 1;
                return true;
            }
            slot = -1;
            return false;
        }

        public bool TryGet(CoinFlightHandle handle, out ICoinFlightGroup group)
        {
            group = null;
            if (!handle.IsValid) return false;
            int slot = handle.Id - 1;
            if ((uint)slot >= (uint)_capacity) return false;
            if (_generations[slot] != handle.Generation) return false;
            var g = _groups[slot];
            if (g == null) return false;
            group = g;
            return true;
        }

        /// <summary>
        /// Удаляет запись по <paramref name="handle"/> и инкрементирует generation.
        /// Будущие хэндлы, ссылающиеся на тот же слот со старой generation, будут
        /// отвергнуты методом <see cref="TryGet"/>.
        /// </summary>
        public void Unregister(CoinFlightHandle handle)
        {
            if (!handle.IsValid) return;
            int slot = handle.Id - 1;
            if ((uint)slot >= (uint)_capacity) return;
            if (_generations[slot] != handle.Generation) return;

            _groups[slot] = null;
            unchecked { _generations[slot]++; }
            if (_generations[slot] == 0) _generations[slot] = 1; // never zero
            _free[_freeCount++] = slot;
        }

        /// <summary>
        /// Вызывает <see cref="ICoinFlightGroup.Stop"/> на каждой живой группе.
        /// Не делает unregister — ожидается, что группы сами себя убирают в ходе выполнения Stop.
        /// </summary>
        public void StopAll(CancelPolicy policy)
        {
            for (int slot = 0; slot < _capacity; slot++)
            {
                var g = _groups[slot];
                if (g == null) continue;
                if (!g.IsAlive) continue;
                g.Stop(policy);
            }
        }

        private void Grow(int newMax)
        {
            if (newMax <= _max) return;
            Array.Resize(ref _groups, newMax);
            Array.Resize(ref _generations, newMax);
            Array.Resize(ref _free, newMax);
            _max = newMax;
        }
    }
}
