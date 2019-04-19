using System;

namespace MultiMod.Shared
{
    /// <summary>
    ///     Represents a platform or a combination of platforms.
    /// </summary>
    [Flags]
    [Serializable]
    public enum ModPlatform
    {
        Windows = 1,
        Linux = 2,
        OSX = 4,
        Android = 8
    }
}