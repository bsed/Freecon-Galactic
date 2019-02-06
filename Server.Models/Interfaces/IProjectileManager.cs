namespace Server.Models.Interfaces
{
    public interface IProjectileManager
    {
        int CreateProjectile(ICanFire firingObj, int projectileID);

    }
}
