#if NET472_OR_GREATER
using System.Runtime.CompilerServices;

using InlineMethod;

using RiceTea.Core;
using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;
using RiceTea.Core.Text;

namespace System.IO;

internal partial class PathInternal
{
    private static readonly IPlatformImpl _impl = GetPlatformImplForPlatform();

    [Inline(InlineBehavior.Remove)]
    private static IPlatformImpl GetPlatformImplForPlatform()
    {
        if (PlatformHelper.IsWindows)
            return new WindowsImpl();
        if (PlatformHelper.IsUnix)
            return new UnixImpl();
        return PlatformNotSupportedException.Throw<IPlatformImpl>();
    }

    /// <summary>
    /// Returns true if the two paths have the same root
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool AreRootsEqual(string first, string second, StringComparison comparisonType)
    {
        int firstRootLength, secondRootLength;
        IPlatformImpl impl = _impl;
        switch (impl)
        {
            case WindowsImpl:
                firstRootLength = WindowsImpl.GetRootLength(first);
                secondRootLength = WindowsImpl.GetRootLength(second);
                break;
            case UnixImpl:
                firstRootLength = UnixImpl.GetRootLength(first);
                secondRootLength = UnixImpl.GetRootLength(second);
                break;
            default:
                firstRootLength = impl.GetRootLength(first);
                secondRootLength = impl.GetRootLength(second);
                break;
        }

        return firstRootLength == secondRootLength
            && SequenceHelper.Equals(
                a: first,
                indexA: 0,
                b: second,
                indexB: 0,
                length: firstRootLength,
                comparisonType: comparisonType);
    }

    /// <summary>
    /// Gets the count of common characters from the left optionally ignoring case
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe int EqualStartingCharacterCount(string first, string second, bool ignoreCase)
    {
        if (StringHelper.IsNullOrEmpty(first) || StringHelper.IsNullOrEmpty(second))
            return 0;

        int commonChars = 0;

        fixed (char* f = first)
        fixed (char* s = second)
        {
            char* l = f;
            char* r = s;
            char* leftEnd = l + first.Length;
            char* rightEnd = r + second.Length;

            while (l != leftEnd && r != rightEnd
                && (*l == *r || (ignoreCase && char.ToUpperInvariant(*l) == char.ToUpperInvariant(*r))))
            {
                commonChars++;
                l++;
                r++;
            }
        }

        return commonChars;
    }

    /// <summary>
    /// Get the common path length from the start of the string.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetCommonPathLength(string first, string second, bool ignoreCase)
    {
        int commonChars = EqualStartingCharacterCount(first, second, ignoreCase: ignoreCase);

        // If nothing matches
        if (commonChars == 0)
            return commonChars;

        // Or we're a full string and equal length or match to a separator
        if (commonChars == first.Length
            && (commonChars == second.Length || IsDirectorySeparator(second[commonChars])))
            return commonChars;

        if (commonChars == second.Length && IsDirectorySeparator(first[commonChars]))
            return commonChars;

        // It's possible we matched somewhere in the middle of a segment e.g. C:\Foodie and C:\Foobar.
        while (commonChars > 0 && !IsDirectorySeparator(first[commonChars - 1]))
            commonChars--;

        return commonChars;
    }

    [Inline(InlineBehavior.Remove)]
    public static bool IsDirectorySeparator(char c)
        => c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar;

