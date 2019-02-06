namespace Core.Models.Enums
{
    /// <summary>
    /// 0 is top, 1000 is bottom. To use, divide by 1000 and convert to float when calling spritebatch.draw
    /// Keep this list in order
    /// </summary>
    public enum DrawDepths : int
    {
        PlayerShip = 0,


        NetworkShip = 1,


        Projectile = 100,


        Structure = 200,


        



    }
}
