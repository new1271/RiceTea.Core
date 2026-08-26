#if NET472_OR_GREATER
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

using InlineIL;

using InlineMethod;

using RiceTea.Core.Buffers;
using RiceTea.Core.Helpers;
using RiceTea.Core.Structures;
using RiceTea.Core.Text;

namespace RiceTea.Core.Native;

partial class NativeMethods
{
    [SuppressUnmanagedCodeSecurity]
    unsafe partial class UnixInstance : INativeMethodInstance
    {
        private const string CLibraryName = "libc";

        private static readonly void*[] EmptyPointerArray = [];

        static UnixInstance()
        {
            (_syscallID_gettid, _syscallID_futex) = RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X86 => (224, 240),
                Architecture.X64 => (186, 202),
                Architecture.Arm => (224, 240),
                Architecture.Arm64 => (178, 98),
                _ => (0, 0)
            };
            if (_syscallID_futex == 0)
                Debug.WriteLine($"This platform doesn't support futex, so {nameof(SetWaitingHandle)}, {nameof(WaitForWaitingHandle)} cannot work correctly!");
            void* func = GetImportedMethodPointer(null, nameof(gettid));
            if (func is null)
                func = (delegate*<int>)&gettid_fallback;

            _gettidFunc = func;
            _cacheflushFunc = GetImportedMethodPointer(null, nameof(cacheflush));
        }

