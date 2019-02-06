namespace Freecon.Client.Objects.Pilots
{
    public enum LogicStates : byte
    {
        
        HoldingPosition,
        Patroling,
        MovingToPosition,
        AttackingAll,//Attacking everything in the system
        Roaming,//Just flying around doing random NPC stuff
        Stopped,//Do nothing until attacked or commanded
        EnterObject,//Use for things like warping
        AttackingTarget,
        AttackingToPosition,//Attack all enemies en route to position
        HoldPosAggressively,//Holds position, attacking enemies which get in range
    }
}