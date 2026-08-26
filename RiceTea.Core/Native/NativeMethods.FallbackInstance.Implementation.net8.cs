#if NET8_0_OR_GREATER
using System;
using System.Threading;

namespace RiceTea.Core.Native;

partial class NativeMethods
{
    partial class FallbackInstance
    {
        public static partial uint GetCurrentProcessorId() => (uint)Thread.GetCurrentProcessorId();

        public static partial ulong GetTicksForSystem() => (ulong)Environment.TickCount64;
    }
}
#endif