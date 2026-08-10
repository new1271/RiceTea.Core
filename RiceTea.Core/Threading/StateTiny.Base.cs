using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RiceTea.Core.Threading;

partial class StateTiny
{
    [StructLayout(LayoutKind.Auto)]
    private struct Base<T> where T : struct
    {
        private T _value;
        private nuint _version;

        public Base(T value) => _value = value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly T GetValueUnsafe() => _value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly T GetValue() => StateHelper.GetValue(in _value, in _version);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly T GetValue(out nuint version) => StateHelper.GetValue(in _value, in _version, out version);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(T value) => StateHelper.SetValue(ref _value, ref _version, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetValue(T value, nuint version) => StateHelper.TrySetValue(ref _value, ref _version, value, version);
    }
}
