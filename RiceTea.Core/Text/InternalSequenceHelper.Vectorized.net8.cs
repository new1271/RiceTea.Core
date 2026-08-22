#if NET8_0_OR_GREATER
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

using InlineMethod;

using RiceTea.Core.Helpers;

namespace RiceTea.Core.Text;

partial class InternalSequenceHelper
{
    private static partial bool CheckTypeCanBeVectorized<T>() where T : unmanaged
            => Vector<T>.IsSupported;

    [SkipLocalsInit]
    private static unsafe partial int VectorizedCompareTo<T>(T* ptrA, T* ptrB, nuint length) where T : unmanaged
    {
        T* ptrEnd = ptrA + length;
        if (Limits.UseVector512())
        {
            Vector512<T>* ptrALimit = ((Vector512<T>*)ptrA) + 1;
            if (ptrALimit < ptrEnd)
            {
                Vector512<T>* ptrBLimit = ((Vector512<T>*)ptrB) + 1;
                UnsafeHelper.SkipInit(out Vector512<T> compareVector);
                UnsafeHelper.InitBlockUnaligned(&compareVector, 0xFF, UnsafeHelper.SizeOf<Vector512<T>>());
                do
                {
                    Vector512<T> sourceVectorA = Vector512.Load(ptrA);
                    Vector512<T> sourceVectorB = Vector512.Load(ptrB);
                    Vector512<T> resultVector = Vector512.Equals(sourceVectorA, sourceVectorB);
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
        if (Limits.UseVector256())
        {
            Vector256<T>* ptrALimit = ((Vector256<T>*)ptrA) + 1;
            if (ptrALimit < ptrEnd)
            {
                Vector256<T>* ptrBLimit = ((Vector256<T>*)ptrB) + 1;
                UnsafeHelper.SkipInit(out Vector256<T> compareVector);
                UnsafeHelper.InitBlockUnaligned(&compareVector, 0xFF, UnsafeHelper.SizeOf<Vector256<T>>());
                do
                {
                    Vector256<T> sourceVectorA = Vector256.Load(ptrA);
                    Vector256<T> sourceVectorB = Vector256.Load(ptrB);
                    Vector256<T> resultVector = Vector256.Equals(sourceVectorA, sourceVectorB);
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
        if (Limits.UseVector128())
        {
            Vector128<T>* ptrALimit = ((Vector128<T>*)ptrA) + 1;
            if (ptrALimit < ptrEnd)
            {
                Vector128<T>* ptrBLimit = ((Vector128<T>*)ptrB) + 1;
                UnsafeHelper.SkipInit(out Vector128<T> compareVector);
                UnsafeHelper.InitBlockUnaligned(&compareVector, 0xFF, UnsafeHelper.SizeOf<Vector128<T>>());
                do
                {
                    Vector128<T> sourceVectorA = Vector128.Load(ptrA);
                    Vector128<T> sourceVectorB = Vector128.Load(ptrB);
                    Vector128<T> resultVector = Vector128.Equals(sourceVectorA, sourceVectorB);
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
        if (Limits.UseVector64())
        {
            Vector64<T>* ptrALimit = ((Vector64<T>*)ptrA) + 1;
            if (ptrALimit < ptrEnd)
            {
                Vector64<T>* ptrBLimit = ((Vector64<T>*)ptrB) + 1;
                UnsafeHelper.SkipInit(out Vector64<T> compareVector);
                UnsafeHelper.InitBlockUnaligned(&compareVector, 0xFF, UnsafeHelper.SizeOf<Vector64<T>>());
                do
                {
                    Vector64<T> sourceVectorA = Vector64.Load(ptrA);
                    Vector64<T> sourceVectorB = Vector64.Load(ptrB);
                    Vector64<T> resultVector = Vector64.Equals(sourceVectorA, sourceVectorB);
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

    [Inline(InlineBehavior.Remove)]
    private static int FindIndexForResultVector<T>(in Vector512<T> vector) where T : unmanaged
        => MathHelper.TrailingZeroCount(vector.ExtractMostSignificantBits());

    [Inline(InlineBehavior.Remove)]
    private static int FindIndexForResultVector<T>(in Vector256<T> vector) where T : unmanaged
        => MathHelper.TrailingZeroCount(vector.ExtractMostSignificantBits());

    [Inline(InlineBehavior.Remove)]
    private static int FindIndexForResultVector<T>(in Vector128<T> vector) where T : unmanaged
        => MathHelper.TrailingZeroCount(vector.ExtractMostSignificantBits());

    [Inline(InlineBehavior.Remove)]
    private static unsafe int FindIndexForResultVector<T>(in Vector64<T> vector) where T : unmanaged
        => MathHelper.TrailingZeroCount(*(ulong*)UnsafeHelper.AsPointerIn(in vector)) / sizeof(T) / 8;
}
#endif