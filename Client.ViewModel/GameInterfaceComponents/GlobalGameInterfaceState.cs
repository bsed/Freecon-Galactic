namespace Freecon.Client.ViewModel.GameInterfaceComponents
{
    public class GlobalGameInterfaceState
    {
        public StatBarDisplayState StatBars { get; set; }

        public GlobalGameInterfaceState(StatBarDisplayState statBarDisplayState)
        {
            StatBars = statBarDisplayState;
        }
    }
}
