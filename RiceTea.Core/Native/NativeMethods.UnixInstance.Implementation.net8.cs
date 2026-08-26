#if NET8_0_OR_GREATER
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

using RiceTea.Core.Structures;

namespace RiceTea.Core.Native;

partial class NativeMethods
{
    [SuppressUnmanagedCodeSecurity]
    unsafe partial class UnixInstance : INativeMethodInstance
    {
        private const string CLibraryName = "c";

        private static readonly void*[] EmptyPointerArray = [];

        static UnixInstance()
        {
            (_syscallID_gettid, _syscallID_futex) = RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X86 => (224, 240),
                Architecture.X64 => (186, 202),
                Architecture.Arm => (224, 240),
                Architecture.Arm64 => (178, 98),
                Architecture.S390x => (236, 238),
                Architecture.LoongArch64 => (178, 98),
                Architecture.Armv6 => (224, 240),
                Architecture.Ppc64le => (179, 221),
                //Architecture.RiscV64 => (178, 98),
                _ => (0, 0)
            };
            if (_syscallID_futex == 0)
                Debug.WriteLine($"This platform doesn't support futex, so {nameof(SetWaitingHandle)}, {nameof(WaitForWaitingHandle)} cannot work correctly!");
            void* func = GetImportedMethodPointer(null, nameof(gettid));
            if (func is null)
                func = (delegate* unmanaged[Cdecl]<int>)&gettid_fallback;

            _gettidFunc = func;
            _cacheflushFunc = GetImportedMethodPointer(null, nameof(cacheflush));
        }

        public static partial ulong GetTicksForSystem()
        {
            const int CLOCK_MONOTONIC = 1;

            TimeSpecification ts;
            if (clock_gettime(CLOCK_MONOTONIC, &ts) != 0)
                return (ulong)Environment.TickCount64;
            return ts.tv_sec * TimeSpan.TicksPerSecond + ts.tv_nsec / 100;
        }

        public static partial void* GetImportedMethodPointer(string? dllName, int methodIndex) => null;

        public static partial void* GetImportedMethodPointer(string? dllName, string methodName)
        {
            IntPtr module = LoadLibrary(dllName);
            return GetImportedMethodPointer(module, methodName);
        }

        public static partial void*[] GetImportedMethodPointers(string? dllName, in ParamArrayTiny<int> methodIndices)
        {
            int length = methodIndices.Length;
            if (length <= 0)
                return EmptyPointerArray;

            return new void*[length];
        }

        public static partial void*[] GetImportedMethodPointers(string? dllName, in ParamArrayTiny<string> methodNames)
        {
            int length = methodNames.Length;
            if (length <= 0)
                return EmptyPointerArray;

            IntPtr module = LoadLibrary(dllName);
            void*[] result = new void*[length];
            int i = 0;
            do
            {
                result[i] = GetImportedMethodPointer(module, methodNames[i]);
            } while (++i < length);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IntPtr LoadLibrary(string? dllName)
            => dllName is null ?
                NativeLibrary.GetMainProgramHandle() :
                (NativeLibrary.TryLoad(dllName, out IntPtr result) ? result : IntPtr.Zero);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void* GetImportedMethodPointer(IntPtr module, string methodName)
            => NativeLibrary.TryGetExport(module, methodName, out IntPtr result) ? result.ToPointer() : null;

#if NET8_0_OR_GREATER
        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#endif
        private static partial int gettid_fallback();
    }
}
#endif