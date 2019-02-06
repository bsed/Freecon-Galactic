using System;

namespace RedisWrapper
{
    /// <summary>
    /// Behaviour markers associated with a given command.
    /// </summary>
    [Flags]
    public enum RedisCommandFlags
    {
        None = 0,
        HighPriority = 1,
        FireAndForget = 2,
        PreferMaster = 0,
        DemandMaster = 4,
        PreferSlave = 8,
        DemandSlave = PreferSlave | DemandMaster,
        NoRedirect = 64,
    }

    /// <summary>
    /// Indicates when this operation should be performed (only some variations are legal in a given context).
    /// </summary>
    public enum SetWhen
    {
        Always,
        Exists,
        NotExists,
    }
}
