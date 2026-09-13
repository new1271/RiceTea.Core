#if NET472_OR_GREATER
using InlineMethod;

using RiceTea.Core.Helpers;

namespace System.IO;

partial class PathExtensions
{
    // Copied from https://github.com/dotnet/runtime/blob/main/src/libraries/Common/src/System/IO/PathInternal.Windows.cs
    private sealed class WindowsImpl : IPlatformImpl
    {
        private const int MaxShortPath = 260;
        private const int MaxShortDirectoryPath = 248;
        // \\?\, \\.\, \??\
        private const int DevicePrefixLength = 4;
        // \\
        private const int UncPrefixLength = 2;
        // \\?\UNC\, \\.\UNC\
        private const int UncExtendedPrefixLength = 8;
        private const char VolumeSeparatorChar = ':';

        public unsafe bool IsEffectivelyEmpty(string path)
        {
            fixed (char* ptr = path)
            {
                if (ptr[0] == '\0') //empty
                    return true;
                char* iterator = ptr;
                for (char c = *iterator; c != '\0'; iterator++)
                {
                    if (c != ' ')
                        return false;
                }
                return true;
            }
        }

        public int GetRootLength(string path)
        {
            int pathLength = path.Length;
            int i = 0;

            bool deviceSyntax = IsDevice(path);
            bool deviceUnc = deviceSyntax && IsDeviceUNC(path);

            if ((!deviceSyntax || deviceUnc) && pathLength > 0 && IsDirectorySeparator(path[0]))
            {
                // UNC or simple rooted path (e.g. "\foo", NOT "\\?\C:\foo")
                if (deviceUnc || (pathLength > 1 && IsDirectorySeparator(path[1])))
                {
                    // UNC (\\?\UNC\ or \\), scan past server\share

                    // Start past the prefix ("\\" or "\\?\UNC\")
                    i = deviceUnc ? UncExtendedPrefixLength : UncPrefixLength;

                    // Skip two separators at most
                    int n = 2;
                    while (i < pathLength && (!IsDirectorySeparator(path[i]) || --n > 0))
                        i++;
                }
                else
                {
                    // Current drive rooted (e.g. "\foo")
                    i = 1;
                }
            }
            else if (deviceSyntax)
            {
                // Device path (e.g. "\\?\.", "\\.\")
                // Skip any characters following the prefix that aren't a separator
                i = DevicePrefixLength;
                while (i < pathLength && !IsDirectorySeparator(path[i]))
                    i++;

                // If there is another separator take it, as long as we have had at least one
                // non-separator after the prefix (e.g. don't take "\\?\\", but take "\\?\a\")
                if (i < pathLength && i > DevicePrefixLength && IsDirectorySeparator(path[i]))
                    i++;
            }
            else if (pathLength >= 2
                && path[1] == Path.VolumeSeparatorChar
                && IsValidDriveChar(path[0]))
            {
                // Valid drive specified path ("C:", "D:", etc.)
                i = 2;

                // If the colon is followed by a directory separator, move past it (e.g "C:\")
                if (pathLength > 2 && IsDirectorySeparator(path[2]))
                    i++;
            }

            return i;
        }

        public StringComparison GetPathComparisonMode() => StringComparison.OrdinalIgnoreCase;

        /// <summary>
        /// Returns true if the given character is a valid drive letter
        /// </summary>
        [Inline(InlineBehavior.Remove)]
        private static bool IsValidDriveChar(char value)
        {
            return (uint)((value | 0x20) - 'a') <= (uint)('z' - 'a');
        }

        /// <summary>
        /// Returns true if the path uses any of the DOS device path syntaxes. ("\\.\", "\\?\", or "\??\")
        /// </summary>
        [Inline(InlineBehavior.Remove)]
        private static bool IsDevice(string path)
        {
            // If the path begins with any two separators is will be recognized and normalized and prepped with
            // "\??\" for internal usage correctly. "\??\" is recognized and handled, "/??/" is not.
            return IsExtended(path)
                ||
                (
                    path.Length >= DevicePrefixLength
                    && IsDirectorySeparator(path[0])
                    && IsDirectorySeparator(path[1])
                    && (path[2] == '.' || path[2] == '?')
                    && IsDirectorySeparator(path[3])
                );
        }

