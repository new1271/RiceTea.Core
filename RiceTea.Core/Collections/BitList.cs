using System;

using InlineMethod;

using RiceTea.Core.Helpers;

namespace RiceTea.Core.Collections;

public sealed class BitList : BitListBase
{
    public BitList() : base(new nuint[16]) { }

    public BitList(int capacity) : base(GetRequiredSectionArray(capacity)) { }

    [Inline(InlineBehavior.Remove)]
    private static nuint[] GetRequiredSectionArray(int capacity)
    {
        capacity = GetRequiredSectionCount(capacity);
        return capacity <= 0 ? Array.Empty<nuint>() : (new nuint[capacity]);
    }

    protected override void EnsureCapacity()
    {
        nuint[] array = _array;
        int capacity = _array.Length;
        int count = _count;
        if (count <= capacity * SectionSize)
            return;

        int newCapacity;
        if (capacity >= Limits.MaxArrayLength / 2)
        {
            if (capacity >= Limits.MaxArrayLength)
                OutOfMemoryException.Throw();
            newCapacity = Limits.MaxArrayLength;
        }
        else
            newCapacity = MathHelper.Max(capacity * 2, MathHelper.CeilDiv(count, SectionSize));

        nuint[] newArray = new nuint[newCapacity];
        Array.Copy(array, newArray, capacity);
        _array = newArray;
    }
}
