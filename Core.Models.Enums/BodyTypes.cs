namespace Freecon.Models.TypeEnums
{
    //This might belong in Client.Models. Here for now, move if you concur.
    public enum BodyTypes : uint
    {
        Temp,
        RightWall,//Ground to the left, 10
        LeftWall,//Ground to the right, 01
        TopWall,//Ground to the bottom
        BottomWall,//Ground to the top
        PlayerShip,
        NetworkShip,
        Planet,
        Sun,
        Port,
        Missile,
        MissileSplash,
        DefensiveMine,
        Projectile,
        WallTile,
        WarpHole,
        Mothership,
        Moon,
        Orb,
        WallEdge,
        VerticalBorder,
        HorizontalBorder,
        Structure,
        Turret,
        Ship,
        CommandCenter,
        FloatyAreaObject,
    }
}
