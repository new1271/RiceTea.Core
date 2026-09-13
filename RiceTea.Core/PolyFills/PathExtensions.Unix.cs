#if NET472_OR_GREATER
namespace System.IO;

partial class PathExtensions
{
    // Copied from https://github.com/dotnet/runtime/blob/main/src/libraries/Common/src/System/IO/PathInternal.Unix.cs
    private sealed class UnixImpl : IPlatformImpl
    {
        public unsafe bool IsEffectivelyEmpty(string path)
            => string.IsNullOrWhiteSpace(path);

        public int GetRootLength(string path)
            => path.Length > 0 && IsDirectorySeparator(path[0]) ? 1 : 0;

        public StringComparison GetPathComparisonMode() => StringComparison.Ordinal;

        public bool IsPartiallyQualified(string path)
        {
            // This is much simpler than Windows where paths can be rooted, but not fully qualified (such as Drive Relative)
            // As long as the path is rooted in Unix it doesn't use the current directory and therefore is fully qualified.
            return !Path.IsPathRooted(path);
        }
    }
}
#endif