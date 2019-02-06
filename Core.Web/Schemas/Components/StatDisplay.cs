namespace Core.Web.Schemas.Components
{
    /// <summary>
    /// A generic display to show things like Population or Morale.
    /// </summary>
    public class StatDisplay
    {
        public string DisplayName { get; set; }

        public string Tooltip { get; set; }

        public float CurrentValue { get; set; }

        public float MaxValue { get; set; }

        public float RateOfChange { get; set; }

        public StatDisplayTypes Type { get; set; }

        public TimeUnits TimeUnit { get; set; }
    }
}
