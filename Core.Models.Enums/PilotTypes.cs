namespace Freecon.Models.TypeEnums
{
    //This needs to be fully implemented
    public enum PilotTypes : byte
    {
        Player,
        NPC,
        Simulator,//Not implemented yet, TODO: use when players log off to simulate simple physics (gravity)
    }
}
