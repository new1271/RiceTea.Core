using System;
using System.Runtime.InteropServices;

using RiceTea.Core.Helpers;
using RiceTea.Core.Structures;

namespace RiceTea.Core.Native;

partial class NativeMethods
{
    [StructLayout(LayoutKind.Sequential, Size = 8)]
    private unsafe struct RawWaitingEvent
    {
        public const int HandleSize = sizeof(uint);

        private readonly SysBool32 _autoReset;

        private uint _state;

        public readonly bool IsAutoReset => _autoReset;

        public readonly bool State => MathHelper.ToBoolean(Atomics.Read(ref UnsafeHelper.AsRefIn(in _state)));

        public static IntPtr GetWaitingHandleFromEvent(RawWaitingEvent* source)
            => (IntPtr)(&source->_state);

        public static RawWaitingEvent* GetEventFromWaitingHandle(IntPtr waitingHandle)
            => (RawWaitingEvent*)(((SysBool32*)waitingHandle) - 1);

        public RawWaitingEvent(bool initialState, bool autoReset)
        {
            _autoReset = autoReset;
            _state = MathHelper.BooleanToUInt32(initialState);
        }

        public bool Set() => Atomics.Exchange(ref _state, Booleans.TrueInt) == Booleans.FalseInt;

        public bool Reset() => Atomics.Exchange(ref _state, Booleans.FalseInt) != Booleans.FalseInt;
    }
}
