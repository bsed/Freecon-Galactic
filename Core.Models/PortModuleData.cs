namespace Freecon.Models
{
    ///// <summary>
    ///// Class that should contain all the information to be displayed about a module
    ///// </summary>
    //public class PortModuleData
    //{
    //    public ModuleTypes ModuleType;

    //    public ModifierType ModifierType;

    //    public float ModifierValue;

    //    public int Level;

    //    public float PurchasePrice;

    //    public int Id;//GalaxyID, used to purchase the module
    //}

    public enum ModifierType:byte
    {
        Additive,
        Multiplicative,
        Special

    }
}
