using System;
using System.Runtime.CompilerServices;

using RiceTea.Core.Buffers;
using RiceTea.Core.Helpers;
using RiceTea.Core.Native;
using RiceTea.Core.Windows.Structures;

namespace RiceTea.Core.Windows.ObjectModels;

public unsafe sealed class Win32Stream : Win32SequentialStream, IWin32Stream
{
    public static readonly Guid IID_IStream = new Guid(0x0000000c, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

    public Win32Stream() { }

    public Win32Stream(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

    private new enum MethodTable
    {
        _Start = Win32SequentialStream.MethodTable._End,
        Seek = _Start,
        SetSize,
        CopyTo,
        Commit,
        Revert,
        LockRegion,
        UnlockRegion,
        Stat,
        Clone,
        _End
    }

    [SkipLocalsInit]
    public ulong Seek(long dlibMove, StreamSeekType dwOrigin)
    {
        ulong result;
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Seek);
        int hr = ((delegate* unmanaged[Stdcall]<void*, long, StreamSeekType, ulong*, int>)functionPointer)(nativePointer,
            dlibMove, dwOrigin, &result);
        ThrowHelper.ThrowExceptionForHR(hr);
        return result;
    }

    public void SetSize(ulong libNewSize)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Seek);
        int hr = ((delegate* unmanaged[Stdcall]<void*, ulong, int>)functionPointer)(nativePointer, libNewSize);
        ThrowHelper.ThrowExceptionForHR(hr);
    }

    public void CopyTo(ComObject pstm, ulong cb, out ulong pcbRead, out ulong pcbWritten) => CopyToCore(pstm.NativePointer, cb, out pcbRead, out pcbWritten);

    public void CopyTo(IWin32Stream pstm, ulong cb, out ulong pcbRead, out ulong pcbWritten)
    {
        if (pstm is ComObject comObject)
        {
            CopyToCore(comObject.NativePointer, cb, out pcbRead, out pcbWritten);
            return;
        }
        CopyToCoreAlternative(pstm, cb, out pcbRead, out pcbWritten);
    }

    private void CopyToCore(void* pstm, ulong cb, out ulong pcbRead, out ulong pcbWritten)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Seek);
        int hr = ((delegate* unmanaged[Stdcall]<void*, void*, ulong, ulong*, ulong*, int>)functionPointer)(nativePointer,
            pstm, cb, UnsafeHelper.AsPointerOut(out pcbRead), UnsafeHelper.AsPointerOut(out pcbWritten));
        ThrowHelper.ThrowExceptionForHR(hr);
    }

    private void CopyToCoreAlternative(IWin32Stream pstm, ulong cb, out ulong pcbRead, out ulong pcbWritten)
    {
        const uint TransferBufferSize = 8192;

        pcbRead = 0;
        pcbWritten = 0;
        ArrayPool<byte> pool = ArrayPool<byte>.Shared;
        if (cb >= TransferBufferSize)
        {
            byte[] buffer = pool.Rent(TransferBufferSize);
            do
            {
                fixed (byte* ptr = buffer)
                {
                    ulong bytesRead = Read(ptr, TransferBufferSize);
                    if (bytesRead == 0)
                    {
                        pool.Return(buffer);
                        return;
                    }
                    ulong bytesWritten = pstm.Write(ptr, bytesRead);
                    pcbRead += bytesRead;
                    pcbWritten += bytesWritten;
                    if (bytesWritten != bytesRead)
                    {
                        pool.Return(buffer);
                        throw new InvalidOperationException();
                    }
                }
                cb -= TransferBufferSize;
            } while (cb >= TransferBufferSize);
            pool.Return(buffer);
        }
        if (cb == 0)
            return;
        {
            byte[] buffer = pool.Rent(unchecked((uint)cb));
            fixed (byte* ptr = buffer)
            {
                ulong bytesRead = Read(ptr, cb);
                if (bytesRead == 0)
                {
                    pool.Return(buffer);
                    return;
                }
                ulong bytesWritten = pstm.Write(ptr, bytesRead);
                pcbRead += bytesRead;
                pcbWritten += bytesWritten;
                if (bytesWritten != bytesRead)
                {
                    pool.Return(buffer);
                    throw new InvalidOperationException();
                }
            }
            pool.Return(buffer);
        }
    }

    public void Commit(StreamCommitFlags grfCommitFlags)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Commit);
        int hr = ((delegate* unmanaged[Stdcall]<void*, StreamCommitFlags, int>)functionPointer)(nativePointer, grfCommitFlags);
        ThrowHelper.ThrowExceptionForHR(hr);
    }

    public void Revert()
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Revert);
        int hr = ((delegate* unmanaged[Stdcall]<void*, int>)functionPointer)(nativePointer);
        ThrowHelper.ThrowExceptionForHR(hr);
    }

    public void LockRegion(ulong libOffset, ulong cb, LockType dwLockType)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.LockRegion);
        int hr = ((delegate* unmanaged[Stdcall]<void*, ulong, ulong, LockType, int>)functionPointer)(nativePointer,
            libOffset, cb, dwLockType);
        ThrowHelper.ThrowExceptionForHR(hr);
    }

    public void UnlockRegion(ulong libOffset, ulong cb, LockType dwLockType)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.UnlockRegion);
        int hr = ((delegate* unmanaged[Stdcall]<void*, ulong, ulong, LockType, int>)functionPointer)(nativePointer,
            libOffset, cb, dwLockType);
        ThrowHelper.ThrowExceptionForHR(hr);
    }

    [SkipLocalsInit]
    public StructuredStorageStat Stat(StructuredStorageStatFlags grfStatFlag)
    {
        StructuredStorageStat stat;
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Stat);
        int hr = ((delegate* unmanaged[Stdcall]<void*, StructuredStorageStat*, StructuredStorageStatFlags, int>)functionPointer)(nativePointer,
            &stat, grfStatFlag);
        ThrowHelper.ThrowExceptionForHR(hr);
        return stat;
    }

    public Win32Stream Clone()
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Clone);
        int hr = ((delegate* unmanaged[Stdcall]<void*, void**, int>)functionPointer)(nativePointer, &nativePointer);
        ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
        return new Win32Stream(nativePointer, ReferenceType.Owned);
    }

    IWin32Stream IWin32Stream.Clone() => Clone();
}
