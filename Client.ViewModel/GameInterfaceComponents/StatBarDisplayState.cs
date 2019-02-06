namespace Freecon.Client.ViewModel.GameInterfaceComponents
{
    public class StatBarDisplayState
    {
        public StatBarGroupState Ship { get; set; }

        public StatBarGroupState Weapons { get; set; }

        public StatBarDisplayState(StatBarGroupState ship, StatBarGroupState weapons)
        {
            Ship = ship;
            Weapons = weapons;
        }
    }
}
