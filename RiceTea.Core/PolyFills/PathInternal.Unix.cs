#if NET472_OR_GREATER
using RiceTea.Core.Helpers;

namespace System.IO;

partial class PathInternal
{
    // Copied from https://github.com/dotnet/runtime/blob/main/src/libraries/Common/src/System/IO/PathInternal.Unix.cs
    private sealed class UnixImpl : IPlatformImpl
    {
        public UnixImpl()
        {
        }

        public static bool IsEffectivelyEmpty(string path)
            => StringHelper.IsNullOrWhiteSpace(path);

        public static int GetRootLength(string path)
            => path.Length > 0 && IsDirectorySeparator(path[0]) ? 1 : 0;

        public static StringComparison GetPathComparisonMode() => StringComparison.Ordinal;

        public static bool IsPartiallyQualified(string path)
        {
            // This is much simpler than Windows where paths can be rooted, but not fully qualified (such as Drive Relative)
            // As long as the path is rooted in Unix it doesn't use the current directory and therefore is fully qualified.
            return !Path.IsPathRooted(path);
        }

        bool IPlatformImpl.IsEffectivelyEmpty(string path) => IsEffectivelyEmpty(path);

        int IPlatformImpl.GetRootLength(string path) => GetRootLength(path);

        StringComparison IPlatformImpl.GetPathComparisonMode() => GetPathComparisonMode();

        bool IPlatformImpl.IsPartiallyQualified(string path) => IsPartiallyQualified(path);
    }
}
#endif