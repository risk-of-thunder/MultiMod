using System;
using System.IO;

namespace MultiMod.Shared
{
    /// <summary>
    ///     Extension methods for enums.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        ///     Unity's enum mask fields set all bits to 1. This sets all unused bits to 0, so it can be converted to a string and
        ///     serialized properly.
        /// </summary>
        /// <param name="self">An enum instance.</param>
        /// <returns>A fixed enum.</returns>
        public static int FixEnum(this Enum self)
        {
            var bits = 0;
            foreach (var enumValue in Enum.GetValues(self.GetType()))
            {
                var checkBit = Convert.ToInt32(self) & (int) enumValue;
                if (checkBit != 0) bits |= (int) enumValue;
            }

            return bits;
        }
    }

    /// <summary>
    ///     Extension methods for strings.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        ///     Is this path a sub directory or the same directory of another path?
        /// </summary>
        /// <param name="self">A string.</param>
        /// <param name="other">A path.</param>
        /// <returns>True if this string is a sub directory or the same directory as the other.</returns>
        public static bool IsDirectoryOrSubdirectory(this string self, string other)
        {
            var isChild = false;
            try
            {
                var candidateInfo = new DirectoryInfo(self);
                var otherInfo = new DirectoryInfo(other);

                if (candidateInfo.FullName == otherInfo.FullName)
                    return true;

                while (candidateInfo.Parent != null)
                    if (candidateInfo.Parent.FullName == otherInfo.FullName)
                    {
                        isChild = true;
                        break;
                    }
                    else
                    {
                        candidateInfo = candidateInfo.Parent;
                    }
            }
            catch (Exception e)
            {
                var message = string.Format("Unable to check directories {0} and {1}: {2}", self, other, e);
                LogUtility.LogWarning(message);
            }

            return isChild;
        }

        /// <summary>
        ///     Returns a normalized version of a path.
        /// </summary>
        /// <param name="self">A string.</param>
        /// <returns>A normalized version of a path.</returns>
        public static string NormalizedPath(this string self)
        {
            var normalizedPath = Path.GetFullPath(new Uri(self).LocalPath);
            normalizedPath = normalizedPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            normalizedPath = normalizedPath.ToLowerInvariant();
            return normalizedPath;
        }
    }
}