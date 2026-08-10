using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace RiceTea.Core.Threading;

partial class StateTiny
{
    [StructLayout(LayoutKind.Auto)]
    public struct MultipleWriter<T> where T : struct
    {
        private Lock? _writerLock; // 支援無初始化啟動
        private Base<T> _base;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MultipleWriter(T value)
        {
            _writerLock = new Lock();
            _base = new Base<T>(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly T GetValueUnsafe() => _base.GetValueUnsafe();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Lock GetWriterLock() 
            => _writerLock ?? LazyTinyHelper.InitializeAndReturn(ref _writerLock, static () => new Lock(), threadSafe: true, syncLock: null);

        public T Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => _base.GetValue();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                lock (GetWriterLock())
                    _base.SetValue(value);
            }
        }

        public void Update(Func<T, T> updateFactory)
        {
            T value = updateFactory.Invoke(_base.GetValue(out nuint version));
            lock (GetWriterLock())
            {
                if (_base.TrySetValue(value, version))
                    return;
                _base.SetValue(updateFactory.Invoke(_base.GetValueUnsafe()));
            }
        }

        public void Update<TState>(Func<T, TState, T> updateFactory, TState state)
        {
            T value = updateFactory.Invoke(_base.GetValue(out nuint version), state);
            lock (GetWriterLock())
            {
                if (_base.TrySetValue(value, version))
                    return;
                _base.SetValue(updateFactory.Invoke(_base.GetValueUnsafe(), state));
            }
        }
    }
}
