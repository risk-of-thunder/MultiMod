﻿namespace MultiMod
{
    /// <summary>
    ///     Represents a load state.
    /// </summary>
    public enum ResourceLoadState
    {
        Unloaded,
        Loading,
        Loaded,
        Cancelling,
        Unloading
    }
}