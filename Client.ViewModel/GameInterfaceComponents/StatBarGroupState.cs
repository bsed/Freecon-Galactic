using System.Collections.Generic;

namespace Freecon.Client.ViewModel.GameInterfaceComponents
{
    public class StatBarGroupState
    {
        public string Name { get; set; }

        public IList<StatBarState> Stats { get; set; }

        public StatBarGroupState(string name, IList<StatBarState> stats)
        {
            Name = name;
            Stats = stats;
        }
    }
}
