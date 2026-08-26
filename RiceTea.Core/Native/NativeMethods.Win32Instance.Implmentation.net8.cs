#if NET8_0_OR_GREATER
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

using RiceTea.Core.Structures;

namespace RiceTea.Core.Native;

partial class NativeMethods
{
    [SuppressUnmanagedCodeSecurity]
    unsafe partial class Win32Instance
    {
        private static readonly void*[] EmptyPointerArray = [];

        [SuppressGCTransition]
        [DllImport("kernel32")]
        private static extern void* GetProcAddress(IntPtr hModule, byte* lpProcName);

        public static partial void* GetImportedMethodPointer(string? dllName, int methodIndex)
        {
            IntPtr module = LoadLibrary(dllName);
            return GetProcAddress(module, (byte*)methodIndex);
        }

        public static partial void* GetImportedMethodPointer(string? dllName, string methodName)
        {
            IntPtr module = LoadLibrary(dllName);
            return GetProcAddress(module, methodName);
        }

        public static partial void*[] GetImportedMethodPointers(string? dllName, in ParamArrayTiny<int> methodIndices)
        {
            int length = methodIndices.Length;
            if (length <= 0)
                return EmptyPointerArray;

            IntPtr module = LoadLibrary(dllName);
            void*[] result = new void*[length];
            int i = 0;
            do
            {
                result[i] = GetProcAddress(module, (byte*)methodIndices[i]);
            } while (++i < length);
            return result;
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
                result[i] = GetProcAddress(module, methodNames[i]);
            } while (++i < length);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IntPtr LoadLibrary(string? dllName)
            => dllName is null ?
                NativeLibrary.GetMainProgramHandle() :
                (NativeLibrary.TryLoad(dllName, out IntPtr result) ? result : IntPtr.Zero);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void* GetProcAddress(IntPtr module, string methodName)
            => NativeLibrary.TryGetExport(module, methodName, out IntPtr result) ? result.ToPointer() : null;
    }
}
#endif