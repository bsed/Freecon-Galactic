namespace Server.Models.Interfaces
{
    public interface ICanFire: ITeamable
    {
        int Id { get; }
        Weapon GetWeapon(int slot);
    
    }
}
