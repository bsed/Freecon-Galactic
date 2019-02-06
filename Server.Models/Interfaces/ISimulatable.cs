namespace Server.Models.Interfaces
{
    /// <summary>
    /// Anything that can be simulated by a client (NPCs, Turrets) gets this interface
    /// </summary>
    public interface ISimulatable
    {
        int Id { get; }
    }
}
