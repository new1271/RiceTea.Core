using System;
using System.Runtime.InteropServices;

using RiceTea.Core.Structures;

namespace RiceTea.Core.Windows.Internals;

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct SecurityAttributes
{
    public int nLength;
    public void* lpSecurityDescriptor;
    public SysBool32 bInheritHandle;
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct StartupInfo
{
    public uint cb;
    public char* lpReserved;
    public char* lpDesktop;
    public char* lpTitle;
    public uint dwX;
    public uint dwY;
    public uint dwXSize;
    public uint dwYSize;
    public uint dwXCountChars;
    public uint dwYCountChars;
    public uint dwFillAttribute;
    public StartupInfoFlags dwFlags;
    public ushort wShowWindow;
    public ushort cbReserved2;
    public byte* lpReserved2;
    public IntPtr hStdInput;
    public IntPtr hStdOutput;
    public IntPtr hStdError;
}

[StructLayout(LayoutKind.Sequential)]
internal struct ProcessInformation
{
    public IntPtr hProcess;
    public IntPtr hThread;
    public uint dwProcessId;
    public uint dwThreadId;
}
