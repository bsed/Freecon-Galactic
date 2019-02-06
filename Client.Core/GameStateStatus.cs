namespace Freecon.Client.Core.States
{
    public enum GameStateStatus:byte
    {        
        Activating = 1,
        ReadyToActivate = 2,
        Active = 3,
                
        Deactivating = 4,
        ReadyToDeactivate = 5,
        Inactive = 0,//Apparently the default only works if all enums have an explicit value. Otherwise defaults to top of the list. Good to know.
    }
}
