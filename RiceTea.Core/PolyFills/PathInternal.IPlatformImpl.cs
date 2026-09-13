#if NET472_OR_GREATER
namespace System.IO;

partial class PathInternal
{
    private interface IPlatformImpl
    {
        /// <summary>
        /// Returns true if the path is effectively empty for the current OS.
        /// For unix, this is empty or null. For Windows, this is empty, null, or
        /// just spaces ((char)32).
        /// </summary>
        bool IsEffectivelyEmpty(string path);

        /// <summary>
        /// Gets the length of the root of the path
        /// </summary>
        int GetRootLength(string path);

        /// <summary>
        /// Gets the length of the root of the path
        /// </summary>
        bool IsPartiallyQualified(string path);

        StringComparison GetPathComparisonMode();
    }
}
#endif
