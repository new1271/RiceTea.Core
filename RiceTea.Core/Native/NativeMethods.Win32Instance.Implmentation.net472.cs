#if NET472_OR_GREATER
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

using RiceTea.Core.Buffers;
using RiceTea.Core.Structures;
using RiceTea.Core.Text;

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

        [DllImport("kernel32")]
        private static extern IntPtr GetModuleHandleW(char* lpModuleName);

        [DllImport("kernel32")]
        private static extern IntPtr LoadLibraryW(char* lpLibFileName);

        [DllImport("kernel32")]
        private static extern void* HeapAlloc(IntPtr hHeap, uint dwFlags, nuint dwBytes);

        [DllImport("kernel32")]
        private static extern void HeapFree(IntPtr hHeap, uint dwFlags, void* lpMem);

        private static readonly IntPtr _heap = GetProcessHeap();

        void* INativeMethodInstance.AllocMemory(nuint size)
            => AllocMemory(size);

        void INativeMethodInstance.FreeMemory(void* ptr)
            => FreeMemory(ptr);

        public static void* AllocMemory(nuint size) => HeapAlloc(_heap, 0, size);

        public static void FreeMemory(void* ptr) => HeapFree(_heap, 0, ptr);

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
        {
            if (dllName is null)
                return GetModuleHandleW(null);

            fixed (char* ptr = dllName)
                return LoadLibraryW(ptr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void* GetProcAddress(IntPtr module, string methodName)
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

                return GetProcAddress(module, buffer);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static void* SlowRoute(IntPtr module, char* methodName, int length)
            {
                byte[] buffer = new byte[length + 1];
                fixed (byte* ptr = buffer)
                {
                    AsciiEncodingHelper.ReadFromUtf16BufferCore_OutOfAsciiRange(methodName, ptr, (nuint)length);
                    ptr[length] = 0;

                    return GetProcAddress(module, ptr);
                }
            }
        }
    }
}
#endif