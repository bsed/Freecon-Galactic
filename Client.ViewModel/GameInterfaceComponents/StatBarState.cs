namespace Freecon.Client.ViewModel.GameInterfaceComponents
{


    public class StatBarState
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public float Percentage { get; set; }
        public string Text { get; set; }

        public StatBarState(string name, string color, float percentage, string text)
        {
            Name = name;
            Color = color;
            Percentage = percentage;
            Text = text;
        }
    }
}