        /// <summary>
        /// Returns true if the path uses the canonical form of extended syntax ("\\?\" or "\??\"). If the
        /// path matches exactly (cannot use alternate directory separators) Windows will skip normalization
        /// and path length checks.
        /// </summary>
        [Inline(InlineBehavior.Remove)]
        private static bool IsExtended(string path)
        {
            // While paths like "//?/C:/" will work, they're treated the same as "\\.\" paths.
            // Skipping of normalization will *only* occur if back slashes ('\') are used.
            return path.Length >= DevicePrefixLength
                && path[0] == '\\'
                && (path[1] == '\\' || path[1] == '?')
                && path[2] == '?'
                && path[3] == '\\';
        }

        /// <summary>
        /// Returns true if the path is a device UNC (\\?\UNC\, \\.\UNC\)
        /// </summary>
        [Inline(InlineBehavior.Remove)]
        private static bool IsDeviceUNC(string path)
        {
            return path.Length >= UncExtendedPrefixLength
                && IsDevice(path)
                && IsDirectorySeparator(path[7])
                && path[4] == 'U'
                && path[5] == 'N'
                && path[6] == 'C';
        }

        /// <summary>
        /// Returns true if the two paths have the same root
        /// </summary>
        [Inline(InlineBehavior.Remove)]
        private bool AreRootsEqual(string first, string second)
        {
            int firstRootLength = GetRootLength(first);
            int secondRootLength = GetRootLength(second);

            return firstRootLength == secondRootLength
                && string.Compare(
                    strA: first,
                    indexA: 0,
                    strB: second,
                    indexB: 0,
                    length: firstRootLength,
                    comparisonType: StringComparison.OrdinalIgnoreCase) == 0;
        }

        /// <summary>
        /// Returns true if the path specified is relative to the current drive or working directory.
        /// Returns false if the path is fixed to a specific drive or UNC path.  This method does no
        /// validation of the path (URIs will be returned as relative as a result).
        /// </summary>
        /// <remarks>
        /// Handles paths that use the alternate directory separator.  It is a frequent mistake to
        /// assume that rooted paths (Path.IsPathRooted) are not relative.  This isn't the case.
        /// "C:a" is drive relative- meaning that it will be resolved against the current directory
        /// for C: (rooted, but relative). "C:\a" is rooted and not relative (the current directory
        /// will not be used to modify the path).
        /// </remarks>
        public bool IsPartiallyQualified(string path)
        {
            if (path.Length < 2)
            {
                // It isn't fixed, it must be relative.  There is no way to specify a fixed
                // path with one character (or less).
                return true;
            }

            ref readonly char reference = ref UnsafeHelper.GetStringDataReference(path);

            if (IsDirectorySeparator(reference))
            {
                char secondChar = UnsafeHelper.AddTypedOffsetAsReadOnly(in reference, 1);
                // There is no valid way to specify a relative path with two initial slashes or
                // \? as ? isn't valid for drive relative paths and \??\ is equivalent to \\?\
                return !(secondChar == '?' || IsDirectorySeparator(secondChar));
            }

            // The only way to specify a fixed path that doesn't begin with two slashes
            // is the drive, colon, slash format- i.e. C:\
            return !((path.Length >= 3)
                && (UnsafeHelper.AddTypedOffsetAsReadOnly(in reference, 1) == VolumeSeparatorChar)
                && IsDirectorySeparator(UnsafeHelper.AddTypedOffsetAsReadOnly(in reference, 2))
                // To match old behavior we'll check the drive character for validity as the path is technically
                // not qualified if you don't have a valid drive. "=:\" is the "=" file's default data stream.
                && IsValidDriveChar(reference));
        }
    }
}
#endif