using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RiceTea.Core.Threading;

partial class StateTiny
{
    [StructLayout(LayoutKind.Auto)]
    public struct SingleWriter<T> where T : struct
    {
        private Base<T> _base;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SingleWriter(T value) => _base = new Base<T>(value);

        public T Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => _base.GetValue();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _base.SetValue(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly T GetValueUnsafe() => _base.GetValueUnsafe();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(Func<T, T> updateFactory)
            => _base.SetValue(updateFactory.Invoke(_base.GetValueUnsafe()));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update<TState>(Func<T, TState, T> updateFactory, TState state)
            => _base.SetValue(updateFactory.Invoke(_base.GetValueUnsafe(), state));
    }
}
