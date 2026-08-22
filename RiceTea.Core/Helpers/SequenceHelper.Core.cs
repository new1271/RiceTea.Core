using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using InlineMethod;

using RiceTea.Core.Structures;

namespace RiceTea.Core.Helpers;

partial class SequenceHelper
{
    [SkipLocalsInit]
    private static partial class FastCore
    {
        [Inline(InlineBehavior.Remove)]
        public static bool IsIdempotence([InlineParameter] UnaryOperatorType type) => type == UnaryOperatorType.Identity;

        [Inline(InlineBehavior.Remove)]
        public static bool IsIdempotence([InlineParameter] BinaryOperatorType type) =>
            type is BinaryOperatorType.Left or BinaryOperatorType.Right or
            BinaryOperatorType.Or or BinaryOperatorType.And or
            BinaryOperatorType.Min or BinaryOperatorType.Max;

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DebuggerStepThrough]
        [DoesNotReturn]
        public static Unit ThrowDivideByZeroException() => throw new DivideByZeroException();
    }

    [SkipLocalsInit]
    private static partial class FastCore<T> where T : unmanaged { }

    [SkipLocalsInit]
    private static partial class FastCoreOfBoolean { }

    [SkipLocalsInit]
    private static partial class SlowCore { }

    [SkipLocalsInit]
    private static partial class SlowCore<T> { }
}