        [DllImport(CLibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void* malloc(nuint size);

        [DllImport(CLibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void free(void* memblock);

        void* INativeMethodInstance.AllocMemory(nuint size) => AllocMemory(size);

        void INativeMethodInstance.FreeMemory(void* ptr) => FreeMemory(ptr);

        public static partial ulong GetTicksForSystem()
        {
            const int CLOCK_MONOTONIC = 1;

            TimeSpecification ts;
            if (clock_gettime(CLOCK_MONOTONIC, &ts) != 0)
                return (ulong)(Environment.TickCount & uint.MaxValue);
            return ts.tv_sec * TimeSpan.TicksPerSecond + ts.tv_nsec / 100;
        }

        public static void* AllocMemory(nuint size) => malloc(size);

        public static void FreeMemory(void* ptr) => free(ptr);

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
        {
            const int RTLD_NOW = 2;
            const int RTLD_LOCAL = 0;

            if (dllName is null)
                return LibDl.Instance.dlopen(null, RTLD_LOCAL | RTLD_NOW);

            return Core(dllName);

            static IntPtr Core(string dllName)
            {
                int length = dllName.Length;
                int bufferLength = Utf8EncodingHelper.GetWorstCaseForDecodeLength(length);
                fixed (char* ptr = dllName)
                {
                    if (length < Limits.MaxStackallocBytes)
                        return Core_FastRoute(ptr, length, bufferLength);
                    else
                        return Core_SlowRoute(ptr, length, bufferLength);
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            [SkipLocalsInit]
            static IntPtr Core_FastRoute(char* dllName, int length, int bufferLength)
            {
                byte* buffer = stackalloc byte[bufferLength + 1];
                byte* bufferEnd = Utf8EncodingHelper.ReadFromUtf16Buffer(dllName, dllName + length, buffer, buffer + length);
                *bufferEnd = 0;
                return LibDl.Instance.dlopen(buffer, RTLD_LOCAL | RTLD_NOW);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            [SkipLocalsInit]
            static IntPtr Core_SlowRoute(char* dllName, int length, int bufferLength)
            {
                byte[] buffer = new byte[bufferLength + 1];
                fixed (byte* ptr = buffer)
                {
                    byte* bufferEnd = Utf8EncodingHelper.ReadFromUtf16Buffer(dllName, dllName + length, ptr, ptr + length);
                    *bufferEnd = 0;
                    return LibDl.Instance.dlopen(ptr, RTLD_LOCAL | RTLD_NOW);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void* GetImportedMethodPointer(IntPtr module, string methodName)
        {
            int length = methodName.Length;
            fixed (char* ptr = methodName)
            {
                if (length < Limits.MaxStackallocBytes)
                    return FastRoute(module, ptr, length);
                else
                    return SlowRoute(module, ptr, length);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            [SkipLocalsInit]
            static void* FastRoute(IntPtr module, char* methodName, int length)
            {
                byte* buffer = stackalloc byte[length + 1];
                AsciiEncodingHelper.ReadFromUtf16BufferCore_OutOfAsciiRange(methodName, buffer, (nuint)length);
                buffer[length] = 0;

                return LibDl.Instance.dlsym(module, buffer);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static void* SlowRoute(IntPtr module, char* methodName, int length)
            {
                byte[] buffer = new byte[length + 1];
                fixed (byte* ptr = buffer)
                {
                    AsciiEncodingHelper.ReadFromUtf16BufferCore_OutOfAsciiRange(methodName, ptr, (nuint)length);
                    ptr[length] = 0;

                    return LibDl.Instance.dlsym(module, ptr);
                }
            }
        }

        private static partial int gettid_fallback();

#if !NET8_0_OR_GREATER
        private static class LibDl
        {
            public static readonly ILibDl Instance = FindInstance();

            private static ILibDl FindInstance()
            {
                try
                {
                    return CreateModernInstance();
                }
                catch (Exception)
                {
                    return CreateLegacyInstance();
                }

                [MethodImpl(MethodImplOptions.NoInlining)]
                static ILibDl CreateModernInstance() => new Modern();

                [MethodImpl(MethodImplOptions.NoInlining)]
                static ILibDl CreateLegacyInstance() => new Legacy();
            }

            private sealed class Modern : ILibDl
            {
                public Modern()
                {
                    IL.Emit.Ldtoken(new MethodRef(typeof(Modern), nameof(dlopen)));
                    IL.Emit.Call(new MethodRef(typeof(RuntimeHelpers), nameof(RuntimeHelpers.PrepareMethod), typeof(RuntimeMethodHandle)));
                    IL.Emit.Ldtoken(new MethodRef(typeof(Modern), nameof(dlsym)));
                    IL.Emit.Call(new MethodRef(typeof(RuntimeHelpers), nameof(RuntimeHelpers.PrepareMethod), typeof(RuntimeMethodHandle)));
                }

                [DllImport(CLibraryName, CallingConvention = CallingConvention.Cdecl)]
                private static extern IntPtr dlopen(byte* filename, int flags);

                [DllImport(CLibraryName, CallingConvention = CallingConvention.Cdecl)]
                private static extern void* dlsym(IntPtr handle, byte* symbol);

                IntPtr ILibDl.dlopen(byte* filename, int flags) => dlopen(filename, flags);

                void* ILibDl.dlsym(nint handle, byte* symbol) => dlsym(handle, symbol);
            }

            private sealed class Legacy : ILibDl
            {
                public Legacy()
                {
                    IL.Emit.Ldtoken(new MethodRef(typeof(Legacy), nameof(dlopen)));
                    IL.Emit.Call(new MethodRef(typeof(RuntimeHelpers), nameof(RuntimeHelpers.PrepareMethod), typeof(RuntimeMethodHandle)));
                    IL.Emit.Ldtoken(new MethodRef(typeof(Legacy), nameof(dlsym)));
                    IL.Emit.Call(new MethodRef(typeof(RuntimeHelpers), nameof(RuntimeHelpers.PrepareMethod), typeof(RuntimeMethodHandle)));
                }

                [DllImport("dl", CallingConvention = CallingConvention.Cdecl)]
                private static extern IntPtr dlopen(byte* filename, int flags);

                [DllImport("dl", CallingConvention = CallingConvention.Cdecl)]
                private static extern void* dlsym(IntPtr handle, byte* symbol);

                IntPtr ILibDl.dlopen(byte* filename, int flags) => dlopen(filename, flags);

                void* ILibDl.dlsym(nint handle, byte* symbol) => dlsym(handle, symbol);
            }
        }

        private interface ILibDl
        {
            IntPtr dlopen(byte* filename, int flags);

            void* dlsym(IntPtr handle, byte* symbol);
        }
#endif
    }
}
#endif