    /// <summary>
    /// Returns true if the path ends in a directory separator.
    /// </summary>
    [Inline(InlineBehavior.Remove)]
    private static bool EndsInDirectorySeparator(string path)
    {
        int length = path.Length;
        return length > 0 && IsDirectorySeparator(path.LastOrDefault());
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string GetRelativePath(string relativeTo, string path)
    {
        ArgumentNullException.ThrowIfNull(relativeTo);
        ArgumentNullException.ThrowIfNull(path);

        StringComparison comparison;

        IPlatformImpl impl = _impl;
        switch (impl)
        {
            case WindowsImpl:
                if (WindowsImpl.IsEffectivelyEmpty(relativeTo))
                    ArgumentException.Throw("the path cannot be empty!", nameof(relativeTo));
                if (WindowsImpl.IsEffectivelyEmpty(path))
                    ArgumentException.Throw("the path cannot be empty!", nameof(path));
                comparison = WindowsImpl.GetPathComparisonMode();
                break;
            case UnixImpl:
                if (UnixImpl.IsEffectivelyEmpty(relativeTo))
                    ArgumentException.Throw("the path cannot be empty!", nameof(relativeTo));
                if (UnixImpl.IsEffectivelyEmpty(path))
                    ArgumentException.Throw("the path cannot be empty!", nameof(path));
                comparison = UnixImpl.GetPathComparisonMode();
                break;
            default:
                if (impl.IsEffectivelyEmpty(relativeTo))
                    ArgumentException.Throw("the path cannot be empty!", nameof(relativeTo));
                if (impl.IsEffectivelyEmpty(path))
                    ArgumentException.Throw("the path cannot be empty!", nameof(path));
                comparison = impl.GetPathComparisonMode();
                break;
        }

        relativeTo = Path.GetFullPath(relativeTo);
        path = Path.GetFullPath(path);

        // Need to check if the roots are different- if they are we need to return the "to" path.
        if (!AreRootsEqual(relativeTo, path, comparison))
            return path;

        int commonLength = GetCommonPathLength(relativeTo, path, ignoreCase: comparison == StringComparison.OrdinalIgnoreCase);

        // If there is nothing in common they can't share the same root, return the "to" path as is.
        if (commonLength == 0)
            return path;

        // Trailing separators aren't significant for comparison
        int relativeToLength = relativeTo.Length;
        if (EndsInDirectorySeparator(relativeTo))
            relativeToLength--;

        bool pathEndsInSeparator = EndsInDirectorySeparator(path);
        int pathLength = path.Length;
        if (pathEndsInSeparator)
            pathLength--;

        // If we have effectively the same path, return "."
        if (relativeToLength == pathLength && commonLength >= relativeToLength) return ".";

        // We have the same root, we need to calculate the difference now using the
        // common Length and Segment count past the length.
        //
        // Some examples:
        //
        //  C:\Foo C:\Bar L3, S1 -> ..\Bar
        //  C:\Foo C:\Foo\Bar L6, S0 -> Bar
        //  C:\Foo\Bar C:\Bar\Bar L3, S2 -> ..\..\Bar\Bar
        //  C:\Foo\Foo C:\Foo\Bar L7, S1 -> ..\Bar

        using StringBuilderTiny builder = new StringBuilderTiny();
        if (Limits.UseStackallocStringBuilder)
        {
            unsafe
            {
                char* buffer = stackalloc char[260];
                builder.SetStartPointer(buffer, 260);
            }
        }

        // Add parent segments for segments past the common on the "from" path
        if (commonLength < relativeToLength)
        {
            builder.Append("..");

            for (int i = commonLength + 1; i < relativeToLength; i++)
            {
                if (IsDirectorySeparator(relativeTo[i]))
                {
                    builder.Append(Path.DirectorySeparatorChar);
                    builder.Append("..");
                }
            }
        }
        else if (IsDirectorySeparator(path[commonLength]))
        {
            // No parent segments and we need to eat the initial separator
            //  (C:\Foo C:\Foo\Bar case)
            commonLength++;
        }

        // Now add the rest of the "to" path, adding back the trailing separator
        int differenceLength = pathLength - commonLength;
        if (pathEndsInSeparator)
            differenceLength++;

        if (differenceLength > 0)
        {
            if (builder.Length > 0)
            {
                builder.Append(Path.DirectorySeparatorChar);
            }

            builder.Append(path, commonLength, differenceLength);
        }

        return builder.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPartiallyQualified(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        IPlatformImpl impl = _impl;
        return impl switch
        {
            WindowsImpl => WindowsImpl.IsPartiallyQualified(path),
            UnixImpl => UnixImpl.IsPartiallyQualified(path),
            _ => impl.IsPartiallyQualified(path),
        };
    }
}
#endif