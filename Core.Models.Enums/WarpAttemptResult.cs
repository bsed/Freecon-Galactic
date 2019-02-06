namespace Core.Models.Enums
{
    //TODO: Find a better place for this
    public enum WarpAttemptResult : byte
    {
        NotConnectedArea,//bug, or hacking?
        NotEnoughEnergy,
        NotNearWarp,//hacking?
        StillInWarpCooldown,//Consecutive warps before cooldown expires
        CurrentAreaNotFound,
        DestinationAreaIDMismatch,
        Trading,


        Success
    }
}
