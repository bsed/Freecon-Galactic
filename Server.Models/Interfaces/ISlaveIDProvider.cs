namespace Server.Models.Interfaces
{
    /// <summary>
    /// Returns the current slave id, must be consistent for a given server instance.
    /// </summary>
    public interface ISlaveIDProvider
    {
        int? SlaveID { get; }
    }
}
