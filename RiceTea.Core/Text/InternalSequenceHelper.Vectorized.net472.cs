#if NET472_OR_GREATER
using System.Numerics;
using System.Runtime.CompilerServices;

using RiceTea.Core.Helpers;

namespace RiceTea.Core.Text;

partial class InternalSequenceHelper
{
    private static partial bool CheckTypeCanBeVectorized<T>() where T : unmanaged
            => (typeof(T) == typeof(byte)) ||
                   (typeof(T) == typeof(short)) ||
                   (typeof(T) == typeof(int)) ||
                   (typeof(T) == typeof(long)) ||
                   (typeof(T) == typeof(sbyte)) ||
                   (typeof(T) == typeof(ushort)) ||
                   (typeof(T) == typeof(uint)) ||
                   (typeof(T) == typeof(ulong)) ||
                   (typeof(T) == typeof(float)) ||
                   (typeof(T) == typeof(double));

    [SkipLocalsInit]
    private static unsafe partial int VectorizedCompareTo<T>(T* ptrA, T* ptrB, nuint length) where T : unmanaged
    {
        T* ptrEnd = ptrA + length;
        if (Limits.UseVector())
        {
            Vector<T>* ptrALimit = ((Vector<T>*)ptrA) + 1;
            if (ptrALimit < ptrEnd)
            {
                Vector<T>* ptrBLimit = ((Vector<T>*)ptrB) + 1;
                UnsafeHelper.SkipInit(out Vector<T> compareVector);
                UnsafeHelper.InitBlockUnaligned(&compareVector, 0xFF, UnsafeHelper.SizeOf<Vector<T>>());
                do
                {
                    Vector<T> sourceVectorA = UnsafeHelper.ReadUnaligned<Vector<T>>(ptrA);
                    Vector<T> sourceVectorB = UnsafeHelper.ReadUnaligned<Vector<T>>(ptrB);
                    Vector<T> resultVector = Vector.Equals(sourceVectorA, sourceVectorB);
                    if (resultVector != compareVector)
                    {
                        int index = FindIndexForResultVector(~resultVector);
                        int result = CompareTo(ptrA[index], ptrB[index]);
                        DebugHelper.ThrowIf(result == 0);
                        return result;
                    }
                    ptrA = (T*)ptrALimit;
                    ptrB = (T*)ptrBLimit++;
                } while (++ptrALimit < ptrEnd);
                if (ptrA >= ptrEnd)
                    return 0;
            }
        }

        return LegacyCompareTo(ptrA, ptrEnd, ptrB);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe int FindIndexForResultVector<T>(in Vector<T> vector) where T : unmanaged
    {
        ulong* ptrVector = (ulong*)UnsafeHelper.AsPointerIn(in vector);
        for (int i = 0; i < Vector<ulong>.Count; i++)
        {
            int result = MathHelper.TrailingZeroCount(ptrVector[i]);
            if (result == sizeof(ulong) * 8)
                continue;
            return i * (sizeof(ulong) / sizeof(T)) + result / sizeof(T) / 8;
        }
        return Vector<T>.Count;
    }
}
#endif